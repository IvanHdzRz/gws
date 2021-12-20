//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using GuidedWorkRunner;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Honeywell.Firebird.WorkflowEngine;

    /// <summary>
    /// Displays a Order Picking-style confirmation prompt for the "skip item" command.
    /// </summary>
    /// <remarks>
    /// Displays the item to be skipped, with the
    /// details unique to the Order Picking workflow.
    /// </remarks>
    public class OrderPickingBooleanConfirmationController : BooleanConfirmationController
    {
        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        protected OrderPickingDataStore _DataStore => OrderPickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderPickingBooleanConfirmationController"/> class.
        /// </summary>
        public OrderPickingBooleanConfirmationController(CoreViewControllerDependencies dependencies,
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
        /// Creates the view model used by the OrderPickingBooleanConfirmationController.
        /// </summary>
        /// <param name="viewModelName">ViewModel name to create</param>
        /// <returns>An instance of OrderPickingConfirmationViewModel</returns>
        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var viewModel = (OrderPickingBooleanConfirmationViewModel)base.CreateViewModel(viewModelName);

            viewModel.ExpectedStockCodeResponseLength = _DataStore.ExpectedStockCodeResponseLength;
            viewModel.LastDigitsLabel = GetLocalizedText("LastDigits", _DataStore.ExpectedStockCodeResponseLength.ToString());
            viewModel.OrderIdentifier = _DataStore.OrderIdentifier;
            viewModel.Price = _DataStore.Price;
            viewModel.ProductName = _DataStore.ProductName;
            viewModel.ProductImage = _DataStore.ProductImage;
            viewModel.Size = _DataStore.Size;
            viewModel.LocationDescriptors = _DataStore.LocationDescriptors;
            viewModel.RemainingQuantity = _DataStore.RemainingQuantity.ToString();
            viewModel.Instructions = TranslateExtension.GetLocalizedTextForBaseKey("Instructions");
            viewModel.ProductIdentifier = _DataStore.ProductIdentifier;
            viewModel.CurrentProductIndex = _DataStore.CurrentProductIndex.ToString();
            viewModel.TotalProducts = _DataStore.TotalProducts.ToString();

            return viewModel;
        }

        /// <summary>
        /// Sets the extra data as affirmative.
        /// </summary>
        public override void Affirmative()
        {
            _GuidedWorkStore.UpdateActiveObjectExtraData("Button",
                ((OrderPickingBooleanConfirmationViewModel)ViewModel).AffirmativeWord);
        }

        /// <summary>
        /// Sets the extra data as negative.
        /// </summary>
        public override void Negative()
        {
            _GuidedWorkStore.UpdateActiveObjectExtraData("Button",
                ((OrderPickingBooleanConfirmationViewModel)ViewModel).NegativeWord);
        }

        private async void OnStoreUpdated()
        {
            await _GuidedWorkRunner.RespondAsync();
            await _GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(_GuidedWorkRunner.WorkflowEventName);
        }
    }
}
