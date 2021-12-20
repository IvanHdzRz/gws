namespace BasePickingExample
{
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using System.Threading.Tasks;
    public class BasePickingExampleQuantityStateMachine : SimplifiedBaseBusinessLogic<IBasePickingExampleModel, BasePickingExampleBusinessLogic, BasePickingExampleConfigRepository>
    {
        public static readonly CoreAppSMState DisplayEnterPickQuantity = new CoreAppSMState(nameof(DisplayEnterPickQuantity));
        public static readonly CoreAppSMState VerifyPickQuantity = new CoreAppSMState(nameof(VerifyPickQuantity));
        public static readonly CoreAppSMState DisplayConfirmShortProduct = new CoreAppSMState(nameof(DisplayConfirmShortProduct));
        public static readonly CoreAppSMState VerifyConfirmShortProduct = new CoreAppSMState(nameof(VerifyConfirmShortProduct));

        public BasePickingExampleQuantityStateMachine(SimplifiedStateMachineManager<BasePickingExampleBusinessLogic, IBasePickingExampleModel> manager, IBasePickingExampleModel model) : base(manager, model)
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
                new InteractiveItem(BasePickingExampleModuleVocab.ShortProduct, processActionAsync: () => { return Task.FromResult(DisplayConfirmShortProduct); }) 
           });

            ConfigureReturnLogicState(VerifyPickQuantity,
                                      () =>
                                      {
                                          bool parsed = int.TryParse(Model.EnteredPickQuantityString, out int enteredQuantity);

                                          if (!parsed)
                                          {
                                              CurrentUserMessage = Translate.GetLocalizedTextForKey("BasePicking_EnterQuantity_Failed");
                                              NextState = DisplayEnterPickQuantity;
                                              return;
                                          }

                                          Model.EnteredPickQuantity = enteredQuantity;

                                          int expectedQuantity = Model.CurrentPick.QuantityToPick - Model.CurrentPick.QuantityPicked;

                                          if (Model.EnteredPickQuantity == expectedQuantity)
                                          {
                                              Model.CurrentPick.QuantityPicked += Model.EnteredPickQuantity;
                                  // Leave NextState null to return to the previous state machine
                              }
                                          else if (Model.EnteredPickQuantity == 0)
                                          {
                                  // TODO: implement confirm short
                                  NextState = DisplayConfirmShortProduct;
                              }
                                          else if (Model.EnteredPickQuantity < expectedQuantity)
                                          {
                                              Model.CurrentPick.QuantityPicked += Model.EnteredPickQuantity;
                                              NextState = DisplayEnterPickQuantity;
                                          }
                                          else
                                          {
                                              CurrentUserMessage = Translate.GetLocalizedTextForKey("BasePicking_EnterQuantity_Wrong");
                                              NextState = DisplayEnterPickQuantity;
                                          }
                                      },
                                      DisplayConfirmShortProduct, 
                                      DisplayEnterPickQuantity);
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

        private WorkflowObjectContainer EncodeEnterPickQuantity(IBasePickingExampleModel model)
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
                wfo.AdditionalOverflowMenuOptions.Add(
                    new InteractiveItem(
                        BasePickingExampleModuleVocab.SkipSlot,
                        processActionAsync: () =>
                        {
                    // NOTE: This is going to the skip slot state in the pick state machine
                    return Task.FromResult(BasePickingExamplePickStateMachine.SkipSlot);
                        }));
            }

            wfo.AdditionalProperties.ConfirmVoiceInput = true;
            wfo.AdditionalProperties.ConfirmScreenInput = true;

            wfo.MessageType = model.MessageType;
            model.ConfigureProgressBar(wfo);
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeEnterPickQuantity(SlotContainer slotContainer, IBasePickingExampleModel basePickingModel)
        {
            string response = GenericBaseEncoder<IBasePickingExampleModel>.DecodeValueEntry(slotContainer);
            basePickingModel.EnteredPickQuantityString = response;
        }

        private WorkflowObjectContainer EncodeConfirmShortProduct(IBasePickingExampleModel model)
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

        private void DecodePutInContainer(SlotContainer slotContainer, IBasePickingExampleModel basePickingModel)
        {
            bool? response = GenericBaseEncoder<IBasePickingExampleModel>.DecodeBooleanPrompt(slotContainer);
            basePickingModel.EnteredConfirmShortProductResponse = response ?? false;
        }
    }
}