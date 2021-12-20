//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWorkRunner;

    /// <summary>
    /// Displays a Order Picking-style confirmation prompt for the "skip item" command.
    /// </summary>
    /// <remarks>
    /// Displays the item to be skipped, with the
    /// details unique to the Order Picking workflow.
    /// </remarks>
    public class ReceivingBooleanConfirmationController : BooleanConfirmationController
    {
        protected readonly IGuidedWorkRunner GuidedWorkRunner;
        protected readonly IGuidedWorkStore GuidedWorkStore;

        protected ReceivingDataStore DataStore => ReceivingDataStore.DeserializeObject(GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivingBooleanConfirmationController"/> class.
        /// </summary>
        public ReceivingBooleanConfirmationController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore)
            : base(dependencies)
        {
            GuidedWorkRunner = guidedWorkRunner;
            GuidedWorkStore = guidedWorkStore;
        }

        public override bool ShouldAllowBackNavigation()
        {
            GuidedWorkStore.UpdateActiveObjectExtraData("Button", "NavigateBack");
            return false;
        }

        protected override void OnStart(NavigationReason reason)
        {
            base.OnStart(reason);
            GuidedWorkStore.StoreUpdated += OnStoreUpdated;
        }

        protected override void OnStop()
        {
            base.OnStop();
            GuidedWorkStore.StoreUpdated -= OnStoreUpdated;
        }

        /// <summary>
        /// Creates the view model used by the ReceivingBooleanConfirmationController.
        /// </summary>
        /// <param name="viewModelName">ViewModel name to create</param>
        /// <returns>An instance of ReceivingConfirmationViewModel</returns>
        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var viewModel = (ReceivingBooleanConfirmationViewModel)base.CreateViewModel(viewModelName);

            var dataStore = DataStore;

            viewModel.ProductName = dataStore.ProductName;
            viewModel.ProductImage = dataStore.ProductImage;
            viewModel.RemainingQuantity = dataStore.RemainingQuantity;
            viewModel.Instructions = TranslateExtension.GetLocalizedTextForBaseKey("Instructions");
            viewModel.ProductIdentifier = dataStore.ProductIdentifier;
            viewModel.CurrentProductIndex = dataStore.CurrentProductIndex;
            viewModel.TotalProducts = dataStore.TotalProducts;

            return viewModel;
        }

        /// <summary>
        /// Sets the extra data as affirmative.
        /// </summary>
        public override void Affirmative()
        {
            GuidedWorkStore.UpdateActiveObjectExtraData("Button",
                ((ReceivingBooleanConfirmationViewModel)ViewModel).AffirmativeWord);
        }

        /// <summary>
        /// Sets the extra data as negative.
        /// </summary>
        public override void Negative()
        {
            GuidedWorkStore.UpdateActiveObjectExtraData("Button",
                ((ReceivingBooleanConfirmationViewModel)ViewModel).NegativeWord);
        }

        private async void OnStoreUpdated()
        {
            await GuidedWorkRunner.RespondAsync();
            await GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(GuidedWorkRunner.WorkflowEventName);
        }
    }
}
