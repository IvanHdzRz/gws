//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace SimpleApp
{
    using System.Threading.Tasks;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;

    public class SimpleAppSecondStateMachine : SimplifiedBaseBusinessLogic<ISimpleAppModel, SimpleAppBusinessLogic, ISimpleAppConfigRepository>
    {
        public static readonly CoreAppSMState DisplayEnterPhoneNumber = new CoreAppSMState(nameof(DisplayEnterPhoneNumber));
        public static readonly CoreAppSMState ExecuteProcessPhoneNumber = new CoreAppSMState(nameof(ExecuteProcessPhoneNumber));

        public SimpleAppSecondStateMachine(SimplifiedStateMachineManager<SimpleAppBusinessLogic, ISimpleAppModel> manager, ISimpleAppModel model) : base(manager, model) {}

        public override void ConfigureStates()
        {
            ConfigureDisplayState(DisplayEnterPhoneNumber, 
                                  ExecuteProcessPhoneNumber,
                                  encodeAction: EncodeDisplayEnterPhoneNumber,
                                  availableOverflowMenuItems: new InteractiveItem[]
                                  {
                                      new InteractiveItem("OverflowMenu_SkipPhoneNumber", Translate.GetLocalizedTextForKey("OverflowMenu_SkipPhoneNumber"),  processActionAsync: ProcessSkipPhoneNumberAsync, confirm: true),
                                      new InteractiveItem("OverflowMenu_EnterDefaultNumber", Translate.GetLocalizedTextForKey("OverflowMenu_EnterDefaultNumber"),  processActionAsync: ProcessEnterDefaultNumberAsync)
                                  },
                                  decodeAction: DecodeDisplayEnterPhoneNumber,
                                  backButtonAction: () => Manager.ExecuteGoToStateAsync(SimpleAppBusinessLogic.DisplayGetName));

            ConfigureReturnLogicState(ExecuteProcessPhoneNumber,
                                      ProcessPhoneNumber);
        }

        #region PhoneNumberConfiguration
        private WorkflowObjectContainer EncodeDisplayEnterPhoneNumber(ISimpleAppModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("SimpleApp_GetPhoneNumber_Header"),
                                                                 "getName",
                                                                 Translate.GetLocalizedTextForKey("SimpleApp_GetPhoneNumber_Label"),
                                                                 Translate.GetLocalizedTextForKey("SimpleApp_GetPhoneNumber_Prompt"),
                                                                 null,
                                                                 model.CurrentUserMessage);

            wfo.ValueProperties.AllowedCharacters = CharacterSet.Numeric + "-";
            wfoContainer.Add(wfo);

            return wfoContainer;
        }

        private void DecodeDisplayEnterPhoneNumber(SlotContainer slotContainer, ISimpleAppModel model)
        {
            model.PhoneNumber = GenericBaseEncoder<ISimpleAppModel>.DecodeValueEntry(slotContainer);
        }

        private void ProcessPhoneNumber()
        {
            Model.PhoneNumber = Model.PhoneNumber + "_Processed";
        }

        private Task<CoreAppSMState> ProcessSkipPhoneNumberAsync()
        {
            Model.PhoneNumber = null;
            return Task.FromResult(SimpleAppBusinessLogic.DisplayShowNameAndNumber);
        }

        private Task<CoreAppSMState> ProcessEnterDefaultNumberAsync()
        {
            Model.PhoneNumber = "111-111-1111";
            return Task.FromResult(SimpleAppBusinessLogic.DisplayShowNameAndNumber);
        }
        #endregion
    }
}
