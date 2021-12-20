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

    public class PickAssignmentStateMachine :  SimplifiedBaseBusinessLogic<IVoiceLinkModel, VoiceLinkStateMachine, IVoiceLinkConfigRepository>
    {
        public static readonly CoreAppSMState StartPickAssignment = new CoreAppSMState(nameof(StartPickAssignment));
        public static readonly CoreAppSMState CheckNextPick = new CoreAppSMState(nameof(CheckNextPick));
        public static readonly CoreAppSMState CommVerifyLocationReplenished = new CoreAppSMState(nameof(CommVerifyLocationReplenished));

        public static readonly CoreAppSMState PreAisleCheck = new CoreAppSMState(nameof(PreAisleCheck));
        public static readonly CoreAppSMState DisplayPreAislePrompt = new CoreAppSMState(nameof(DisplayPreAislePrompt));

        public static readonly CoreAppSMState AisleCheck = new CoreAppSMState(nameof(AisleCheck));
        public static readonly CoreAppSMState DisplayAislePrompt = new CoreAppSMState(nameof(DisplayAislePrompt));

        public static readonly CoreAppSMState PostAisleCheck = new CoreAppSMState(nameof(PostAisleCheck));
        public static readonly CoreAppSMState DisplayPostAislePrompt = new CoreAppSMState(nameof(DisplayPostAislePrompt));

        public static readonly CoreAppSMState PickPrompt = new CoreAppSMState(nameof(PickPrompt));
        public static readonly CoreAppSMState CommEndPicking = new CoreAppSMState(nameof(CommEndPicking));
        public static readonly CoreAppSMState AfterReplenishmentTransmitted = new CoreAppSMState(nameof(AfterReplenishmentTransmitted));


        private PickPromptSingleStateMachine _PickPromptSingleSM;
        private PickPromptSingleStateMachine PickPromptSingleSM { get { return Manager.CreateStateMachine(ref _PickPromptSingleSM); } }

        private PickPromptMultipleStateMachine _PickPromptMultipleSM;
        private PickPromptMultipleStateMachine PickPromptMultipleSM { get { return Manager.CreateStateMachine(ref _PickPromptMultipleSM); } }

        public static bool AutoShort { get; private set; }
        public static List<Pick> PickList { get; private set; }

        private string _Status;

        public PickAssignmentStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            ConfigureLogicState(StartPickAssignment, () =>
            {
                _Status = "N";
                if (PicksResponse.CurrentResponse.Where((p, idx) => p.Status == "B").Count() > 0) _Status = "B";

                if (!SelectionStateMachine.PickOnly || !PickingRegionsResponse.CurrentPickingRegion.PickByPick)
                {
                    Model.ResetAisleDirections();
                }

                NextState = CheckNextPick;
            }, CheckNextPick);

            ConfigureLogicState(CheckNextPick, () =>
            {
                // Check for next pick
                AutoShort = false;
                // Holds the complete next pick list at the end
                PickList = new List<Pick>();
                // Holds the first pick found with the right status
                Pick firstPick = null;

                //Holds candidates for matching picks
                List<Pick> candidates = new List<Pick>();

                bool combineAssignments = PickingRegionsResponse.CurrentPickingRegion.ContainerType != 0;

                // Search for a pick with the correct status. Process all picks in the LUT,
                // remembering possible match candidates.
                foreach (var pick in PicksResponse.CurrentResponse)
                {
                    if (pick.Status == _Status && firstPick == null)
                    {
                        firstPick = pick;
                        PickList.Add(pick);
                    }
                    else if (pick.Status != "P")
                    {
                        // This is a match candidate
                        candidates.Add(pick);
                    }
                }

                if (firstPick != null)
                {
                    if (candidates.Count > 0)
                    {
                        foreach (var pick in candidates)
                        {
                            if (firstPick.Matches(pick, combineAssignments))
                            {
                                pick.Status = firstPick.Status;
                                PickList.Add(pick);
                            }
                        }
                    }
                    Model.SetNextPick(PickList);
                    NextState = CommVerifyLocationReplenished;
                }
                else
                {
                    backgroundActivityNextState = CommEndPicking;
                    CurrentBackGroundActivityHeader = Translate.GetLocalizedTextForKey("VoiceLink_Selection_Picking_Checking_Shorts_And_Skips_Header");
                    NextState = CoreAppStates.BackgroundActvity;
                }
            }, CoreAppStates.BackgroundActvity, CommVerifyLocationReplenished);

            //--------------------------------------------------------------------
            //Replenishment slot checks
            ConfigureLogicState(CommVerifyLocationReplenished, async () =>
            {
                NextState = PreAisleCheck;
                if (PickList[0].VerifyLocationFlag)
                {
                    NextState = AfterReplenishmentTransmitted;
                    await Model.LUTtransmit(LutType.Replenishment, "VoiceLink_BackgroundActivity_Header_Verifying_Replen",
                        parameters: new ReplenishmentParam(PickList[0]),
                        goToStateIfFail: SelectionStateMachine.GetAssignment //TODO: 
                    );
                }
            }, AfterReplenishmentTransmitted, PreAisleCheck);

            ConfigureLogicState(AfterReplenishmentTransmitted, () =>
            {
                NextState = PreAisleCheck;
                PickList[0].VerifyLocationFlag = false;
                if (!VerifyReplenishmentResponse.Replenished)
                {
                    // Not replenished
                    AutoShort = true;
                    NextState = PickPrompt;
                }
            }, PickPrompt, PreAisleCheck);

            //--------------------------------------------------------------------
            //Pre-Aisle states
            ConfigureLogicState(PreAisleCheck, () =>
            {
                NextState = AisleCheck;
                if (PickList[0].PreAisleDirection != Model.CurrentPreAisle)
                {
                    if (PickList[0].PreAisleDirection != "")
                    {
                        Model.CurrentPreAisle = PickList[0].PreAisleDirection;
                        NextState = DisplayPreAislePrompt;
                    }
                }
            }, AisleCheck, DisplayPreAislePrompt);


            ConfigureDisplayState(DisplayPreAislePrompt, AisleCheck, encodeAction: EncodePreAislePrompt, decodeAction: DecodePreAislePrompt);

            //--------------------------------------------------------------------
            //Aisle states
            ConfigureLogicState(AisleCheck, () =>
            {
                NextState = PostAisleCheck;
                if (PickList[0].Aisle != Model.CurrentAisle)
                {
                    if (PickList[0].Aisle != "")
                    {
                        Model.CurrentAisle = PickList[0].Aisle;
                        NextState = DisplayAislePrompt;
                    }
                }
            }, PostAisleCheck, DisplayAislePrompt);

            ConfigureDisplayState(DisplayAislePrompt, PostAisleCheck, encodeAction: EncodeAislePrompt, decodeAction: DecodeAislePrompt);

            //--------------------------------------------------------------------
            //Post Aisle states
            ConfigureLogicState(PostAisleCheck, () =>
            {
                NextState = PickPrompt;
                if (PickList[0].PostAisleDirection != Model.CurrentPostAisle)
                {
                    if (PickList[0].PostAisleDirection != "")
                    {
                        Model.CurrentPostAisle = PickList[0].PostAisleDirection;
                        NextState = DisplayPostAislePrompt;
                    }
                }
            }, PickPrompt, DisplayPostAislePrompt);

            ConfigureDisplayState(DisplayPostAislePrompt, PickPrompt, encodeAction: EncodePostAislePrompt, decodeAction: DecodePostAislePrompt);

            //--------------------------------------------------------------------
            //Post Aisle states
            ConfigureReturnLogicState(PickPrompt, async () =>
            {
                if (PickingRegionsResponse.CurrentPickingRegion.MultiPickPrompt == 2)
                {
                    PickPromptMultipleSM.Reset();
                    await PickPromptMultipleSM.InitializeStateMachineAsync();
                }
                else
                {
                    PickPromptSingleSM.Reset();
                    await PickPromptSingleSM.InitializeStateMachineAsync();
                }

                NextState = CheckNextPick;
            }, CheckNextPick);

            //--------------------------------------------------------------------
            //End Picking States
            ConfigureReturnLogicState(CommEndPicking, async () =>
            {
                if (_Status != "N" && _Status != "B")
                {
                    Model.SetNextPick(new List<Pick>());
                    CurrentBackGroundActivityHeader = Translate.GetLocalizedTextForKey("VoiceLink_BackgroundActivity_Header_CompletingPicking");
                }
                else
                {
                    if (_Status == "N")
                    {
                        if (!PickingRegionsResponse.CurrentPickingRegion.PickByPick)
                        {
                            if (PicksResponse.HasPicks(null, new List<string>(new string[] { "N", "S" })))
                            {
                                foreach (var pick in PicksResponse.CurrentResponse)
                                {
                                    if (pick.Status == "S") pick.Status = "N";
                                }
                            }
                            else
                            {
                                NextTrigger = null;
                                CurrentBackGroundActivityHeader = Translate.GetLocalizedTextForKey("VoiceLink_BackgroundActivity_Header_CompletingPicking");
                                return;
                            }
                        }
                    }
                    else if (_Status == "B")
                    {
                        _Status = "N";
                    }

                    NextState = CheckNextPick;
                    CurrentBackGroundActivityHeader = Translate.GetLocalizedTextForKey("VoiceLink_Selection_Going_Back_For_Skips_Header");
                    await Model.LUTtransmit(LutType.UpdateStatus,
                        parameters: new UpdateStatusParam(null, "2", "N")
                    );
                }
            }, CheckNextPick);
        }

        #region EncodersDecoders
        private WorkflowObjectContainer EncodePreAislePrompt(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("VoiceLink_PreAisle_Header"),
                                                              "readyNone",
                                                              Translate.GetLocalizedTextForKey("VoiceLink_PreAisle_Prompt", model.CurrentPreAisle),
                                                              model.CurrentUserMessage,
                                                              initialPrompt: model.CurrentUserMessage);
            wfo.MessageType = model.MessageType;
            wfo.UIElements = new List<UIElement>
            {
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Value = Translate.GetLocalizedTextForKey("VoiceLink_PreAisle_Prompt", model.CurrentPreAisle),
                    Centered = true,
                    Bold = true
                }
            };

            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodePreAislePrompt(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            model.CurrentPreAisle = PickList[0].PreAisleDirection;
            model.CurrentAisle = "";
            model.CurrentPostAisle = "";
        }

        private WorkflowObjectContainer EncodeAislePrompt(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("VoiceLink_Aisle_Header"),
                                                              "ap",
                                                              Translate.GetLocalizedTextForKey("VoiceLink_Aisle_Prompt", model.CurrentAisle),
                                                              model.CurrentUserMessage,
                                                              initialPrompt: model.CurrentUserMessage);

            wfo.MessageType = model.MessageType;

            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(VoiceLinkModuleVocab.SkipAisle,
                confirm: true,
                processActionAsync: PerformSkipAisle,
                itemValidationDelegate: CheckLastAisle));

            wfo.UIElements = new List<UIElement>
            {
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Value = Translate.GetLocalizedTextForKey("VoiceLink_Aisle_Prompt", model.CurrentAisle),
                    Centered = true,
                    Bold = true
                }
            };

            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private string CheckLastAisle()
        {
            string result = null;

            //Do nothing, skip aisle always valid in PickByPick regions
            if (PickingRegionsResponse.CurrentPickingRegion.PickByPick)
            {
            }
            //If not pick by pick and skip aisle allowed, than check for last aisle
            else if (PickingRegionsResponse.CurrentPickingRegion.AllowSkipAisle)
            {
                //If there are not any non-picked picks (status P or X) that are not in current Aisle (and pre-aisle)
                //Then user is on last aisle and cannot be skipped
                if (PicksResponse.CurrentResponse.Where(pick => pick.Status != "P" && pick.Status != "X" 
                        && (pick.PreAisleDirection != Model.CurrentPreAisle || pick.Aisle != Model.CurrentAisle)).Count() == 0)
                {
                    result = Translate.GetLocalizedTextForKey("VoiceLink_SkipAisle_LastAisle");
                }
            }
            else
            {
                result = Translate.GetLocalizedTextForKey("VoiceLink_SkipAisle_NotAllowed");
            }

            return result;
        }

        private async Task<CoreAppSMState> PerformSkipAisle()
        {
            string skipStatus = _Status == "B" ? "N" :"S";

            foreach (var pick in PicksResponse.CurrentResponse)
            {
                if (pick.Aisle == Model.CurrentAisle && pick.PreAisleDirection == Model.CurrentPreAisle && pick.Status == _Status)
                {
                    pick.Status = skipStatus;
                }
            }

            GenericResponseLUT response;
            do
            {
                response = await Model.DataService.UpdateStatusAsync(AssignmentsResponse.CurrentResponse[0].GroupID, Model.CurrentPick.LocationId, "1", skipStatus, PickingRegionsResponse.CurrentPickingRegion.UseLut);
            } while (response.ErrorCode != 0);
            Model.CurrentAisle = "";

            return CheckNextPick;
        }


        private void DecodeAislePrompt(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            model.CurrentAisle = PickList[0].Aisle;
            model.CurrentPostAisle = "";
        }

        private WorkflowObjectContainer EncodePostAislePrompt(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("VoiceLink_PostAisle_Header"),
                                                              "readyNone",
                                                              Translate.GetLocalizedTextForKey("VoiceLink_PostAisle_Prompt", model.CurrentPostAisle),
                                                              model.CurrentUserMessage,
                                                              initialPrompt: model.CurrentUserMessage);
            wfo.MessageType = model.MessageType;
            wfo.UIElements = new List<UIElement>
            {
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Value = Translate.GetLocalizedTextForKey("VoiceLink_PostAisle_Prompt", model.CurrentPostAisle),
                    Centered = true,
                    Bold = true
                }
            };

            wfoContainer.Add(wfo);
            return wfoContainer;
        }
        private void DecodePostAislePrompt(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            model.CurrentPostAisle = PickList[0].PostAisleDirection;
        }
        #endregion

        public override void Reset()
        {
            NextTrigger = null;
            AutoShort = false;
            PickList = new List<Pick>();

            PickPromptSingleSM.Reset();
            PickPromptMultipleSM.Reset();
        }
    }
}