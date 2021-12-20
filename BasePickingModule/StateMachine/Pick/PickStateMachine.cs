//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using Common.Logging;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.DialogueRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class PickStateMachine : SimplifiedBaseBusinessLogic<IBasePickingModel, BasePickingStateMachine, IBasePickingConfigRepository>
    {
        public static readonly CoreAppSMState GetPicks = new CoreAppSMState(nameof(GetPicks));

        public static readonly CoreAppSMState DisplayAisle = new CoreAppSMState(nameof(DisplayAisle));
        public static readonly CoreAppSMState VerifyAisle = new CoreAppSMState(nameof(VerifyAisle));

        public static readonly CoreAppSMState CheckIfConfirmPickLocation = new CoreAppSMState(nameof(CheckIfConfirmPickLocation));
        public static readonly CoreAppSMState DisplayEnterPickLocation = new CoreAppSMState(nameof(DisplayEnterPickLocation));
        public static readonly CoreAppSMState VerifyEnterPickLocation = new CoreAppSMState(nameof(VerifyEnterPickLocation));

        public static readonly CoreAppSMState SkipSlot = new CoreAppSMState(nameof(SkipSlot));

        public static readonly CoreAppSMState CheckIfConfirmProduct = new CoreAppSMState(nameof(CheckIfConfirmProduct));
        public static readonly CoreAppSMState DisplayEnterPickProduct = new CoreAppSMState(nameof(DisplayEnterPickProduct));
        public static readonly CoreAppSMState VerifyEnterPickProduct = new CoreAppSMState(nameof(VerifyEnterPickProduct));

        public static readonly CoreAppSMState DeterminePickMethod = new CoreAppSMState(nameof(DeterminePickMethod));

        public static readonly CoreAppSMState HandlePickMethodSMComplete = new CoreAppSMState(nameof(HandlePickMethodSMComplete));

        public static readonly CoreAppSMState UpdatePick = new CoreAppSMState(nameof(UpdatePick));

        public static readonly CoreAppSMState CheckForMoreWork = new CoreAppSMState(nameof(CheckForMoreWork));

        public static readonly CoreAppSMState DisplayPickAssignmentComplete = new CoreAppSMState(nameof(DisplayPickAssignmentComplete));

        public static readonly CoreAppSMState ExitPickSM = new CoreAppSMState(nameof(ExitPickSM));

        private readonly ILog _Log = LogManager.GetLogger(nameof(PickStateMachine));

        private DiscretePickStateMachine _DiscretePickSM;
        protected DiscretePickStateMachine DiscretePickSM { get { return Manager.CreateStateMachine(ref _DiscretePickSM); } }

        private ClusterPickStateMachine _ClusterPickSM;
        protected ClusterPickStateMachine ClusterPickSM { get { return Manager.CreateStateMachine(ref _ClusterPickSM); } }

        public PickStateMachine(SimplifiedStateMachineManager<BasePickingStateMachine, IBasePickingModel> manager, IBasePickingModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            ConfigureReturnLogicState(GetPicks,
                                      async () =>
                                      {
                                          try
                                          {
                                              Model.Picks = await Model.DataService.GetPicksAsync();
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
                                  VerifyAisle,
                                  encodeAction: EncodeAisle);

            ConfigureLogicState(VerifyAisle,
                                () =>
                                {
                                    NextState = CheckIfConfirmPickLocation;
                                },
                                CheckIfConfirmPickLocation);

            ConfigureLogicState(CheckIfConfirmPickLocation,
                                () =>
                                {
                                    bool.TryParse(_ConfigRepo.GetConfig("ConfirmLocation").Value, out bool confirmLocation);
                                    NextState = confirmLocation ? DisplayEnterPickLocation : CheckIfConfirmProduct;
                                },
                                DisplayEnterPickLocation,
                                CheckIfConfirmProduct);

            ConfigureDisplayState(DisplayEnterPickLocation,
                                  VerifyEnterPickLocation,
                                  encodeAction: EncodeEnterLocation,
                                  decodeAction: DecodeEnterLocation,
                                  availableOverflowMenuItems: new InteractiveItem[]
                                  {
                                      new InteractiveItem(BasePickingModuleVocab.SkipSlot, processActionAsync: () => { return Task.FromResult(SkipSlot); })
                                  });

            ConfigureLogicState(VerifyEnterPickLocation,
                                () =>
                                {
                                    if (Model.LocationCheckDigitResponse == Model.CurrentPick.CheckDigits)
                                    {
                                        NextState = CheckIfConfirmProduct;
                                        return;
                                    }

                                    CurrentUserMessage = Translate.GetLocalizedTextForKey("BasePicking_Location_WrongCheckDigit");
                                    NextState = DisplayEnterPickLocation;
                                },
                                DisplayEnterPickLocation,
                                CheckIfConfirmProduct,
                                SkipSlot);

            /// <summary>
            /// Puts all picks from the start of the pick list with the same slot at the end of the pick list
            /// </summary>
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

            ConfigureLogicState(CheckIfConfirmProduct,
                                () =>
                                {
                                    bool.TryParse(_ConfigRepo.GetConfig("ConfirmProduct").Value, out bool confirmProduct);
                                    NextState = confirmProduct ? DisplayEnterPickProduct : DeterminePickMethod;
                                },
                                DisplayEnterPickProduct,
                                DeterminePickMethod);

            ConfigureDisplayState(DisplayEnterPickProduct,
                                  VerifyEnterPickProduct,
                                  encodeAction: EncodeEnterProduct,
                                  decodeAction: DecodeEnterProduct,
                                  availableOverflowMenuItems: new InteractiveItem[]
                                  {
                                      new InteractiveItem(BasePickingModuleVocab.SkipSlot, processActionAsync: () => { return Task.FromResult(SkipSlot); })
                                  });

            ConfigureLogicState(VerifyEnterPickProduct,
                                () =>
                                {
                                    // if (verification fails)
                                    // {
                                    //     NextState = DisplayEnterPickProduct;
                                    // }

                                    NextState = DeterminePickMethod;
                                },
                                DisplayEnterPickProduct,
                                DeterminePickMethod);

            ConfigureLogicState(DeterminePickMethod,
                                async () =>
                                {
                                    NextState = HandlePickMethodSMComplete;
                                    if (_ConfigRepo.GetConfig("PickMethod").Value == BasePickingPickMethod.Discrete)
                                    {
                                        await StartDiscretePickStateMachineAsync();
                                        return;
                                    }
                                    if (_ConfigRepo.GetConfig("PickMethod").Value == BasePickingPickMethod.Cluster)
                                    {
                                        await StartClusterPickStateMachineAsync();
                                        return;
                                    }
                                },
                                HandlePickMethodSMComplete);

            ConfigureLogicState(HandlePickMethodSMComplete,
                                () =>
                                {
                                    if (Model.SlotSkippedFromQuantity)
                                    {
                                        NextState = SkipSlot;
                                    }
                                    else
                                    {
                                        backgroundActivityNextState = UpdatePick;
                                        CurrentBackGroundActivityHeader = Translate.GetLocalizedTextForKey("BasePicking_BackgroundActivity_Header_UpdatePick");
                                        NextState = CoreAppStates.BackgroundActvity;
                                    }
                                },
                                SkipSlot,
                                CoreAppStates.BackgroundActvity);

            ConfigureReturnLogicState(UpdatePick,
                                      async () =>
                                      {
                                          try
                                          {
                                              await Model.DataService.UpdatePickAsync(Model.CurrentPick.PickId, Model.CurrentPick.QuantityPicked);
                                              Model.CurrentPick.Picked = true;

                                              NextState = CheckForMoreWork;
                                          }
                                          catch (Exception) { }

                                          // Leave NextState null to return to the previous state machine
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
                                            NextState = CheckIfConfirmPickLocation;
                                        }
                                        else
                                        {
                                            NextState = DisplayAisle;
                                        }
                                    }
                                },
                                DisplayAisle,
                                CheckIfConfirmPickLocation,
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

        private WorkflowObjectContainer EncodeAisle(IBasePickingModel model)
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

        private WorkflowObjectContainer EncodeEnterLocation(IBasePickingModel model)
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
            wfo.ValueProperties.ExpectedSpokenOrTypedValues.Add(model.CurrentPick.CheckDigits);

            bool.TryParse(_ConfigRepo.GetConfig("ShowHints").Value, out bool showHints);
            if (showHints)
            {
                wfo.ValueProperties.Placeholder = model.CurrentPick.CheckDigits;
            }

            wfo.MessageType = model.MessageType;
            model.ConfigureProgressBar(wfo);
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeEnterLocation(SlotContainer slotContainer, IBasePickingModel basePickingModel)
        {
            basePickingModel.ButtonResponse = GenericBaseEncoder<IBasePickingModel>.CheckForButtonPress(slotContainer);
            string response = GenericBaseEncoder<IBasePickingModel>.DecodeValueEntry(slotContainer);
            basePickingModel.LocationCheckDigitResponse = response;
        }

        private WorkflowObjectContainer EncodeEnterProduct(IBasePickingModel model)
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

            bool.TryParse(_ConfigRepo.GetConfig("ShowHints").Value, out bool showHints);
            if (showHints)
            {
                wfo.ValueProperties.Placeholder = model.CurrentPick.ProductSpokenVerification;
            }

            wfo.MessageType = model.MessageType;
            model.ConfigureProgressBar(wfo);
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeEnterProduct(SlotContainer slotContainer, IBasePickingModel basePickingModel)
        {
            basePickingModel.ButtonResponse = GenericBaseEncoder<IBasePickingModel>.CheckForButtonPress(slotContainer);
            string response = GenericBaseEncoder<IBasePickingModel>.DecodeValueEntry(slotContainer);
            basePickingModel.ProductBatchNumberResponse = response;
        }

        private WorkflowObjectContainer EncodeAssignmentComplete(IBasePickingModel model)
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

        private async Task StartDiscretePickStateMachineAsync()
        {
            DiscretePickSM.Reset();
            await DiscretePickSM.InitializeStateMachineAsync();
        }

        private async Task StartClusterPickStateMachineAsync()
        {
            ClusterPickSM.Reset();
            await ClusterPickSM.InitializeStateMachineAsync();
        }
    }
}
