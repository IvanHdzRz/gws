//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class CloseContainerStateMachine : SimplifiedBaseBusinessLogic<IVoiceLinkModel, VoiceLinkStateMachine, IVoiceLinkConfigRepository>
    {
        public static readonly CoreAppSMState StartCloseContainer = new CoreAppSMState(nameof(StartCloseContainer));
        public static readonly CoreAppSMState VerifyCloseContainer = new CoreAppSMState(nameof(VerifyCloseContainer));
        public static readonly CoreAppSMState DisplayCloseContainerNotAllowed = new CoreAppSMState(nameof(DisplayCloseContainerNotAllowed));
        public static readonly CoreAppSMState DisplayCloseContainerPrompt = new CoreAppSMState(nameof(DisplayCloseContainerPrompt));
        public static readonly CoreAppSMState HandleCloseContainerResponse = new CoreAppSMState(nameof(HandleCloseContainerResponse));
        public static readonly CoreAppSMState CommCloseContainer = new CoreAppSMState(nameof(CommCloseContainer));
        public static readonly CoreAppSMState CloseContainerPrintLabel = new CoreAppSMState(nameof(CloseContainerPrintLabel));
        public static readonly CoreAppSMState Deliver = new CoreAppSMState(nameof(Deliver));
        public static readonly CoreAppSMState ReturnAfterDeliver = new CoreAppSMState(nameof(ReturnAfterDeliver));

        private PickingRegion _PickingRegion;
        private Assignment _Assignment;
        private List<Pick> _Picks;
        private Container _Container;
        private bool _MultipleAssignments;
        private bool _CloseContainerCmd;
        private string _CloseContainerResponse;

        public CloseContainerStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            ConfigureLogicState(StartCloseContainer, () =>
            {
                if (_CloseContainerCmd)
                {
                    NextState = DisplayCloseContainerNotAllowed;
                    if (_PickingRegion.ContainerType != 0
                        && !_MultipleAssignments
                        && _Picks[0].TargetContainer == 0
                        && ContainersResponse.HasContainersWithStatus(_Assignment.AssignmentID, "O")
                        && ContainersResponse.MultipleOpenContainers(_Assignment.AssignmentID))
                    {
                        NextState = DisplayCloseContainerPrompt;
                    }
                }
                else
                {
                    NextState = CommCloseContainer;
                }
            }, DisplayCloseContainerNotAllowed, DisplayCloseContainerPrompt, CommCloseContainer);

            ConfigureDisplayState(DisplayCloseContainerNotAllowed, CoreAppStates.Standby, returnInputAction: () => Manager.ExecuteReturn(),
                encodeAction: EncodeCloseContainerNotAllowed);

            ConfigureDisplayState(DisplayCloseContainerPrompt, HandleCloseContainerResponse, followedByBackgroundActivity: true, backgroundActivityHeaderKey: "VoiceLink_CLoseContainer_Header",
                encodeAction: EncodeCloseContainerPrompt, decodeAction: DecodeCloseContainerPrompt);

            ConfigureLogicState(HandleCloseContainerResponse, () =>
            {
                var validContainer = ContainersResponse.ValidContainerForAssignment(_Assignment.AssignmentID, _CloseContainerResponse);

                NextState = CommCloseContainer;
                if (!validContainer)
                {
                    CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_CloseContainer_NotValid", string.Join(" ", _CloseContainerResponse));
                    NextState = DisplayCloseContainerPrompt;
                }
                else
                {
                    _Container = ContainersResponse.GetMatchingContainer(_CloseContainerResponse);
                    if (_Container.ContainerStatus == "C")
                    {
                        CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_CloseContainer_AlreadyClosed", string.Join(" ", _CloseContainerResponse));
                        NextState = DisplayCloseContainerPrompt;
                    }
                }

            }, DisplayCloseContainerPrompt, CommCloseContainer);

            ConfigureLogicState(CommCloseContainer, async () =>
            {
                NextState = CloseContainerPrintLabel;
                if (_Container != null)
                {
                    await Model.LUTtransmit(LutType.GetContainers, "VoiceLink_BackgroundActivity_Header_Closing_Container",
                        parameters: new GetContainersParam(_Assignment, null, _Container.ContainerID, _CloseContainerResponse, 1, null),
                        goToStateIfFail: DisplayCloseContainerPrompt
                    );
                }
            }, CloseContainerPrintLabel);

            ConfigureLogicState(CloseContainerPrintLabel, async () =>
            {
                if (_PickingRegion.PrintContainerLabels == 2 && _Container.Printed == 0)
                {
                    // TODO: execute print label state machine when supported
                    await Task.CompletedTask;
                }
                NextState = Deliver;
            }, Deliver);

            ConfigureLogicState(Deliver, async () =>
            {
                if (_PickingRegion.DeliveryType != 2 && _PickingRegion.DeliverContainerClosed)
                {
                    // TODO: execute deliver state machine when supported
                    await Task.CompletedTask;
                }
                NextState = ReturnAfterDeliver;
            }, ReturnAfterDeliver);

            ConfigureReturnLogicState(ReturnAfterDeliver, () =>
            {
                if (_PickingRegion.DeliveryType != 2 && _PickingRegion.DeliverContainerClosed)
                {
                    Model.ResetAisleDirections();
                    NextState = PickAssignmentStateMachine.CheckNextPick;
                }
            }, externalDestinationStates: new List<CoreAppSMState> { PickAssignmentStateMachine.CheckNextPick });
        }

        #region EncodersDecoders
        private WorkflowObjectContainer EncodeCloseContainerNotAllowed(IVoiceLinkModel model)
        {
            var prompt = Translate.GetLocalizedTextForKey("VoiceLink_CloseContainer_NotAllowed");
            if (_PickingRegion.ContainerType != 0)
            {
                if (_MultipleAssignments)
                {
                    prompt = Translate.GetLocalizedTextForKey("VoiceLink_CloseContainer_NotAllowed_MultipleAssignments");
                }
                else if (_Picks[0].TargetContainer > 0)
                {
                    prompt = Translate.GetLocalizedTextForKey("VoiceLink_CloseContainer_NotAllowed_TargetContainers");
                }
                else if (ContainersResponse.HasContainersWithStatus(_Assignment.AssignmentID, "O"))
                {
                    if (!ContainersResponse.MultipleOpenContainers(_Assignment.AssignmentID))
                    {
                        prompt = Translate.GetLocalizedTextForKey("VoiceLink_CloseContainer_NotAllowed_OnlyContainer");
                    }
                }
            }

            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("VoiceLink_CloseContainer_CloseNotAllowed_Header"),
                                                              "closeNotAllowedReady",
                                                              prompt,
                                                              model.CurrentUserMessage,
                                                              navigateBackEnabled: false,
                                                              initialPrompt: model.CurrentUserMessage);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private WorkflowObjectContainer EncodeCloseContainerPrompt(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("VoiceLink_CloseContainer_Container_Header"),
                                                                 "closeContainer",
                                                                 Translate.GetLocalizedTextForKey("VoiceLink_CloseContainer_Container_Label"),
                                                                 Translate.GetLocalizedTextForKey("VoiceLink_CloseContainer_Container_Prompt"),
                                                                 null,
                                                                 null,
                                                                 model.CurrentUserMessage,
                                                                 initialPrompt: model.CurrentUserMessage);

            wfo.ValueProperties.AllowedCharacters = CharacterSet.AlphaNumeric;
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeCloseContainerPrompt(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            _CloseContainerResponse = GenericBaseEncoder<IVoiceLinkModel>.DecodeValueEntry(slotContainer);
        }
        #endregion

        public void InitProperties(PickingRegion pickingRegion, Assignment assignment, List<Pick> picks, Container container, bool multipleAssignments, bool closeContainerCmd)
        {
            this._PickingRegion = pickingRegion;
            this._Assignment = assignment;
            this._Picks = picks;
            this._Container = container;
            this._MultipleAssignments = multipleAssignments;
            this._CloseContainerCmd = closeContainerCmd;
        }

        public override void Reset()
        {
            NextTrigger = null;
        }
    }
}
