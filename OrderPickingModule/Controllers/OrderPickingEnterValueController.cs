//////////////////////////////////////////////////////////////////////////////
//     Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using GuidedWorkRunner;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Honeywell.Firebird.WorkflowEngine;
    using System.Collections.Generic;

    /// <summary>
    /// Abstract controller that encapulates the common features of the digit entry WFAs for Order Picking.
    /// </summary>
    public abstract class OrderPickingEnterValueController : DigitEntryController
    {
        protected readonly IGuidedWorkRunner _GuidedWorkRunner;
        protected readonly IGuidedWorkStore _GuidedWorkStore;

        protected OrderPickingDataStore _DataStore => OrderPickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        protected OrderPickingEnterValueController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) :
        base(dependencies)
        {
            _GuidedWorkRunner = guidedWorkRunner;
            _GuidedWorkStore = guidedWorkStore;
        }

        public override bool ShouldAllowBackNavigation()
        {
            _GuidedWorkStore.UpdateActiveObjectExtraData("Button", "NavigateBack");
            return true;
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

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var viewModel = (OrderPickingEnterDigitsViewModel)base.CreateViewModel(viewModelName);

            viewModel.ExpectedStockCodeResponseLength = _DataStore.ExpectedStockCodeResponseLength;
            viewModel.LastDigitsLabel = GetLocalizedText("LastDigits", _DataStore.ExpectedStockCodeResponseLength.ToString());
            viewModel.OrderIdentifier = _DataStore.OrderIdentifier;
            viewModel.Price = _DataStore.Price;
            viewModel.ProductImage = _DataStore.ProductImage;
            viewModel.ProductName = _DataStore.ProductName;
            viewModel.Size = _DataStore.Size;
            viewModel.LocationDescriptors = _DataStore.LocationDescriptors;
            viewModel.RemainingQuantity = _DataStore.RemainingQuantity.ToString();
            viewModel.Instructions = TranslateExtension.GetLocalizedTextForBaseKey("Instructions");
            viewModel.OverflowMenuItems = new List<string>
            {
                GetLocalizedText("VocabWord_OrderStatus")
            };
            viewModel.ProductIdentifier = _DataStore.ProductIdentifier;
            viewModel.CurrentProductIndex = _DataStore.CurrentProductIndex.ToString();
            viewModel.TotalProducts = _DataStore.TotalProducts.ToString();

            return viewModel;
        }

        private async void OnStoreUpdated()
        {
            await _GuidedWorkRunner.RespondAsync();
            await _GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(_GuidedWorkRunner.WorkflowEventName);
        }
    }
}
