//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class OpenContainerStateMachine : SimplifiedBaseBusinessLogic<IVoiceLinkModel, VoiceLinkStateMachine, IVoiceLinkConfigRepository>
    {
        public static readonly CoreAppSMState StartOpenContainer = new CoreAppSMState(nameof(StartOpenContainer));
        public static readonly CoreAppSMState DisplayPromptForContainer = new CoreAppSMState(nameof(DisplayPromptForContainer));
        public static readonly CoreAppSMState HandlePromptForContainerResponse = new CoreAppSMState(nameof(HandlePromptForContainerResponse));
        public static readonly CoreAppSMState DisplayOpenContainerPrompt = new CoreAppSMState(nameof(DisplayOpenContainerPrompt));
        public static readonly CoreAppSMState OpenContainerPrintLabel = new CoreAppSMState(nameof(OpenContainerPrintLabel));
        public static readonly CoreAppSMState AfterGetContainersLut = new CoreAppSMState(nameof(AfterGetContainersLut));


        private PickingRegion _PickingRegion;
        private Assignment _Assignment;
        private bool _MultipleAssignments;
        private List<Pick> _Picks;
        private string _CurrentContainerID;

        int? _Position;
        string _Container;

        public OpenContainerStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            ConfigureLogicState(StartOpenContainer, () =>
            {
                if (_MultipleAssignments)
                {
                    _Position = _Assignment.Position;
                }

                if (_PickingRegion.PromptOpenContainer)
                {
                    NextState = DisplayPromptForContainer;
                }
                else
                {
                    NextState = HandlePromptForContainerResponse;
                }

            }, HandlePromptForContainerResponse, DisplayPromptForContainer);

            ConfigureDisplayState(DisplayPromptForContainer, HandlePromptForContainerResponse, encodeAction: EncodeContainerPrompt, decodeAction: DecodeContainerPrompt);

            ConfigureReturnLogicState(HandlePromptForContainerResponse, async () =>
            {
                if (_PickingRegion.PromptOpenContainer)
                {
                    if (_CurrentContainerID == null)
                    {
                        // Cancel pressed
                        if (PickingRegionsResponse.CurrentPickingRegion.PromptOpenContainer)
                        {
                            NextState = DisplayPromptForContainer;
                        }
                        else
                        {
                            NextState = PickAssignmentStateMachine.DisplayPostAislePrompt;
                        }
                        return;
                    }

                    _Container = _CurrentContainerID;
                }

                int? targetContainer = null;
                if (_Picks[0].TargetContainer != 0)
                {
                    targetContainer = _Picks[0].TargetContainer;
                }

                NextState = AfterGetContainersLut;
                var goToStateIfFail = PickingRegionsResponse.CurrentPickingRegion.PromptOpenContainer ? DisplayPromptForContainer : PickAssignmentStateMachine.DisplayPostAislePrompt;
                await Model.LUTtransmit(LutType.GetContainers, "VoiceLink_BackgroundActivity_Header_Opening_Container",
                    parameters: new GetContainersParam(_Assignment, targetContainer?.ToString(), null, _Container, 2, null),
                    goToStateIfFail: goToStateIfFail
                );
            }, externalDestinationStates: new List<CoreAppSMState> { PickAssignmentStateMachine.DisplayPostAislePrompt }, 
            AfterGetContainersLut, DisplayPromptForContainer);

            ConfigureLogicState(AfterGetContainersLut, () =>
            {
                NextState = OpenContainerPrintLabel;
                if (!_PickingRegion.PromptOpenContainer)
                {
                    if (_Position == null)
                    {
                        if (_Picks[0].TargetContainer != 0)
                        {
                            _Assignment.ActiveContainer = ContainersResponse.CurrentResponse[0].TargetContainer;
                        }
                    }
                    NextState = DisplayOpenContainerPrompt;
                }
            }, DisplayOpenContainerPrompt, OpenContainerPrintLabel);

            ConfigureDisplayState(DisplayOpenContainerPrompt, OpenContainerPrintLabel, encodeAction: EncodeOpenContainerPrompt);

            ConfigureReturnLogicState(OpenContainerPrintLabel, async () =>
            {
                if (_PickingRegion.PrintContainerLabels == 1 &&
                    ContainersResponse.CurrentResponse[0].Printed == 0)
                {
                    // TODO: Implement when printing is supported
                    await Task.CompletedTask;
                }
            });
        }

        #region EncodersDecoders
        private WorkflowObjectContainer EncodeContainerPrompt(IVoiceLinkModel model)
        {
            var prompt = Translate.GetLocalizedTextForKey("VoiceLink_Selection_NewContainer_Prompt", _Position.ToString());
            if (_Position == null)
            {
                prompt = Translate.GetLocalizedTextForKey("VoiceLink_Selection_NewContainer_Prompt_ID");
            }


            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("VoiceLink_OpenContainer_NewContainer_Header"),
                                                                  "newContainer",
                                                                  Translate.GetLocalizedTextForKey("VoiceLink_OpenContainer_NewContainer_Label"),
                                                                  prompt,
                                                                  null,
                                                                  null,
                                                                  model.CurrentUserMessage,
                                                                  initialPrompt: model.CurrentUserMessage);

            wfo.ValueProperties.AllowedCharacters = CharacterSet.AlphaNumeric;
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeContainerPrompt(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            _CurrentContainerID = GenericBaseEncoder<IVoiceLinkModel>.DecodeValueEntry(slotContainer);
        }

        private WorkflowObjectContainer EncodeOpenContainerPrompt(IVoiceLinkModel model)
        {
            var prompt = Translate.GetLocalizedTextForKey("VoiceLink_Selection_OpenContainer_Open_Last", ContainersResponse.CurrentResponse[0].SpokenContainerValidation);
            if (_Position != null)
            {
                var containers = ContainersResponse.GetOpenContainers(_Assignment.AssignmentID);
                if (containers.Count() == 0)
                {
                    containers = ContainersResponse.GetClosedContainers(_Assignment.AssignmentID);
                }
                if (containers.Count() > 0)
                {
                    prompt = Translate.GetLocalizedTextForKey("VoiceLink_Selection_OpenContainer_Open_Last_Multiple", containers.First().SpokenContainerValidation, _Position.ToString());
                }
                else
                {
                    prompt = Translate.GetLocalizedTextForKey("VoiceLink_Selection_OpenContainer_NoContainers");
                }
            }


            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("VoiceLink_OpenContainer_Open_Header"),
                                                              "readyNone",
                                                              prompt,
                                                              model.CurrentUserMessage,
                                                              navigateBackEnabled: false,
                                                              initialPrompt: model.CurrentUserMessage);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }
        #endregion

        public void InitProperties(PickingRegion pickingRegion, Assignment assignment, List<Pick> picks, bool multipleAssignments)
        {
            this._PickingRegion = pickingRegion;
            this._Assignment = assignment;
            this._Picks = picks;
            this._MultipleAssignments = multipleAssignments;
        }

        public override void Reset()
        {
            NextTrigger = null;
            _Position = null;
            _Container = null;
        }

    }
}
