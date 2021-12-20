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

    public abstract class SingleRegionStateMachine : SimplifiedBaseBusinessLogic<IVoiceLinkModel, VoiceLinkStateMachine, IVoiceLinkConfigRepository>
    {
        public static readonly CoreAppSMState SingleRegionStart = new CoreAppSMState(nameof(SingleRegionStart));
        public static readonly CoreAppSMState CommGetValidRegions = new CoreAppSMState(nameof(CommGetValidRegions));
        public static readonly CoreAppSMState DisplayRegionSelection = new CoreAppSMState(nameof(DisplayRegionSelection));
        public static readonly CoreAppSMState CommGetRegionConfig = new CoreAppSMState(nameof(CommGetRegionConfig));
        public static readonly CoreAppSMState AfterGetValidRegionsTransmit = new CoreAppSMState(nameof(AfterGetValidRegionsTransmit));
        public static readonly CoreAppSMState AfterGetRegionConfigTransmit = new CoreAppSMState(nameof(AfterGetRegionConfigTransmit));

        public const int ERROR_CODE_IN_PROGRESS_SPECIFIC_REGION = 2;
        public const int ERROR_CODE_IN_PROGRESS_ANOTHER_FUNCTION = 3;

        private readonly ILog _Log = LogManager.GetLogger(nameof(SingleRegionStateMachine));

        public SingleRegionStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            ConfigureLogicState(CommGetValidRegions,
                                async () => await CommGetValidRegionsAsync(),
                                AfterGetValidRegionsTransmit);

            ConfigureReturnLogicState(AfterGetValidRegionsTransmit,
                                      () =>
                                      {
                                          NextState = VoiceLinkStateMachine.DisplayFunctionSelection;
                                          if (RegionsResponse.CurrentResponse.ErrorCode == ERROR_CODE_IN_PROGRESS_SPECIFIC_REGION)
                                          {
                                              Model.AvailableRegions.Add(RegionsResponse.CurrentResponse[0]);
                                              SelectionStateMachine.InProgressWork = true;
                                              Model.CurrentRegion = Model.AvailableRegions[0];
                                              CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_GetValidRegions_Success_InProgressWork");
                                              MessageType = UserMessageType.Warning;
                                          }
                                          else if (RegionsResponse.CurrentResponse.ErrorCode == ERROR_CODE_IN_PROGRESS_ANOTHER_FUNCTION)
                                          {
                                              CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_GetValidRegions_Success_AnotherFunction");
                                              MessageType = UserMessageType.Warning;
                                          }
                                          else if (RegionsResponse.CurrentResponse.ErrorCode != 0)
                                          {
                                              CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_GetValidRegions_Failed_RegionPermissions");
                                              return;
                                          }
                                          else if (RegionsResponse.CurrentResponse.Count == 0)
                                          {
                                              CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_GetValidRegions_Failed_NoRegions");
                                              return;
                                          }

                                          if (RegionsResponse.CurrentResponse.ErrorCode != ERROR_CODE_IN_PROGRESS_SPECIFIC_REGION)
                                          {
                                              foreach (var region in RegionsResponse.CurrentResponse)
                                              {
                                                  Model.AvailableRegions.Add(region);
                                              }
                                          }

                                          NextState = DisplayRegionSelection;
                                      },
                                      new List<CoreAppSMState> { VoiceLinkStateMachine.DisplayFunctionSelection },
                                      DisplayRegionSelection);

            ConfigureDisplayState(DisplayRegionSelection,
                                  CommGetRegionConfig, 
                                  encodeAction: EncodeSelectRegion,
                                  decodeAction: DecodeSelectRegion,
                                  followedByBackgroundActivity: true,
                                  backgroundActivityHeaderKey: "VoiceLink_BackgroundActivity_Header_RetrieveRegions",
                                  onEntryAction: () => Model.ResetSelectionRegion());

            ConfigureReturnLogicState(CommGetRegionConfig,
                                      async () => await CommGetRegionConfigAsync(),
                                      new List<CoreAppSMState> { VoiceLinkStateMachine.DisplayFunctionSelection });
        }

        protected virtual WorkflowObjectContainer EncodeSelectRegion(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            WorkflowObject wfo;

            var regions = model.AvailableRegions;
            if (regions.Count > 1)
            {
                wfo = WorkflowObjectFactory.CreateMenuItemsIntent(Translate.GetLocalizedTextForKey("VoiceLink_SelectRegion_Header"),
                                                                  "selectedRegion",
                                                                  Translate.GetLocalizedTextForKey("VoiceLink_SelectRegion_Prompt"),
                                                                  message: model.CurrentUserMessage,
                                                                  initialPrompt: model.CurrentUserMessage);

                //Add regions to menu list with region number as key value
                foreach (var r in regions)
                {
                    wfo.MenuItemsProperties.AddMenuItem(r.Name, key: r.Number.ToString());
                }
                wfo.MenuItemsProperties.DisplayKeys = true;
            }
            else if (regions.Count == 1)
            {
                wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("VoiceLink_SelectRegionSingle_Header"),
                                                              "readyOne",
                                                              Translate.GetLocalizedTextForKey("VoiceLink_SelectRegionSingle_Prompt", regions[0].Name),
                                                              model.CurrentUserMessage,
                                                              initialPrompt: model.CurrentUserMessage);

                wfo.UIElements = new List<UIElement>
                {
                    new UIElement
                    {
                        ElementType = UIElementType.Detail,
                        Value = Translate.GetLocalizedTextForKey("VoiceLink_SelectRegionSingle_Prompt", regions[0].Name),
                        Centered = true,
                        Bold = true
                    }
                };
            }
            else
            {
                wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("VoiceLink_SelectRegionNone_Header"),
                                                              "readyNone",
                                                              Translate.GetLocalizedTextForKey("VoiceLink_SelectRegionNone_Prompt"),
                                                              model.CurrentUserMessage,
                                                              initialPrompt: model.CurrentUserMessage);
            }

            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        protected virtual void DecodeSelectRegion(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            if (model.AvailableRegions.Count > 1)
            {
                (var menuItems, var cancelled) = GenericBaseEncoder<IVoiceLinkModel>.DecodeMenuItems(slotContainer);
                var selectedRegionName = menuItems.First((i) => i.Selected).DisplayName;
                model.CurrentRegion = model.AvailableRegions.FirstOrDefault(r => r.Name == selectedRegionName);
            }
        }

        public override void Reset()
        {
            Model.AvailableRegions = new List<Region>();
        }

        protected abstract Task CommGetValidRegionsAsync();

        protected abstract Task CommGetRegionConfigAsync();
    }
}
