//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using Common.Logging;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class BeginAssignmentStateMachine : SimplifiedBaseBusinessLogic<IVoiceLinkModel, VoiceLinkStateMachine, IVoiceLinkConfigRepository>
    {
        public static readonly CoreAppSMState PrepareNextAssignmentSummary = new CoreAppSMState(nameof(PrepareNextAssignmentSummary));
        public static readonly CoreAppSMState DisplayAssignmentSummary = new CoreAppSMState(nameof(DisplayAssignmentSummary));
        public static readonly CoreAppSMState Print = new CoreAppSMState(nameof(Print));
        public static readonly CoreAppSMState BaseItemSummaryInitialize = new CoreAppSMState(nameof(BaseItemSummaryInitialize));
        public static readonly CoreAppSMState DisplayAskToDisplayBaseItemSummary = new CoreAppSMState(nameof(DisplayAskToDisplayBaseItemSummary));
        public static readonly CoreAppSMState HandleAskToDisplaySummaryResponse = new CoreAppSMState(nameof(HandleAskToDisplaySummaryResponse));
        public static readonly CoreAppSMState PrepepareNextBaseItemSummary = new CoreAppSMState(nameof(PrepepareNextBaseItemSummary));
        public static readonly CoreAppSMState DisplayBaseItemSummary = new CoreAppSMState(nameof(DisplayBaseItemSummary));
        public static readonly CoreAppSMState HandleBaseItemSummaryResponse = new CoreAppSMState(nameof(HandleBaseItemSummaryResponse));
        public static readonly CoreAppSMState DisplayPickBaseItemsQuery = new CoreAppSMState(nameof(DisplayPickBaseItemsQuery));
        public static readonly CoreAppSMState CommHandlePickBaseItemsQueryResponse = new CoreAppSMState(nameof(CommHandlePickBaseItemsQueryResponse));
        public static readonly CoreAppSMState AfterHandlePickBaseItems = new CoreAppSMState(nameof(AfterHandlePickBaseItems));

        private readonly ILog _Log = LogManager.GetLogger(nameof(BeginAssignmentStateMachine));

        private int _AssignmentSummaryIndex = 0;
        private List<Pick> _BaseItems = new List<Pick>();
        private int _BaseItemSummaryIndex = 0;
        private bool? _PickBaseItems = false;
        private bool? _DisplayBaseItemSummary = false;

        public BeginAssignmentStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            ConfigureLogicState(PrepareNextAssignmentSummary,
                                () => PerformPrepareNextAssigmmentSummary(),
                                PrepareNextAssignmentSummary,
                                DisplayAssignmentSummary,
                                Print);

            ConfigureDisplayState(DisplayAssignmentSummary, 
                                  PrepareNextAssignmentSummary,
                                  encodeAction: EncodeAssignmentSummary);

            ConfigureLogicState(Print,
                                async () =>
                                {
                                    // TODO: Implement when printing is supported
                                    await Task.CompletedTask;

                                    NextState = BaseItemSummaryInitialize;
                                },
                                BaseItemSummaryInitialize);

            ConfigureReturnLogicState(BaseItemSummaryInitialize,
                                      () =>
                                      {
                                          _BaseItems.Clear();
                                          var pickIds = new List<long>();
                                          foreach (var pick in PicksResponse.CurrentResponse)
                                          {
                                              if (pick.Status == "B" && !pickIds.Contains(pick.PickId))
                                              {
                                                  pickIds.Add(pick.PickId);
                                                  var baseItemPick = pick;
                                                  foreach (var matchPick in PicksResponse.CurrentResponse)
                                                  {
                                                      if (!pickIds.Contains(matchPick.PickId) && baseItemPick.Matches(matchPick))
                                                      {
                                                          pickIds.Add(matchPick.PickId);
                                                          baseItemPick.QuantityToPick += matchPick.QuantityToPick;
                                                      }
                                                  }

                                                  _BaseItems.Add(pick);
                                              }
                                          }

                                          if (_BaseItems.Count > 0)
                                          {
                                              NextState = DisplayAskToDisplayBaseItemSummary;
                                          }
                                      },
                                      DisplayAskToDisplayBaseItemSummary);

            ConfigureDisplayState(DisplayAskToDisplayBaseItemSummary, 
                                  HandleAskToDisplaySummaryResponse, 
                                  DisplayAssignmentSummary, 
                                  () => 
                                  { 
                                      CurrentUserMessage = null;  
                                      _AssignmentSummaryIndex = 0; 
                                      PerformPrepareNextAssigmmentSummary(); 
                                  },
                                  encodeAction: EncodeAskToDisplayBaseItemSummary,
                                  decodeAction: DecodeBaseItemSummary);

            ConfigureLogicState(HandleAskToDisplaySummaryResponse,
                                () => PerformHandleAskToDisplaySummaryResponse(),
                                DisplayAskToDisplayBaseItemSummary,
                                PrepepareNextBaseItemSummary,
                                DisplayPickBaseItemsQuery);

            ConfigureLogicState(PrepepareNextBaseItemSummary,
                                () =>
                                {
                                    if (_BaseItemSummaryIndex >= _BaseItems.Count)
                                    {
                                        NextState = DisplayPickBaseItemsQuery;
                                        return;
                                    }

                                    var currentBaseItem = _BaseItems[_BaseItemSummaryIndex++];

                                    // Determine id ID description is needed.
                                    long? idDescription = null;
                                    if (PickingRegionsResponse.CurrentPickingRegion.ContainerType == 0 &&
                                        AssignmentsResponse.CurrentResponse.Where(a => a.GroupID != null).Count() > 0)
                                    {
                                        idDescription = currentBaseItem.IDDescription;
                                    }

                                    var promptKey = "BeginAssignment_Prompt_BaseSummary";
                                    if (string.IsNullOrEmpty(currentBaseItem.PreAisleDirection))
                                    {
                                        promptKey = "BeginAssignment_Prompt_BaseSummary_NoPreaisle";
                                    }

                                    if (string.IsNullOrEmpty(currentBaseItem.Aisle))
                                    {
                                        promptKey = "BeginAssignment_Prompt_BaseSummary_NoAisle";
                                    }

                                    if (string.IsNullOrEmpty(currentBaseItem.PostAisleDirection))
                                    {
                                        promptKey = "BeginAssignment_Prompt_BaseSummary_NoPostaisle";
                                    }

                                    if (string.IsNullOrEmpty(currentBaseItem.PreAisleDirection) && string.IsNullOrEmpty(currentBaseItem.Aisle))
                                    {
                                        promptKey = "BeginAssignment_Prompt_BaseSummary_NoPreaisle_NoAisle";
                                    }

                                    if (string.IsNullOrEmpty(currentBaseItem.PreAisleDirection) && string.IsNullOrEmpty(currentBaseItem.Aisle) && string.IsNullOrEmpty(currentBaseItem.PostAisleDirection))
                                    {
                                        promptKey = "BeginAssignment_Prompt_BaseSummary_NoPreaisle_NoAisle_NoPoseaisle";
                                    }

                                    if (idDescription == null)
                                    {
                                        promptKey = "BeginAssignment_Prompt_BaseSummary_ID";
                                        if (string.IsNullOrEmpty(currentBaseItem.PreAisleDirection))
                                        {
                                            promptKey = "BeginAssignment_Prompt_BaseSummary_ID_NoPreaisle";
                                        }

                                        if (string.IsNullOrEmpty(currentBaseItem.Aisle))
                                        {
                                            promptKey = "BeginAssignment_Prompt_BaseSummary_ID_NoAisle";
                                        }

                                        if (string.IsNullOrEmpty(currentBaseItem.PostAisleDirection))
                                        {
                                            promptKey = "BeginAssignment_Prompt_BaseSummary_ID_NoPostaisle";
                                        }

                                        if (string.IsNullOrEmpty(currentBaseItem.PreAisleDirection) && string.IsNullOrEmpty(currentBaseItem.Aisle))
                                        {
                                            promptKey = "BeginAssignment_Prompt_BaseSummary_ID_NoPreaisle_NoAisle";
                                        }

                                        if (string.IsNullOrEmpty(currentBaseItem.PreAisleDirection) && string.IsNullOrEmpty(currentBaseItem.Aisle) && string.IsNullOrEmpty(currentBaseItem.PostAisleDirection))
                                        {
                                            promptKey = "BeginAssignment_Prompt_BaseSummary_ID_NoPreaisle_NoAisle_NoPoseaisle";
                                        }
                                    }

                                    CurrentUserMessage = Translate.GetLocalizedTextForKey(promptKey, new string[] { currentBaseItem.PreAisleDirection, currentBaseItem.Aisle, currentBaseItem.PostAisleDirection, currentBaseItem.Slot, currentBaseItem.ItemDescription.ToLower(), idDescription?.ToString(), currentBaseItem.QuantityToPick.ToString() });
                                    NextState = DisplayBaseItemSummary;
                                },
                                DisplayBaseItemSummary,
                                DisplayPickBaseItemsQuery);

            ConfigureDisplayState(DisplayBaseItemSummary, 
                                  HandleAskToDisplaySummaryResponse, 
                                  DisplayAskToDisplayBaseItemSummary,
                                  encodeAction: EncodeBaseItemSummary,
                                  decodeAction: DecodeBaseItemSummary);

            ConfigureLogicState(HandleBaseItemSummaryResponse,
                                () => PerformHandleAskToDisplaySummaryResponse(),
                                DisplayBaseItemSummary,
                                PrepepareNextBaseItemSummary,
                                DisplayPickBaseItemsQuery);

            ConfigureDisplayState(DisplayPickBaseItemsQuery,
                                  CommHandlePickBaseItemsQueryResponse,
                                  DisplayAskToDisplayBaseItemSummary,
                                  encodeAction: EncodePickBaseItemsQuery,
                                  decodeAction: DecodePickBaseItemsQuery);

            ConfigureLogicState(CommHandlePickBaseItemsQueryResponse,
                                async () =>
                                {
                                    if (_PickBaseItems == null)
                                    {
                                        NextState = DisplayPickBaseItemsQuery;
                                        CurrentUserMessage = "Display base item summary selection not set";
                                    }
                                    else if (_PickBaseItems == true)
                                    {
                                        NextState = AfterHandlePickBaseItems;
                                        await Model.LUTtransmit(LutType.UpdateStatus,
                                            parameters: new UpdateStatusParam(null, "2", "N")
                                        );
                                    }
                                },
                                DisplayPickBaseItemsQuery,
                                AfterHandlePickBaseItems);

            ConfigureReturnLogicState(AfterHandlePickBaseItems,
                                      () =>
                                      {
                                          PicksResponse.CurrentResponse.Where(p => p.Status == "B").ToList().ForEach(p => p.Status = "N");
                                      });
        }

        public override void Reset()
        {
            _AssignmentSummaryIndex = 0;
            _BaseItems = new List<Pick>();
            _DisplayBaseItemSummary = false;
            _BaseItemSummaryIndex = 0;
            _PickBaseItems = false;
            Model.PassInprogress = false;
            if (AssignmentsResponse.CurrentResponse == null || AssignmentsResponse.CurrentResponse.Where(a => a.PassAssignment == 1).Count() > 0)
            {
                Model.PassInprogress = true;
            }
        }

        private void PerformPrepareNextAssigmmentSummary()
        {
            if (_AssignmentSummaryIndex >= AssignmentsResponse.CurrentResponse.Count)
            {
                NextState = Print;
                return;
            }

            var currentAssignmentForSummary = AssignmentsResponse.CurrentResponse[_AssignmentSummaryIndex++];

            // Check if override prompt was set, that one was given
            if (currentAssignmentForSummary.SummaryPromptType == 2 && currentAssignmentForSummary.OverridePrompt == "")
            {
                currentAssignmentForSummary.SummaryPromptType = 0;
            }

            NextState = DisplayAssignmentSummary;
            if (currentAssignmentForSummary.SummaryPromptType != 0 && currentAssignmentForSummary.SummaryPromptType != 2)
            {
                NextState = PrepareNextAssignmentSummary;
                return;
            }
        }

        #region EncodersDecoders
        protected virtual WorkflowObjectContainer EncodeAssignmentSummary(IVoiceLinkModel model)
        {
            var prompt = BuildAssignmentSummaryPrompt();
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("VoiceLink_AssignmentSummary_Header"),
                                                              "readyNone",
                                                              prompt,
                                                              model.CurrentUserMessage,
                                                              initialPrompt: model.CurrentUserMessage,
                                                              isPriorityPrompt: false);
            wfo.MessageType = model.MessageType;
            wfo.UIElements = new List<UIElement>
            {
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Value = Translate.GetLocalizedTextForKey("VoiceLink_PostAisle_Prompt", prompt),
                    Centered = true,
                    Bold = true
                }
            };

            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        protected string BuildAssignmentSummaryPrompt()
        {
            // Build prompt
            var currentAssignmentForSummary = AssignmentsResponse.CurrentResponse[_AssignmentSummaryIndex - 1];
            var promptValues = new List<string>();
            if (currentAssignmentForSummary.SummaryPromptType == 0)
            {
                var promptKey = "BeginAssignment_Prompt";
                promptValues.Add(currentAssignmentForSummary.IDDescription.ToString());

                // Check if chase
                if (currentAssignmentForSummary.IsChase)
                {
                    promptKey += "_Chase";
                }

                // Check if multiple assignments
                if (AssignmentsResponse.CurrentResponse.Count > 1)
                {
                    promptKey += "_Position";
                    promptValues.Insert(0, currentAssignmentForSummary.Position.ToString());
                }

                // Check if goal time
                if (!FloatEqualityUtil.NearlyEqual(currentAssignmentForSummary.GoalTime, 0))
                {
                    promptValues.Add(currentAssignmentForSummary.GoalTime.ToString());
                    if (FloatEqualityUtil.NearlyEqual(currentAssignmentForSummary.GoalTime, 1))
                    {
                        promptKey += "_Goaltime_Single";
                    }
                    else
                    {
                        promptKey += "_Goaltime_Multi";
                    }
                }

                return Translate.GetLocalizedTextForKey(promptKey, promptValues.ToArray());
            }
            else if (currentAssignmentForSummary.SummaryPromptType == 2)
            {
                // Override prompt
                return currentAssignmentForSummary.OverridePrompt;
            }

            return "";
        }

        protected virtual WorkflowObjectContainer EncodeAskToDisplayBaseItemSummary(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateBooleanIntent(Translate.GetLocalizedTextForKey("VoiceLink_AskToDisplayBaseItemSummary_Header"),
                                                                "atdbis",
                                                                model.CurrentUserMessage,
                                                                Translate.GetLocalizedTextForKey("VoiceLink_AskToDisplayBaseItemSummary_Prompt"),
                                                                initialPrompt: model.CurrentUserMessage,
                                                                affirmativeVocab: VoiceLinkModuleVocab.Yes,
                                                                negativeVocab: VoiceLinkModuleVocab.No);

            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        protected virtual void DecodeBaseItemSummary(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            _DisplayBaseItemSummary = GenericBaseEncoder<IVoiceLinkModel>.DecodeBooleanPrompt(slotContainer);
        }

        private void PerformHandleAskToDisplaySummaryResponse()
        {
            NextState = DisplayAskToDisplayBaseItemSummary;

            if (_DisplayBaseItemSummary == null)
            {
                CurrentUserMessage = "Display base item summary selection not set";
            }
            else if (_DisplayBaseItemSummary == true)
            {
                NextState = PrepepareNextBaseItemSummary;
            }
            else
            {
                NextState = DisplayPickBaseItemsQuery;
            }
            _DisplayBaseItemSummary = null;
        }

        protected virtual WorkflowObjectContainer EncodeBaseItemSummary(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateBooleanIntent(Translate.GetLocalizedTextForKey("VoiceLink_DisplayBaseItemSummary_Header"),
                                                                "atdbis",
                                                                model.CurrentUserMessage,
                                                                null,
                                                                initialPrompt: model.CurrentUserMessage,
                                                                affirmativeVocab: VoiceLinkModuleVocab.VocabReady,
                                                                negativeVocab: VoiceLinkModuleVocab.VocabCancel);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        protected virtual WorkflowObjectContainer EncodePickBaseItemsQuery(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateBooleanIntent(Translate.GetLocalizedTextForKey("VoiceLink_PickBaseItemsQuery_Header"),
                                                                "atdbis",
                                                                Translate.GetLocalizedTextForKey("VoiceLink_PickBaseItemsQuery_Prompt"),
                                                                model.CurrentUserMessage,
                                                                initialPrompt: model.CurrentUserMessage,
                                                                affirmativeVocab: VoiceLinkModuleVocab.Yes,
                                                                negativeVocab: VoiceLinkModuleVocab.No);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        protected virtual void DecodePickBaseItemsQuery(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            _PickBaseItems = GenericBaseEncoder<IVoiceLinkModel>.DecodeBooleanPrompt(slotContainer);
        }
        #endregion
    }
}
