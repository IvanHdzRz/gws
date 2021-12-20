namespace BasePickingExample
{
    using Common.Logging;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using System;
    using System.Linq;
    using Honeywell.DialogueRunner;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    public class BasePickingExamplePickStateMachine : SimplifiedBaseBusinessLogic<IBasePickingExampleModel, BasePickingExampleBusinessLogic, BasePickingExampleConfigRepository>
    {
        public static readonly CoreAppSMState GetPicks = new CoreAppSMState(nameof(GetPicks));
        public static readonly CoreAppSMState DisplayAisle = new CoreAppSMState(nameof(DisplayAisle));

        public static readonly CoreAppSMState DisplayEnterPickLocation = new CoreAppSMState(nameof(DisplayEnterPickLocation));
        public static readonly CoreAppSMState VerifyEnterPickLocation = new CoreAppSMState(nameof(VerifyEnterPickLocation));
        public static readonly CoreAppSMState DisplayEnterPickProduct = new CoreAppSMState(nameof(DisplayEnterPickProduct));
        public static readonly CoreAppSMState StartQuantitySM = new CoreAppSMState(nameof(StartQuantitySM));
        public static readonly CoreAppSMState HandleQuantitySMComplete = new CoreAppSMState(nameof(HandleQuantitySMComplete));
        public static readonly CoreAppSMState UpdatePick = new CoreAppSMState(nameof(UpdatePick));
        public static readonly CoreAppSMState SkipSlot = new CoreAppSMState(nameof(SkipSlot));

        public static readonly CoreAppSMState CheckForMoreWork = new CoreAppSMState(nameof(CheckForMoreWork));

        public static readonly CoreAppSMState DisplayPickAssignmentComplete = new CoreAppSMState(nameof(DisplayPickAssignmentComplete));
        public static readonly CoreAppSMState ExitPickSM = new CoreAppSMState(nameof(ExitPickSM));

        private readonly ILog _Log = LogManager.GetLogger(nameof(BasePickingExamplePickStateMachine));
        private BasePickingExampleQuantityStateMachine _QuantitySM;
        private BasePickingExampleQuantityStateMachine QuantitySM { get { return Manager.CreateStateMachine(ref _QuantitySM); } }
        public BasePickingExamplePickStateMachine(SimplifiedStateMachineManager<BasePickingExampleBusinessLogic, IBasePickingExampleModel> manager, IBasePickingExampleModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {

            ConfigureReturnLogicState(GetPicks,
                          async () =>
                          {
                              try
                              {
                                  Model.Picks = await Model.ServiceProvider.GetPicksAsync(Model.CurrentOperator.OperatorIdentifier);
                                  NextState = CheckForMoreWork;
                              }
                              // TODO: handle other exceptions or errors depending on communication mechanism
                              catch (OperationCanceledException)
                              {
                                  _Log.Debug("Operation canceled via communication Timeout Handler");
                                  Model.CurrentUserMessage = Translate.GetLocalizedTextForKey("BasePicking_NetworkTimeout_Error");
                              }

                              // Leave NextState null to return to the previous state machine
                          },
                          CheckForMoreWork);

            ConfigureDisplayState(DisplayAisle,
                DisplayEnterPickLocation,
                encodeAction: EncodeAisle);


            ConfigureDisplayState(DisplayEnterPickLocation,
                  VerifyEnterPickLocation,
                  encodeAction: EncodeEnterLocation,
                  decodeAction: DecodeEnterLocation,
                  availableOverflowMenuItems: new InteractiveItem[]
                    {
                        new InteractiveItem(BasePickingExampleModuleVocab.SkipSlot, processActionAsync: () => { return Task.FromResult(SkipSlot); }) 
                    }
                );
                   

            ConfigureLogicState(VerifyEnterPickLocation,
                                () =>
                                {
                                    if (Model.LocationCheckDigitResponse == Model.CurrentPick.CheckDigits)
                                    {
                                        NextState = DisplayEnterPickProduct;
                                        return;
                                    }

                                    CurrentUserMessage = Translate.GetLocalizedTextForKey("BasePicking_Location_WrongCheckDigit");
                                    NextState = DisplayEnterPickLocation;
                                },
                                DisplayEnterPickLocation,
                                DisplayEnterPickProduct);

            ConfigureDisplayState(DisplayEnterPickProduct,
                      StartQuantitySM,
                      encodeAction: EncodeEnterProduct,
                      decodeAction: DecodeEnterProduct,
                      availableOverflowMenuItems: new InteractiveItem[]
                      {
                          new InteractiveItem(BasePickingExampleModuleVocab.SkipSlot, processActionAsync: () => { return Task.FromResult(SkipSlot); })
             });

            ConfigureLogicState(StartQuantitySM,
                    async () =>
                    {
                        NextState = HandleQuantitySMComplete;
                        QuantitySM.Reset();
                        await QuantitySM.InitializeStateMachineAsync();
                    },
                    HandleQuantitySMComplete);

            ConfigureLogicState(HandleQuantitySMComplete,
                                () =>
                                {
                                    backgroundActivityNextState = UpdatePick;
                                    CurrentBackGroundActivityHeader = Translate.GetLocalizedTextForKey("BasePicking_BackgroundActivity_Header_UpdatePick");
                                    NextState = CoreAppStates.BackgroundActvity;
                                },
                                CoreAppStates.BackgroundActvity);

            ConfigureReturnLogicState(UpdatePick,
                                      async () =>
                                      {
                                          try
                                          {
                                              await Model.ServiceProvider.UpdatePickAsync(Model.CurrentPick.PickId, Model.CurrentPick.QuantityPicked);
                                              Model.CurrentPick.Picked = true;

                                              NextState = CheckForMoreWork;
                                          }
                                          catch (Exception) { }

                              // Leave NextState null to return to the previous state machine
                          },
                                      CheckForMoreWork);

            ConfigureLogicState(SkipSlot,
                    () =>
                    {
                        var picksToSkip = new List<Pick>();
                        foreach (var pick in Model.Picks)
                        {
                            if (pick.Aisle == Model.CurrentPick.Aisle && pick.Slot == Model.CurrentPick.Slot)
                            {
                                picksToSkip.Add(pick);
                            }
                        }
                        foreach (var pick in picksToSkip)
                        {
                            Model.Picks.Remove(pick);
                            Model.Picks.Add(pick);
                        }
                        NextState = CheckForMoreWork;
                    },
                    CheckForMoreWork);

            ConfigureLogicState(CheckForMoreWork,
                    () =>
                    {
                        NextState = DisplayPickAssignmentComplete;
                        string lastAisle = Model.CurrentPick?.Aisle;
                        Model.CurrentPick = Model.Picks.FirstOrDefault(p => !p.Picked);
                        if (Model.CurrentPick != null)
                        {
                            if (Model.CurrentPick.Aisle == lastAisle)
                            {
                                // TODO: Implement state
                                NextState = DisplayEnterPickLocation;
                            }
                            else
                            {
                                // TODO: Implement state
                                NextState = DisplayAisle;
                            }
                        }
                    },
                    DisplayAisle,
                    DisplayEnterPickLocation,
                    DisplayPickAssignmentComplete);

            ConfigureDisplayState(DisplayPickAssignmentComplete,
                                  ExitPickSM,
                                  encodeAction: EncodeAssignmentComplete);

            ConfigureReturnLogicState(ExitPickSM,
                                      () =>
                                      {
                                          // Leave NextState null to return to the previous state machine
                                      });
            
        }

        private WorkflowObjectContainer EncodeAisle(IBasePickingExampleModel model)
        {
            var uiElements = model.GenerateUIElements(model.CurrentPick, showAisle: true);

            var wfoContainer = new WorkflowObjectContainer();

            var wfo = WorkflowObjectFactory.CreateReadyUIElementIntent(Translate.GetLocalizedTextForKey("BasePicking_Aisle_Header"),
                                                                       "aisle",
                                                                       Translate.GetLocalizedTextForKey("BasePicking_Aisle_Prompt", model.CurrentPick.Aisle),
                                                                       uiElements,
                                                                       model.CurrentUserMessage,
                                                                       initialPrompt: model.CurrentUserMessage);

            wfo.MessageType = model.MessageType;
            model.ConfigureProgressBar(wfo);
            wfoContainer.Add(wfo);
            return wfoContainer;
        }
        private WorkflowObjectContainer EncodeEnterLocation(IBasePickingExampleModel model)
        {
            var uiElements = model.GenerateUIElements(model.CurrentPick, showAisle: true, showSlot: true);

            string slotAsDigits = CommonDialogueUtils.SpellDigits(model.CurrentPick.Slot);

            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("BasePicking_EnterLocation_Header"),
                                                                  "enterLocation",
                                                                  Translate.GetLocalizedTextForKey("BasePicking_EnterLocation_Label"),
                                                                  Translate.GetLocalizedTextForKey("BasePicking_EnterLocation_Prompt", slotAsDigits),
                                                                  uiElements,
                                                                  model.CurrentUserMessage,
                                                                  initialPrompt: model.CurrentUserMessage);

            wfo.ValueProperties.MinRequiredLength = model.CurrentPick.CheckDigits.Length;
            wfo.ValueProperties.MaxAllowedLength = model.CurrentPick.CheckDigits.Length;

            wfo.ValueProperties.Placeholder = model.CurrentPick.CheckDigits;

            wfo.MessageType = model.MessageType;
            model.ConfigureProgressBar(wfo);
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeEnterLocation(SlotContainer slotContainer, IBasePickingExampleModel basePickingModel)
        {
            string response = GenericBaseEncoder<IBasePickingExampleModel>.DecodeValueEntry(slotContainer);
            basePickingModel.LocationCheckDigitResponse = response;
        }
        private WorkflowObjectContainer EncodeEnterProduct(IBasePickingExampleModel model)
        {
            var uiElements = model.GenerateUIElements(model.CurrentPick, showAisle: true, showSlot: true, showProductId: true, showProductName: true);

            var wfoContainer = new WorkflowObjectContainer();

            WorkflowObject wfo;

            wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("BasePicking_EnterProduct_Header"),
                                                                    "enterProduct",
                                                                    Translate.GetLocalizedTextForKey("BasePicking_EnterProduct_Label"),
                                                                    model.CurrentPick.ProductName,
                                                                    uiElements,
                                                                    model.CurrentUserMessage,
                                                                    initialPrompt: model.CurrentUserMessage);

            wfo.ValueProperties.MinRequiredLength = model.CurrentPick.ProductSpokenVerification.Length;
            wfo.ValueProperties.MaxAllowedLength = model.CurrentPick.ProductSpokenVerification.Length;
            wfo.ValueProperties.ExpectedSpokenOrTypedValues.Add(model.CurrentPick.ProductSpokenVerification);
            wfo.ValueProperties.ExpectedScannedValues.Add(model.CurrentPick.ProductScannedVerification);

            wfo.ValueProperties.Placeholder = model.CurrentPick.ProductSpokenVerification;

            wfo.MessageType = model.MessageType;
            model.ConfigureProgressBar(wfo);
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeEnterProduct(SlotContainer slotContainer, IBasePickingExampleModel basePickingModel)
        {
            string response = GenericBaseEncoder<IBasePickingExampleModel>.DecodeValueEntry(slotContainer);
            basePickingModel.ProductBatchNumberResponse = response;
        }
        private WorkflowObjectContainer EncodeAssignmentComplete(IBasePickingExampleModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();

            var wfo = WorkflowObjectFactory.CreateReadyUIElementIntent(Translate.GetLocalizedTextForKey("BasePicking_AssignmentComplete_Header"),
                                                           "assignmentComplete",
                                                           Translate.GetLocalizedTextForKey("BasePicking_AssignmentComplete_Prompt"),
                                                           null,
                                                           model.CurrentUserMessage,
                                                           initialPrompt: model.CurrentUserMessage);

            wfo.MessageType = model.MessageType;
            model.ConfigureProgressBar(wfo);
            wfoContainer.Add(wfo);
            return wfoContainer;
        }
    }
}