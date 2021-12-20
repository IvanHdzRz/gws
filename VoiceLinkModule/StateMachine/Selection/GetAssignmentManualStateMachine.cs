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

    public class GetAssignmentManualStateMachine : GetAssignmentStateMachine
    {
        public static readonly CoreAppSMState DisplayWorkId = new CoreAppSMState(nameof(DisplayWorkId));
        public static readonly CoreAppSMState CommGetWorkId = new CoreAppSMState(nameof(CommGetWorkId));
        public static readonly CoreAppSMState DisplayReviewWorkIds = new CoreAppSMState(nameof(DisplayReviewWorkIds));
        public static readonly CoreAppSMState HandleReviewWorkIds = new CoreAppSMState(nameof(HandleReviewWorkIds));
        public static readonly CoreAppSMState AfterGetWorkId = new CoreAppSMState(nameof(AfterGetWorkId));

        public const int ERROR_CODE_NO_MORE_ALLOWED = 3;
        public const int ERROR_CODE_DUPLICATE_MATCHES_FOUND = 4;

        protected int NumberOfAssignmentsRequested { get; set; }

        public GetAssignmentManualStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
        }

        public override void Reset()
        {
            base.Reset();
            NumberOfAssignmentsRequested = 0;
        }

        protected override void DefineStates()
        {
            base.DefineStates();
            DefineDisplayWorkId();
            DefineGetWorkIdState();
            DefineDisplayDuplicateWorkId();
            DefineHandleDuplicateWorkId();
        }

        protected override void DefineStartState()
        {
            base.DefineStartState();
            AddDestinationState(StartGetAssignment, DisplayWorkId);
        }

        protected override void PerformStartGetAssignment()
        {
            if (SelectionStateMachine.InProgressWork)
            {
                NextState = CommGetAssignments;
            }
            else
            {
                NextState = DisplayWorkId;
            }
        }

        protected virtual void DefineDisplayWorkId()
        {
            ConfigureDisplayState(DisplayWorkId,
                                  CommGetWorkId,
                                  encodeAction: EncodeDisplayWorkIdPrompt,
                                  decodeAction: DecodeDisplayWorkIdPrompt,
                                  followedByBackgroundActivity: true,
                                  backgroundActivityHeaderKey: "VoiceLink_BackgroundActivity_Header_RetrieveWorkId");
        }

        protected virtual WorkflowObjectContainer EncodeDisplayWorkIdPrompt(IVoiceLinkModel voiceLinkModel)
        {
            var wfoContainer = new WorkflowObjectContainer();
            voiceLinkModel.WorkId = null;
            voiceLinkModel.WorkIdScanned = false;
            int workIdLen = PickingRegionsResponse.CurrentPickingRegion.WorkIdLength;

            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("VoiceLink_GetAssignment_Manaual_EnterWorkId_Header"),
                                                                 "workid",
                                                                 Translate.GetLocalizedTextForKey("VoiceLink_GetAssignment_Manaual_EnterWorkId_Label"),
                                                                 Translate.GetLocalizedTextForKey("VoiceLink_GetAssignment_Manaual_EnterWorkId_Label"),
                                                                 "",
                                                                 new List<UIElement> {
                                                                         new UIElement
                                                                         {
                                                                             ElementType = UIElementType.Detail,
                                                                             Label = Translate.GetLocalizedTextForKey("VoiceLink_GetAssignment_Manaual_EnterWorkIdLen_Label"),
                                                                             Value = workIdLen <= 0 ? Translate.GetLocalizedTextForKey("VoiceLink_GetAssignment_Manaual_EnterWorkIdLenAll_Label") : workIdLen.ToString(),
                                                                             Bold = true,
                                                                             ValueInlineWithLabel = true
                                                                         }
                                                                 },
                                                                 voiceLinkModel.CurrentUserMessage,
                                                                 initialPrompt: voiceLinkModel.CurrentUserMessage,
                                                                 isPriorityPrompt: false);

            wfo.ValueProperties.AllowedCharacters = CharacterSet.AlphaNumeric;
            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(DefaultModuleVocab.VocabCancel));

            if (workIdLen > 0)
            {
                wfo.ValueProperties.MaxAllowedLength = workIdLen;
                wfo.ValueProperties.MinRequiredLength = workIdLen;
            }
            wfo.MessageType = voiceLinkModel.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeDisplayWorkIdPrompt(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            var response = GenericBaseEncoder<IVoiceLinkModel>.DecodeValueEntry(slotContainer);
            model.WorkId = response == DefaultModuleVocab.VocabCancel.IdentificationKey ? null : response;
            model.WorkIdScanned = false;

            //If work ID scanned, or work ID entered by screen, 
            //and not the expected partial length, then treat as scanned (full work ID)
            if (slotContainer.Slots[0].InputMethod == InputMethodTypes.Scanned)
            {
                model.WorkIdScanned = true;
            }
            else if (slotContainer.Slots[0].InputMethod == InputMethodTypes.Screen
                && model.WorkId != null
                && model.WorkId.Length != PickingRegionsResponse.CurrentPickingRegion.WorkIdLength)
            {
                model.WorkIdScanned = true;
            }
        }

        protected virtual void DefineGetWorkIdState()
        {
            ConfigureReturnLogicState(CommGetWorkId,
                                      async () =>
                                      {
                                          if (Model.WorkId == null)
                                          {
                                              //work ID input canceled, if some assignments already requested then proceed 
                                              //with requested assignments, otherwise return to region selection
                                              NextState = NumberOfAssignmentsRequested > 0
                                                  ? CommGetAssignments
                                                  : SelectionStateMachine.StartSelection;
                                              return;
                                          }
                                          NextState = AfterGetWorkId;
                                          await Model.LUTtransmit(LutType.RequestWork, "VoiceLink_BackgroundActivity_Header_RetrieveWorkId",
                                              new List<int>() { ERROR_CODE_NO_MORE_ALLOWED, ERROR_CODE_DUPLICATE_MATCHES_FOUND },
                                              goToStateIfFail: DisplayWorkId
                                          );
                                      },
                                      new List<CoreAppSMState> { SelectionStateMachine.StartSelection },
                                      CommGetAssignments,
                                      AfterGetWorkId);

            ConfigureLogicState(AfterGetWorkId,
                                () =>
                                {
                                    if (RequestWorkResponse.ErrorCode == ERROR_CODE_NO_MORE_ALLOWED) //Host says we are done
                                    {
                                        NumberOfAssignmentsRequested++;
                                        NextState = CommGetAssignments;
                                    }
                                    else if (RequestWorkResponse.ErrorCode == ERROR_CODE_DUPLICATE_MATCHES_FOUND) //Duplicate matches found
                                    {
                                        NextState = DisplayReviewWorkIds;
                                        Model.DuplicateWorkIds = RequestWorkResponse.CurrentResponse.GetDuplicates();
                                    }
                                    else if (RequestWorkResponse.ErrorCode == 0)
                                    {
                                        NumberOfAssignmentsRequested++;
                                        if (NumberOfAssignmentsRequested < 1) //TODO: Replace 1 with following when multiples supported "PickingRegionsResponse.CurrentPickingRegion.NumAssignsAllowed"
                                        {
                                            NextState = DisplayWorkId;
                                        }
                                        else
                                        {
                                            NextState = CommGetAssignments;
                                        }
                                    }
                                },
                                DisplayWorkId,
                                DisplayReviewWorkIds,
                                CommGetAssignments);
        }

        protected virtual void DefineDisplayDuplicateWorkId()
        {
            ConfigureDisplayState(DisplayReviewWorkIds,
                                  HandleReviewWorkIds,
                                  encodeAction: EncodeDisplayReviewWorkIdsPrompt,
                                  decodeAction: DecodeDisplayReviewWorkIdsPrompt,
                                  followedByBackgroundActivity: true,
                                  backgroundActivityHeaderKey: "VoiceLink_BackgroundActivity_Header_RetrieveWorkId");
        }

        protected virtual WorkflowObjectContainer EncodeDisplayReviewWorkIdsPrompt(IVoiceLinkModel voiceLinkModel)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateMenuItemsIntent(Translate.GetLocalizedTextForKey("VoiceLink_GetAssignment_Manaual_MultiMatches_Header"),
                                                                  "reviewWorkId",
                                                                  Translate.GetLocalizedTextForKey("VoiceLink_GetAssignment_Manaual_MultiMatches_Prompt"),
                                                                  voiceLinkModel.DuplicateWorkIds,
                                                                  "",
                                                                  initialPrompt: string.Format(
                                                                      Translate.GetLocalizedTextForKey("VoiceLink_GetAssignment_Manaual_MultiMatches_InitialPrompt"),
                                                                      voiceLinkModel.WorkId));
            wfo.MessageType = voiceLinkModel.MessageType;
            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(DefaultModuleVocab.VocabCancel));
            wfo.MenuItemsProperties.ReturnValueIfNoOptionsSelected = DefaultModuleVocab.VocabCancel.IdentificationKey;

            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        protected virtual void DecodeDisplayReviewWorkIdsPrompt(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            (var menuItems, var cancelled) = GenericBaseEncoder<IVoiceLinkModel>.DecodeMenuItems(slotContainer, DefaultModuleVocab.VocabCancel.IdentificationKey);
            model.WorkId = cancelled ? null : menuItems.First((i) => i.Selected).DisplayName; ;
            model.WorkIdScanned = true;
        }

        protected virtual void DefineHandleDuplicateWorkId()
        {
            ConfigureLogicState(HandleReviewWorkIds,
                                () =>
                                {
                                    //User selected on of duplicate matches, so request it, if user canceled, then 
                                    //go prompt for another.
                                    NextState = CommGetWorkId;
                                    if (Model.WorkId == null) //user canceled
                                    {
                                        NextState = DisplayWorkId;
                                    }
                                },
                                CommGetWorkId,
                                DisplayWorkId); //TODO: Change to review later
        }
    }
}
