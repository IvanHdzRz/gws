//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWorkRunner;

    /// <summary>
    /// Displays a Warehouse Picking-style confirmation prompt for the "skip item" command.
    /// </summary>
    /// <remarks>
    /// Displays the item to be skipped, with the
    /// details unique to the Warehouse Picking workflow.
    /// </remarks>
    public class WarehousePickingBooleanConfirmationController : BooleanConfirmationController
    {
        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        protected WarehousePickingDataStore DataStore => WarehousePickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        /// <summary>
        /// Initializes a new instance of the <see cref="WarehousePickingBooleanConfirmationController"/> class.
        /// </summary>
        public WarehousePickingBooleanConfirmationController(CoreViewControllerDependencies dependencies,
                                                   IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore)
            : base(dependencies)
        {
            _GuidedWorkRunner = guidedWorkRunner;
            _GuidedWorkStore = guidedWorkStore;
        }

        public override bool ShouldAllowBackNavigation()
        {
            _GuidedWorkStore.UpdateActiveObjectExtraData("Button", "NavigateBack");
            return false;
        }

        protected override void OnStart(NavigationReason reason)
        {
            base.OnStart(reason);
            _GuidedWorkStore.StoreUpdated += OnStoreUpdated;
        }

        protected override void OnStop()
        {
            base.OnStop();
            _GuidedWorkStore.StoreUpdated -= OnStoreUpdated;
        }

        /// <summary>
        /// Creates the view model used by the WarehousePickingBooleanConfirmationController.
        /// </summary>
        /// <param name="viewModelName">ViewModel name to create</param>
        /// <returns>An instance of WarehousePickingConfirmationViewModel</returns>
        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var viewModel = (WarehousePickingBooleanConfirmationViewModel)base.CreateViewModel(viewModelName);

            var dataStore = DataStore;

            viewModel.TripIdentifier = dataStore.TripIdentifier;
            viewModel.ProductName = dataStore.ProductName;
            viewModel.ProductImage = dataStore.ProductImage;
            viewModel.LocationDescriptors = dataStore.LocationDescriptors;
            viewModel.RemainingQuantity = dataStore.RemainingQuantity.ToString();
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
            _GuidedWorkStore.UpdateActiveObjectExtraData("Button",
                ((WarehousePickingBooleanConfirmationViewModel) ViewModel).AffirmativeWord);
        }

        /// <summary>
        /// Sets the extra data as negative.
        /// </summary>
        public override void Negative()
        {
            _GuidedWorkStore.UpdateActiveObjectExtraData("Button",
                ((WarehousePickingBooleanConfirmationViewModel)ViewModel).NegativeWord);
        }

        private async void OnStoreUpdated()
        {
            await _GuidedWorkRunner.RespondAsync();
            await _GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(_GuidedWorkRunner.WorkflowEventName);
        }
    }
}
