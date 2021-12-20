//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using System.Collections.Generic;

    public class NewContainerStateMachine : SimplifiedBaseBusinessLogic<IVoiceLinkModel, VoiceLinkStateMachine, IVoiceLinkConfigRepository>
    {
        public static readonly CoreAppSMState StartNewContainer = new CoreAppSMState(nameof(StartNewContainer));
        public static readonly CoreAppSMState DisplayConfirmNewContainer = new CoreAppSMState(nameof(DisplayConfirmNewContainer));
        public static readonly CoreAppSMState HandleConfirmNewContainerResponse = new CoreAppSMState(nameof(HandleConfirmNewContainerResponse));
        public static readonly CoreAppSMState DisplayConfirmCloseCurrentContainer = new CoreAppSMState(nameof(DisplayConfirmCloseCurrentContainer));
        public static readonly CoreAppSMState HandleConfirmCloseCurrentContainerResponse = new CoreAppSMState(nameof(HandleConfirmCloseCurrentContainerResponse));
        public static readonly CoreAppSMState NewContainerCloseContainer = new CoreAppSMState(nameof(NewContainerCloseContainer));
        public static readonly CoreAppSMState NewContainerOpenContainer = new CoreAppSMState(nameof(NewContainerOpenContainer));
        public static readonly CoreAppSMState ReturnAfterDelivery = new CoreAppSMState(nameof(ReturnAfterDelivery));

        private OpenContainerStateMachine _OpenContainerSM;
        private OpenContainerStateMachine OpenContainerSM { get { return Manager.CreateStateMachine(ref _OpenContainerSM); } }
        private CloseContainerStateMachine _CloseContainerSM;
        private CloseContainerStateMachine CloseContainerSM { get { return Manager.CreateStateMachine(ref _CloseContainerSM); } }

        private PickingRegion _PickingRegion;
        private Assignment _Assignment;
        private List<Pick> _Picks;
        private Container _Container;
        private bool _MultipleAssignments;
        private bool _ContainerClosed;
        private bool? _ConfirmNewContainerResponse;
        private bool? _ConfirmCloseContainerResponse;
        public NewContainerStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            ConfigureReturnLogicState(StartNewContainer, () =>
            {
                if (_Picks[0].TargetContainer > 0)
                {
                    CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_Selection_NewContainer_NotAllowed");
                    return;
                }

                NextState = DisplayConfirmNewContainer;
            }, DisplayConfirmNewContainer);

            ConfigureDisplayState(DisplayConfirmNewContainer, HandleConfirmNewContainerResponse, encodeAction: EncodeConfirmNewContainer, decodeAction: DecodeConfirmNewContainer);

            ConfigureReturnLogicState(NewContainerOpenContainer, () =>
            {
                if (_ConfirmNewContainerResponse ?? false)
                {
                    if (_PickingRegion.AllowMultipleOpenContainers)
                    {
                        if (!ContainersResponse.MultipleOpenContainers(_Assignment.AssignmentID))
                        {
                            NextState = DisplayConfirmCloseCurrentContainer;
                        }
                        else
                        {
                            NextState = NewContainerOpenContainer;
                        }
                    }
                }
            }, DisplayConfirmCloseCurrentContainer, NewContainerOpenContainer);

            ConfigureDisplayState(DisplayConfirmCloseCurrentContainer, HandleConfirmCloseCurrentContainerResponse, encodeAction: EncodeConfirmCloseContainer, decodeAction: DecodeConfirmCloseContainer);

            ConfigureLogicState(HandleConfirmCloseCurrentContainerResponse, () =>
            {

                NextState = NewContainerCloseContainer;
                if (_ConfirmCloseContainerResponse ?? false)
                {
                    NextState = NewContainerOpenContainer;
                }
            }, NewContainerOpenContainer, NewContainerCloseContainer);

            ConfigureLogicState(NewContainerOpenContainer, async () =>
            {
                _ContainerClosed = false;

                CloseContainerSM.Reset();
                CloseContainerSM.InitProperties(_PickingRegion, _Assignment, _Picks, _Container, _MultipleAssignments, false);
                await CloseContainerSM.InitializeStateMachineAsync();

                NextState = NewContainerOpenContainer;
            }, NewContainerOpenContainer);

            ConfigureLogicState(NewContainerCloseContainer, async () =>
            {
                OpenContainerSM.Reset();
                OpenContainerSM.InitProperties(_PickingRegion, _Assignment, _Picks, _MultipleAssignments);
                await OpenContainerSM.InitializeStateMachineAsync();

                NextState = ReturnAfterDelivery;
            }, ReturnAfterDelivery);

            ConfigureReturnLogicState(ReturnAfterDelivery, () =>
            {
                if (_PickingRegion.DeliveryType != 2 && _PickingRegion.DeliverContainerClosed)
                {
                    Model.ResetAisleDirections();
                    NextState = PickAssignmentStateMachine.CheckNextPick;
                }
                else if (_PickingRegion.PrintContainerLabels == 1 || _PickingRegion.PrintContainerLabels == 2 && _ContainerClosed)
                {
                    NextState = PickPromptStateMachine.SlotVerification;
                }
            }, externalDestinationStates: new List<CoreAppSMState> { PickAssignmentStateMachine.CheckNextPick, PickPromptStateMachine.SlotVerification });
        }

        #region EncodersDecoders
        private WorkflowObjectContainer EncodeConfirmNewContainer(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateBooleanIntent(Translate.GetLocalizedTextForKey("VoiceLink_NewContainer_ConfirmNewContainer_Header"),
                                                                "ecq",
                                                                Translate.GetLocalizedTextForKey("VoiceLink_NewContainer_ConfirmNewContainer_Prompt"),
                                                                model.CurrentUserMessage,
                                                                initialPrompt: model.CurrentUserMessage,
                                                                affirmativeVocab: VoiceLinkModuleVocab.Yes,
                                                                negativeVocab: VoiceLinkModuleVocab.No);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeConfirmNewContainer(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            _ConfirmNewContainerResponse = GenericBaseEncoder<IVoiceLinkModel>.DecodeBooleanPrompt(slotContainer);
        }

        private WorkflowObjectContainer EncodeConfirmCloseContainer(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateBooleanIntent(Translate.GetLocalizedTextForKey("VoiceLink_NewContainer_ConfirmCloseCurrentContainer_Header"),
                                                                       "ecq",
                                                                       Translate.GetLocalizedTextForKey("VoiceLink_NewContainer_ConfirmCloseCurrentContainer_Prompt"),
                                                                       model.CurrentUserMessage,
                                                                       initialPrompt: model.CurrentUserMessage,
                                                                       affirmativeVocab: VoiceLinkModuleVocab.Yes,
                                                                       negativeVocab: VoiceLinkModuleVocab.No);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeConfirmCloseContainer(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            _ConfirmCloseContainerResponse = GenericBaseEncoder<IVoiceLinkModel>.DecodeBooleanPrompt(slotContainer);
        }
        #endregion

        public void InitProperties(PickingRegion pickingRegion, Assignment assignment, List<Pick> picks, Container container, bool multipleAssignments)
        {
            this._PickingRegion = pickingRegion;
            this._Assignment = assignment;
            this._Picks = picks;
            this._Container = container;
            this._MultipleAssignments = multipleAssignments;
        }

        public override void Reset()
        {
            NextTrigger = null;
            _ContainerClosed = false;

            CloseContainerSM.Reset();
            OpenContainerSM.Reset();
        }
    }
}
