//////////////////////////////////////////////////////////////////////////////
//     Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWorkRunner;

    /// <summary>
    /// Abstract controller that encapulates the common features of the digit entry WFAs for Warehouse Picking.
    /// </summary>
    public abstract class WarehousePickingEnterValueController : DigitEntryController
    {
        protected readonly IGuidedWorkRunner GuidedWorkRunner;
        protected readonly IGuidedWorkStore GuidedWorkStore;

        protected WarehousePickingDataStore DataStore => WarehousePickingDataStore.DeserializeObject(GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        protected WarehousePickingEnterValueController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) : 
        base(dependencies)
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

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var viewModel = (WarehousePickingEnterDigitsViewModel)base.CreateViewModel(viewModelName);

            var dataStore = DataStore;

            viewModel.TripIdentifier = dataStore.TripIdentifier;
            viewModel.ProductImage = dataStore.ProductImage;
            viewModel.ProductName = dataStore.ProductName;
            viewModel.LocationDescriptors = dataStore.LocationDescriptors;
            viewModel.RemainingQuantity = dataStore.RemainingQuantity.ToString();
            viewModel.Instructions = TranslateExtension.GetLocalizedTextForBaseKey("Instructions");
            viewModel.ProductIdentifier = dataStore.ProductIdentifier;
            viewModel.CurrentProductIndex = dataStore.CurrentProductIndex;
            viewModel.TotalProducts = dataStore.TotalProducts;
            return viewModel;
        }

        private async void OnStoreUpdated()
        {
            await GuidedWorkRunner.RespondAsync();
            await GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(GuidedWorkRunner.WorkflowEventName);
        }
    }
}
