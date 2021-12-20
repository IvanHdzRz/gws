//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using Honeywell.Firebird.WorkflowEngine;
    using System.Threading.Tasks;
    using Honeywell.Firebird.CoreLibrary;
    using GuidedWorkRunner;

    /// <summary>
    /// Controller for a WFA where the user enters the quantity picked.
    /// </summary>
    public class WarehousePickingEnterQuantityController : WarehousePickingEnterValueController
    {
        public WarehousePickingEnterQuantityController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner,
            IGuidedWorkStore guidedWorkStore) :
        base(dependencies, guidedWorkRunner, guidedWorkStore)
        {
        }

        public string InfoGlobalWordPrompt { get; set; }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var viewModel = (WarehousePickingEnterDigitsViewModel)base.CreateViewModel(viewModelName);

            var dataStore = DataStore;

            viewModel.InitialPrompt = GetLocalizedText("InitialPrompt", dataStore.RemainingQuantity.ToString());
            viewModel.QuantityEntryVisible = true;
            viewModel.StockCodeResponse = dataStore.CheckDigit;
            InfoGlobalWordPrompt = dataStore.ProductDescription ?? dataStore.ProductName;

            if (dataStore.RemainingQuantity < 10)
            {
                viewModel.ExpectedMaximumLength = 1;
            }
            else if (dataStore.RemainingQuantity < 100)
            {
                viewModel.ExpectedMaximumLength = 2;
            }

            return viewModel;
        }

        protected override bool ValidateResponse(string response)
        {
            ResetUiToInitialState();

            if (!base.ValidateResponse(response))
            {
                return false;
            }

            return !IsResponseGreaterThanExpected(response);
        }

        protected override void OnSuccess(string response)
        {
            var activeWorkflowObject = GuidedWorkStore.GetActiveWorkflowObject();

            if (IsInUserVocab(response))
            {
                activeWorkflowObject.ExtraData["Button"] = response;
            }
            else
            {
                var viewModel = (WarehousePickingEnterDigitsViewModel)ViewModel;
                viewModel.Response = response;
                activeWorkflowObject.Data = response;
            }

            activeWorkflowObject.Modified = true;
            GuidedWorkStore.UpdateCompleted();
        }

        protected override Task OnFailureAsync(string response)
        {
            var viewModel = (WarehousePickingEnterDigitsViewModel)ViewModel;
            if (!string.IsNullOrWhiteSpace(response))
            {
                viewModel.ErrorMessage = GetLocalizedText("Error_WrongQuantity");
            }
            return base.OnFailureAsync(response);
        }

        /// <summary>
        /// Determines if the response string, converted to an int, is greater than expected.
        /// </summary>
        /// <returns>Whether or not the response is greater than expected.</returns>
        /// <param name="response">A valid response (e.g., a string of digits).</param>
        private bool IsResponseGreaterThanExpected(string response)
        {
            int quantityPicked = int.Parse(response);
            int expectedQuantity = DataStore.RemainingQuantity;
            return quantityPicked > expectedQuantity;
        }

        private void ResetUiToInitialState()
        {
            var viewModel = (WarehousePickingEnterDigitsViewModel)ViewModel;
            viewModel.Response = string.Empty;
            viewModel.InitialPrompt = GetLocalizedText("InitialPrompt", DataStore.RemainingQuantity.ToString());
            viewModel.ErrorMessage = string.Empty;
        }
    }
}
