//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;

    /// <summary>
    /// Controller for a WFA that confirms the quantity picked for an order
    /// picking item and updates the model as appropriate.
    /// </summary>
    public class OrderPickingConfirmQuantityController : OrderPickingBooleanConfirmationController
    {
        private readonly IGuidedWorkStore _GuidedWorkStore;

        /// <summary>
        /// The names of the events that this workflow publishes.
        /// </summary>
        public const string MoreWorkEventName = "MoreWork";
        public const string PickSubstitutionEventName = "PickSubstitution";

        public OrderPickingConfirmQuantityController(CoreViewControllerDependencies dependencies, 
            IGuidedWorkRunner guidedWorkRunner,
            IGuidedWorkStore guidedWorkStore)
            : base(dependencies, guidedWorkRunner, guidedWorkStore)
        {
            _GuidedWorkStore = guidedWorkStore;
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var viewModel = base.CreateViewModel(viewModelName) as OrderPickingBooleanConfirmationViewModel;

            viewModel.Container = _DataStore.CurrentPickingContainer.Identifier;
            viewModel.InitialPrompt = GetLocalizedText("InitialPrompt", _DataStore.QuantityLastPicked.ToString(), _DataStore.CurrentPickingContainer.Identifier);
            viewModel.QuantityPicked = _DataStore.QuantityLastPicked.ToString();
            viewModel.StockCodeResponse = _DataStore.StockCodeResponse;

            return viewModel;
        }
    }
}
