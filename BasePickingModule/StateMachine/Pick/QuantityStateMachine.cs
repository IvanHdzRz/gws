//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System.Threading.Tasks;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;

    public class QuantityStateMachine : SimplifiedBaseBusinessLogic<IBasePickingModel, BasePickingStateMachine, IBasePickingConfigRepository>
    {
        public static readonly CoreAppSMState DisplayEnterPickQuantity = new CoreAppSMState(nameof(DisplayEnterPickQuantity));
        public static readonly CoreAppSMState VerifyPickQuantity = new CoreAppSMState(nameof(VerifyPickQuantity));

        public static readonly CoreAppSMState DisplayConfirmShortProduct = new CoreAppSMState(nameof(DisplayConfirmShortProduct));
        public static readonly CoreAppSMState VerifyConfirmShortProduct = new CoreAppSMState(nameof(VerifyConfirmShortProduct));

        public QuantityStateMachine(SimplifiedStateMachineManager<BasePickingStateMachine, IBasePickingModel> manager, IBasePickingModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            ConfigureDisplayState(DisplayEnterPickQuantity,
                                  VerifyPickQuantity,
                                  encodeAction: EncodeEnterPickQuantity,
                                  decodeAction: DecodeEnterPickQuantity,
                                  availableOverflowMenuItems: new InteractiveItem[]
                                  {
                                      new InteractiveItem(BasePickingModuleVocab.ShortProduct, processActionAsync: () => { return Task.FromResult(DisplayConfirmShortProduct); })
                                  });

            ConfigureReturnLogicState(VerifyPickQuantity,
                                      PerformVerifyPickQuantity,
                                      DisplayEnterPickQuantity,
                                      DisplayConfirmShortProduct);

            ConfigureDisplayState(DisplayConfirmShortProduct,
                                  VerifyConfirmShortProduct,
                                  encodeAction: EncodeConfirmShortProduct,
                                  decodeAction: DecodePutInContainer,
                                  backButtonDestinationState: DisplayEnterPickQuantity);

            ConfigureReturnLogicState(VerifyConfirmShortProduct,
                                      () =>
                                      {
                                          if (Model.EnteredConfirmShortProductResponse)
                                          {
                                              Model.CurrentPick.QuantityPicked += Model.EnteredPickQuantity;
                                              // Leave NextState null to return to the previous state machine
                                          }
                                          else
                                          {
                                              NextState = DisplayEnterPickQuantity;
                                          }
                                      },
                                      DisplayEnterPickQuantity);
        }

        private WorkflowObjectContainer EncodeEnterPickQuantity(IBasePickingModel model)
        {
            model.EnteredPickQuantityString = null;
            model.EnteredPickQuantity = 0;

            var uiElements = model.GenerateUIElements(model.CurrentPick, showAisle: true, showSlot: true, showProductId: true, showProductName: true);

            int quantityToPick = model.CurrentPick.QuantityToPick - model.CurrentPick.QuantityPicked;

            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("BasePicking_EnterQuantity_Header", quantityToPick.ToString()),
                                                                  "enterPickQuantity",
                                                                  Translate.GetLocalizedTextForKey("BasePicking_EnterQuantity_Label"),
                                                                  Translate.GetLocalizedTextForKey("BasePicking_EnterQuantity_Prompt", quantityToPick.ToString()),
                                                                  uiElements,
                                                                  model.CurrentUserMessage,
                                                                  initialPrompt: model.CurrentUserMessage);

            wfo.ValueProperties.MaxAllowedLength = quantityToPick.ToString().Length;

            if (model.CurrentPick.QuantityPicked == 0)
            {
                wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(BasePickingModuleVocab.SkipSlot));
            }

            bool.TryParse(_ConfigRepo.GetConfig("ConfirmQuantityVoiceInput").Value, out bool confirmVoiceInput);
            bool.TryParse(_ConfigRepo.GetConfig("ConfirmQuantityScreenInput").Value, out bool confirmScreenInput);
            wfo.AdditionalProperties.ConfirmVoiceInput = confirmVoiceInput;
            wfo.AdditionalProperties.ConfirmScreenInput = confirmScreenInput;

            wfo.MessageType = model.MessageType;
            model.ConfigureProgressBar(wfo);
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeEnterPickQuantity(SlotContainer slotContainer, IBasePickingModel basePickingModel)
        {
            basePickingModel.ButtonResponse = GenericBaseEncoder<IBasePickingModel>.CheckForButtonPress(slotContainer);
            string response = GenericBaseEncoder<IBasePickingModel>.CheckForButtonPress(slotContainer);
            if (response == null)
            {
                response = GenericBaseEncoder<IBasePickingModel>.DecodeValueEntry(slotContainer);
            }
            basePickingModel.EnteredPickQuantityString = response;
        }

        private void PerformVerifyPickQuantity()
        {
            if (Model.ButtonResponseIsSkipSlot())
            {
                Model.SlotSkippedFromQuantity = true;
                // Leave NextState null to return to the previous state machine
                return;
            }
			else
			{
                Model.SlotSkippedFromQuantity = false;
            }

            bool parsed = int.TryParse(Model.EnteredPickQuantityString, out int enteredQuantity);

            if (!parsed)
            {
                CurrentUserMessage = Translate.GetLocalizedTextForKey("BasePicking_EnterQuantity_Failed");
                NextState = DisplayEnterPickQuantity;
                return;
            }

            Model.EnteredPickQuantity = enteredQuantity;

            int expectedQuantity = Model.CurrentPick.QuantityToPick - Model.CurrentPick.QuantityPicked;
            
            bool.TryParse(_ConfigRepo.GetConfig("PickQuantityCountdown").Value, out bool countdown);

            if (Model.EnteredPickQuantity == expectedQuantity)
            {
                Model.CurrentPick.QuantityPicked += Model.EnteredPickQuantity;
                // Leave NextState null to return to the previous state machine
            }
            else if (Model.EnteredPickQuantity == 0)
            {
                NextState = DisplayConfirmShortProduct;
            }
            else if (Model.EnteredPickQuantity < expectedQuantity)
            {
                if (countdown)
                {
                    Model.CurrentPick.QuantityPicked += Model.EnteredPickQuantity;
                    NextState = DisplayEnterPickQuantity;
                }
                else
                {
                    NextState = DisplayConfirmShortProduct;
                }
            }
            else
            {
                CurrentUserMessage = Translate.GetLocalizedTextForKey("BasePicking_EnterQuantity_Wrong");
                NextState = DisplayEnterPickQuantity;
            }
        }

        private WorkflowObjectContainer EncodeConfirmShortProduct(IBasePickingModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateBooleanIntent(Translate.GetLocalizedTextForKey("BasePicking_ConfirmShortProduct_Header"),
                                                                  "confirmShortProduct",
                                                                  Translate.GetLocalizedTextForKey("BasePicking_ConfirmShortProduct_Prompt"),
                                                                  model.CurrentUserMessage,
                                                                  initialPrompt: model.CurrentUserMessage);

            wfo.MessageType = model.MessageType;
            model.ConfigureProgressBar(wfo);
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodePutInContainer(SlotContainer slotContainer, IBasePickingModel basePickingModel)
        {
            bool? response = GenericBaseEncoder<IBasePickingModel>.DecodeBooleanPrompt(slotContainer);
            basePickingModel.EnteredConfirmShortProductResponse = response ?? false;
        }
    }
}
