//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWorkRunner;

    /// <summary>
    /// Controller for a WFA that confirms the quantity picked for a warehouse
    /// picking item and updates the model as appropriate.
    /// </summary>
    public class WarehousePickingConfirmQuantityController : WarehousePickingBooleanConfirmationController
    {
        public WarehousePickingConfirmQuantityController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore)
            : base(dependencies, guidedWorkRunner, guidedWorkStore)
        {
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var viewModel = base.CreateViewModel(viewModelName) as WarehousePickingBooleanConfirmationViewModel;

            var dataStore = DataStore;

            viewModel.InitialPrompt = dataStore.QuantityLastPicked == 0 ? GetLocalizedText("InitialPromptShort") : GetLocalizedText("InitialPrompt", dataStore.QuantityLastPicked.ToString(), dataStore.RemainingQuantity.ToString());

            viewModel.QuantityPicked = dataStore.QuantityLastPicked.ToString();
            viewModel.StockCodeResponse = dataStore.CheckDigit;

            return viewModel;
        }
    }
}
