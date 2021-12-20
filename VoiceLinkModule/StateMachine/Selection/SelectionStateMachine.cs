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

    public class SelectionStateMachine : SimplifiedBaseBusinessLogic<IVoiceLinkModel, VoiceLinkStateMachine, IVoiceLinkConfigRepository>
    {
        public static readonly CoreAppSMState StartSelection = new CoreAppSMState(nameof(StartSelection));
        public static readonly CoreAppSMState ValidateRegions = new CoreAppSMState(nameof(ValidateRegions));
        public static readonly CoreAppSMState GetAssignment = new CoreAppSMState(nameof(GetAssignment));
        public static readonly CoreAppSMState CheckAssignment = new CoreAppSMState(nameof(CheckAssignment));
        public static readonly CoreAppSMState BeginAssignment = new CoreAppSMState(nameof(BeginAssignment));
        public static readonly CoreAppSMState PickAssignment = new CoreAppSMState(nameof(PickAssignment));
        public static readonly CoreAppSMState PickRemaining = new CoreAppSMState(nameof(PickRemaining));
        public static readonly CoreAppSMState DisplayPickRemainingPrompt = new CoreAppSMState(nameof(DisplayPickRemainingPrompt));
        public static readonly CoreAppSMState DisplayPickingCompletePrompt = new CoreAppSMState(nameof(DisplayPickingCompletePrompt));
        public static readonly CoreAppSMState PrintLabel = new CoreAppSMState(nameof(PrintLabel));
        public static readonly CoreAppSMState DeliverAssignment = new CoreAppSMState(nameof(DeliverAssignment));
        public static readonly CoreAppSMState CommCompleteAssignment = new CoreAppSMState(nameof(CommCompleteAssignment));
        public static readonly CoreAppSMState DisplayAssignmentComplete = new CoreAppSMState(nameof(DisplayAssignmentComplete));
        public static readonly CoreAppSMState AfterAssignmentLutTransmit = new CoreAppSMState(nameof(AfterAssignmentLutTransmit));

        private SelectionRegionStateMachine _SelectionRegionSM;
        private SelectionRegionStateMachine SelectionRegionSM { get { return Manager.CreateStateMachine(ref _SelectionRegionSM); } }
        private GetAssignmentAutoStateMachine _SelectionGetAssignmentAutoSM;
        private GetAssignmentAutoStateMachine SelectionGetAssignmentAutoSM { get { return Manager.CreateStateMachine(ref _SelectionGetAssignmentAutoSM); } }
        private GetAssignmentManualStateMachine _SelectionGetAssignmentManualSM;
        private GetAssignmentManualStateMachine SelectionGetAssignmentManualSM { get { return Manager.CreateStateMachine(ref _SelectionGetAssignmentManualSM); } }
        private BeginAssignmentStateMachine _BeginAssignmentSM;
        private BeginAssignmentStateMachine BeginAssignmentSM { get { return Manager.CreateStateMachine(ref _BeginAssignmentSM); } }
        private PickAssignmentStateMachine _PickAssignmentSM;
        private PickAssignmentStateMachine PickAssignmentSM { get { return Manager.CreateStateMachine(ref _PickAssignmentSM); } }

        public static bool InProgressWork { get; set; } = false;
        public static bool PickOnly { get; set; } = false;
        private bool _Passed = false;

        public SelectionStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            ConfigureLogicState(StartSelection,
                                async () =>
                                {
                                    SelectionRegionSM.Reset();
                                    await SelectionRegionSM.InitializeStateMachineAsync();

                                    NextState = ValidateRegions;
                                },
                                ValidateRegions);

            ConfigureReturnLogicState(ValidateRegions,
                                     () =>
                                     {
                                         Model.SignOffAllowed = true;
                                         CurrentBackGroundActivityHeader = Translate.GetLocalizedTextForKey("VoiceLink_BackgroundActivity_Header_ValidateRegions");

                                         var valid = false;
                                         var errorCode = 0;
                                         foreach (var regionPermissions in RegionsResponse.CurrentResponse)
                                         {
                                             if (regionPermissions.ErrorCode == SingleRegionStateMachine.ERROR_CODE_IN_PROGRESS_ANOTHER_FUNCTION)
                                             {
                                                 errorCode = SingleRegionStateMachine.ERROR_CODE_IN_PROGRESS_ANOTHER_FUNCTION;
                                             }
                                             if (regionPermissions.Number >= 0)
                                             {
                                                 valid = true;
                                             }
                                         }

                                         if (valid && errorCode == 0)
                                         {
                                             foreach (var pickingRegionConfig in PickingRegionsResponse.CurrentResponse)
                                             {
                                                 if (pickingRegionConfig.ErrorCode == SingleRegionStateMachine.ERROR_CODE_IN_PROGRESS_ANOTHER_FUNCTION)
                                                 {
                                                     errorCode = SingleRegionStateMachine.ERROR_CODE_IN_PROGRESS_ANOTHER_FUNCTION;
                                                 }
                                             }
                                         }

                                         if (errorCode == SingleRegionStateMachine.ERROR_CODE_IN_PROGRESS_ANOTHER_FUNCTION)
                                         {
                                             NextState = VoiceLinkStateMachine.DisplayFunctionSelection;
                                             CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_ValidateRegions_Failed_WorkInAnotherFunction");
                                             return;
                                         }
                                         else if (!valid)
                                         {
                                             NextState = VoiceLinkStateMachine.DisplayFunctionSelection;
                                             CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_ValidateRegions_Failed_RegionNotAuth");
                                             return;
                                         }

                                         NextState = GetAssignment;
                                         return;
                                     },
                                     new List<CoreAppSMState> { VoiceLinkStateMachine.DisplayFunctionSelection },
                                     GetAssignment);

            ConfigureLogicState(GetAssignment,
                                async () =>
                                {
                                    Model.SignOffAllowed = true;

                                    CurrentBackGroundActivityHeader = Translate.GetLocalizedTextForKey("VoiceLink_BackgroundActivity_Header_RetrieveAssignment");

                                    // From VL VAD
                                    // check if we should reset current region config record
                                    // Do not set it if last error was 2 (no assignment for region record),
                                    // in this case we want to keep it where it is at
                                    // Do not set it if only getting pick, this means we are still getting
                                    // picks for the current assignment (pick by pick, of go back pass)
                                    if (AssignmentsResponse.CurrentResponse?.ErrorCode != 2 && !PickOnly)
                                    {
                                        PickingRegionsResponse.ResetCurrentPickingRegionIndex();
                                    }

                                    if (PickingRegionsResponse.CurrentPickingRegion.AutoAssign)
                                    {
                                        SelectionGetAssignmentAutoSM.Reset();
                                        SelectionGetAssignmentAutoSM.PickOnly = PickOnly;
                                        await SelectionGetAssignmentAutoSM.InitializeStateMachineAsync();
                                    }
                                    else
                                    {
                                        SelectionGetAssignmentManualSM.Reset();
                                        SelectionGetAssignmentManualSM.PickOnly = PickOnly;
                                        await SelectionGetAssignmentManualSM.InitializeStateMachineAsync();
                                    }

                                    NextState = CheckAssignment;
                                },
                                CheckAssignment);

            ConfigureReturnLogicState(CheckAssignment,
                                      () =>
                                      {
                                          // VAD State: check assignment
                                          if (AssignmentsResponse.CurrentResponse.ErrorCode == 2)
                                          {
                                              // No assignments available
                                              if (PickingRegionsResponse.CurrentResponse.Count - 1 > PickingRegionsResponse.CurrentPickingRegionIdx)
                                              {
                                                  // If another picking region is available go to the next region
                                                  PickingRegionsResponse.IncrementCurrentPickingRegionIndex();
                                                  NextState = GetAssignment;
                                                  return;
                                              }
                                              else
                                              {
                                                  PickingRegionsResponse.ResetCurrentPickingRegionIndex();
                                                  NextState = StartSelection;
                                                  return;
                                              }
                                          }
                                          else
                                          {
                                              // Find region that matches assignment type
                                              var validRegionFound = false;
                                              PickingRegionsResponse.ResetCurrentPickingRegionIndex();
                                              foreach (PickingRegion pickingRegion in PickingRegionsResponse.CurrentResponse)
                                              {
                                                  if ((pickingRegion.AssignmentType == 2 && AssignmentsResponse.FirstAssignment().IsChase) ||
                                                      (pickingRegion.AssignmentType == 1 && !AssignmentsResponse.FirstAssignment().IsChase))
                                                  {
                                                      validRegionFound = true;
                                                      break;
                                                  }
                                                  PickingRegionsResponse.IncrementCurrentPickingRegionIndex();
                                              }

                                              if (!validRegionFound)
                                              {
                                                  CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_CheckAssignment_Failed_NoRegionMatch");
                                                  NextState = VoiceLinkStateMachine.SignOff;
                                                  return;
                                              }
                                          }

                                          NextState = BeginAssignment;
                                      },
                                      new List<CoreAppSMState> { VoiceLinkStateMachine.SignOff },
                                      GetAssignment,
                                      StartSelection,
                                      BeginAssignment);

            ConfigureLogicState(BeginAssignment,
                                async () =>
                                {
                                    Model.SignOffAllowed = PickingRegionsResponse.CurrentPickingRegion?.AllowSignOff ?? true;

                                    var picksAvailable = false;
                                    foreach (var pick in PicksResponse.CurrentResponse)
                                    {
                                        if (pick.Status != "")
                                        {
                                            picksAvailable = true;
                                            break;
                                        }
                                    }

                                    if (!picksAvailable)
                                    {
                                        NextState = PrintLabel;
                                        return;
                                    }

                                    if (!PickOnly)
                                    {
                                        BeginAssignmentSM.Reset();
                                        await BeginAssignmentSM.InitializeStateMachineAsync();
                                    }
                                    else if (PickingRegionsResponse.CurrentPickingRegion.GoBackForShorts != 0 &&
                                             !PickingRegionsResponse.CurrentPickingRegion.PickByPick)
                                    {
                                        if (PicksResponse.CurrentResponse.Where(p => p.Status == "S").Count() > 0)
                                        {
                                            CurrentUserMessage = Translate.GetLocalizedTextForKey("Prompt_Selection_Picking_Skips_Shorts");
                                        }
                                        else
                                        {
                                            CurrentUserMessage = Translate.GetLocalizedTextForKey("Prompt_Selection_Picking_Shorts");
                                        }
                                    }

                                    NextState = PickAssignment;
                                },
                                PrintLabel,
                                PickAssignment);

            ConfigureLogicState(PickAssignment,
                                async () =>
                                {
                                    PickAssignmentSM.Reset();
                                    await PickAssignmentSM.InitializeStateMachineAsync();

                                    NextState = PickRemaining;
                                },
                                PickRemaining);

            ConfigureLogicState(PickRemaining,
                                () =>
                                {
                                    NextState = PrintLabel;

                                    if (PickingRegionsResponse.CurrentPickingRegion.PickByPick)
                                    {
                                        PickOnly = true;
                                        NextState = GetAssignment;
                                        return;
                                    }
                                    else if (PickingRegionsResponse.CurrentPickingRegion.GoBackForShorts != 0 && PicksResponse.HasAnyShorts() && !PickOnly)
                                    {
                                        PickOnly = true;
                                        NextState = DisplayPickRemainingPrompt;
                                        return;
                                    }

                                    if (PickingRegionsResponse.CurrentPickingRegion.PickByPick)
                                    {
                                        NextState = DisplayPickingCompletePrompt;
                                    }
                                },
                                GetAssignment,
                                DisplayPickRemainingPrompt,
                                DisplayPickingCompletePrompt,
                                PrintLabel);

            ConfigureDisplayState(DisplayPickRemainingPrompt,
                                  GetAssignment,
                                  encodeAction: EncodePickRemainingPrompt);

            ConfigureDisplayState(DisplayPickingCompletePrompt, 
                                  PrintLabel,
                                  encodeAction: EncodePickingCompletePrompt);

            ConfigureLogicState(PrintLabel,
                                async () =>
                                {
                                    if (PickingRegionsResponse.CurrentPickingRegion.PrintContainerLabels == 2)
                                    {
                                        // TODO: Implement when printing is supported
                                        await Task.CompletedTask;
                                    }

                                    NextState = DeliverAssignment;
                                },
                                DeliverAssignment);

            ConfigureLogicState(DeliverAssignment,
                                async () =>
                                {
                                    NextState = CoreAppStates.BackgroundActvity;
                                    backgroundActivityNextState = CommCompleteAssignment;
                                    CurrentBackGroundActivityHeader = Translate.GetLocalizedTextForKey("VoiceLink_BackgroundActivity_Header_CompletingAssignment");

                                    PickOnly = false;

                                    // Check target containers, if deliver at close and target container then do not deliver
                                    if (PickingRegionsResponse.CurrentPickingRegion.DeliverContainerClosed && PicksResponse.CurrentResponse[0].TargetContainer > 0)
                                    {
                                        return;
                                    }

                                    foreach (var assignment in AssignmentsResponse.CurrentResponse)
                                    {
                                        if (Model.PassInprogress == true && assignment.PassAssignment == 1)
                                        {
                                            continue;
                                        }

                                        if (PickingRegionsResponse.CurrentPickingRegion.DeliveryType == 1 || PickingRegionsResponse.CurrentPickingRegion.DeliveryType == 0)
                                        {
                                            // TODO: Implement deliver assignment state machine
                                            await Task.CompletedTask;
                                        }
                                    }
                                },
                                CoreAppStates.BackgroundActvity);

            ConfigureLogicState(CommCompleteAssignment,
                                async () =>
                                {
                                    Model.SignOffAllowed = true;

                                    // Check if any passed assignments
                                    _Passed = Model.PassInprogress == true && AssignmentsResponse.CurrentResponse != null && AssignmentsResponse.CurrentResponse.Where(a => a.PassAssignment == 1).Count() > 0;

                                    // Send LUT telling server the assignment is complete.
                                    NextState = AfterAssignmentLutTransmit;
                                    if (_Passed)
                                    {
                                        await Model.LUTtransmit(LutType.PassAssignment, "VoiceLink_BackgroundActivity_Header_CompletingAssignment",
                                            new List<int>() { 2 }
                                        );
                                    }
                                    else
                                    {
                                        await Model.LUTtransmit(LutType.CompleteAssignment, "VoiceLink_BackgroundActivity_Header_CompletingAssignment",
                                            new List<int>() { 2 }
                                        );
                                    }
                                },
                                AfterAssignmentLutTransmit);

            ConfigureLogicState(AfterAssignmentLutTransmit,
                                () =>
                                {
                                    var error = GenericLUTResponse.ErrorCode;
                                    if (error == 2)
                                    {
                                        // Special case return code tells the operator to switch regions
                                        NextState = StartSelection;
                                    }
                                    else
                                    {
                                        NextState = DisplayAssignmentComplete;
                                    }
                                },
                                StartSelection,
                                DisplayAssignmentComplete);

            ConfigureDisplayState(DisplayAssignmentComplete, 
                                  GetAssignment,
                                  encodeAction: EncodeAssignmentCompletePrompt);
        }

        protected virtual WorkflowObjectContainer EncodePickRemainingPrompt(IVoiceLinkModel model)
        {
            var prompt = Translate.GetLocalizedTextForKey("VoiceLink_Selection_PickRemaining_Prompt");
            if (PicksResponse.HasAnyShorts() && PicksResponse.HasPicksWithStatus("S"))
            {
                prompt = Translate.GetLocalizedTextForKey("VoiceLink_Selection_PickRemaining_Prompt_Shorts_Skips");
            }
            else if (PicksResponse.HasAnyShorts())
            {
                prompt = Translate.GetLocalizedTextForKey("VoiceLink_Selection_PickRemaining_Prompt_Shorts");
            }
            else if (PicksResponse.HasPicksWithStatus("S"))
            {
                prompt = Translate.GetLocalizedTextForKey("VoiceLink_Selection_PickRemaining_Prompt_Skips");
            }


            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("VoiceLink_Selection_PickRemaining_Header"),
                                                              "pickRemainingReady",
                                                              prompt,
                                                              model.CurrentUserMessage,
                                                              navigateBackEnabled: false,
                                                              initialPrompt: model.CurrentUserMessage);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        protected virtual WorkflowObjectContainer EncodePickingCompletePrompt(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("VoiceLink_Selection_Picking_Complete_Header"),
                                                              "pickingCompleteReady",
                                                              Translate.GetLocalizedTextForKey("VoiceLink_Selection_Picking_Complete_Prompt"),
                                                              model.CurrentUserMessage,
                                                              navigateBackEnabled: false,
                                                              initialPrompt: model.CurrentUserMessage);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        protected virtual WorkflowObjectContainer EncodeAssignmentCompletePrompt(IVoiceLinkModel model)
        {
            var prompt = Translate.GetLocalizedTextForKey("VoiceLink_Selection_Assignment_Complete_Prompt");
            if (_Passed)
            {
                prompt = Translate.GetLocalizedTextForKey("VoiceLink_Selection_Assignment_Pass_Prompt");
            }

            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("VoiceLink_Selection_Assignment_Complete_Header"),
                                                              "assignmentCompleteReady",
                                                              prompt,
                                                              model.CurrentUserMessage,
                                                              navigateBackEnabled: false,
                                                              initialPrompt: model.CurrentUserMessage);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        public override void Reset()
        {
            NextTrigger = null;

            InProgressWork = false;
            PickOnly = false;
            _Passed = false;

            SelectionRegionSM.Reset();
            SelectionGetAssignmentAutoSM.Reset();
            SelectionGetAssignmentManualSM.Reset();
            BeginAssignmentSM.Reset();
            PickAssignmentSM.Reset();
        }
    }
}
