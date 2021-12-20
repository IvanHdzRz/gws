//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2015 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GuidedWorkRunner;
    using Honeywell.DialogueRunner;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;

    /// <summary>
    /// Controller for a WFA where the user enters the quantity picked.
    /// </summary>
    public class OrderPickingEnterQuantityController : OrderPickingEnterValueController
    {
        public const string ConfirmQuantityEventName = "ConfirmQuantity";
        public const string OverflowEventName = "Overflow";

        private OrderPickingEnterDigitsViewModel _ViewModel;

        public OrderPickingEnterQuantityController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) :
        base(dependencies, guidedWorkRunner, guidedWorkStore)
        {
        }

        public string InfoGlobalWordPrompt { get; set; }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            _ViewModel = (OrderPickingEnterDigitsViewModel)base.CreateViewModel(viewModelName);

            _ViewModel.Container = _DataStore.CurrentPickingContainer.Identifier;
            _ViewModel.InitialPrompt = GetInitialPrompt();
            _ViewModel.StockCodeResponse = _DataStore.StockCodeResponse;
            InfoGlobalWordPrompt = (_DataStore.ProductDescription != null) ? _DataStore.ProductDescription : _DataStore.ProductName;
            
            return _ViewModel;
        }

        protected override bool ValidateResponse(string response)
        {
            ResetUiToInitialState();

            // Check if the response is a valid vocab word
            if (IsInUserVocab(response))
            {
                return true;
            }

            if (!base.ValidateResponse(response))
            {
                return false;
            }

            //Checks if the returned string length is longer than a 32bit integer
            if (!int.TryParse(response, out _))
            {
                return false;
            }
            //Checks if returned string length is longer than a user set length parameter
            if (response.Length > _ViewModel.ExpectedMaximumLength)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Function called when ValidateResponse returns true
        /// </summary>
        /// <param name="response"></param>
        protected override void OnSuccess(string response)
        {
            var activeWorkflowObject = _GuidedWorkStore.GetActiveWorkflowObject();

            if (IsInUserVocab(response))
            {
                activeWorkflowObject.ExtraData["Button"] = response;
            }
            else
            {
                _ViewModel.Response = response;
                activeWorkflowObject.Data = response;
            }

            activeWorkflowObject.Modified = true;
            _GuidedWorkStore.UpdateCompleted();
        }

        protected override Task OnFailureAsync(string response)
        {
            var viewModel = (OrderPickingEnterDigitsViewModel)ViewModel;
            if (!string.IsNullOrWhiteSpace(response))
            {
                viewModel.ErrorMessage = GetLocalizedText("Error_WrongQuantity");
            }
            return base.OnFailureAsync(response);
        }

        private string GetInitialPrompt()
        {
            var words = new List<string>();
            CommonDialogueUtils.SplitNumberStringIntoWords(_DataStore.OrderIdentifier, ref words);
            string spokenOrderIdentifier = string.Empty;
            foreach (var word in words) { spokenOrderIdentifier += word +" "; }
            return GetLocalizedText("InitialPrompt", _DataStore.RemainingQuantity.ToString(), _DataStore.CurrentPickingContainer.Identifier);
        }

        private void ResetUiToInitialState()
        {
            var viewModel = (OrderPickingEnterDigitsViewModel)ViewModel;
            viewModel.Response = string.Empty;
            viewModel.InitialPrompt = GetInitialPrompt();
            viewModel.ErrorMessage = string.Empty;
        }
    }
}
