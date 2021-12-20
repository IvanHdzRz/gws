//////////////////////////////////////////////////////////////////////////////
//     Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Threading.Tasks;
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWork;
    using GuidedWorkRunner;

    /// <summary>
    /// Controller for a WFA where the user enters the Identifier for the product to pick.
    /// </summary>
    public class OrderPickingEnterProductController : OrderPickingEnterValueController
    {
        private ILog _Log = LogManager.GetLogger(nameof(OrderPickingEnterProductController));
        protected OrderPickingEnterDigitsViewModel _ViewModel;

        public OrderPickingEnterProductController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) : 
        base(dependencies, guidedWorkRunner, guidedWorkStore)
        {
        }

        public string InfoGlobalWordPrompt { get; set; }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            _ViewModel = (OrderPickingEnterDigitsViewModel)base.CreateViewModel(viewModelName);
            string location = LocationParser.ParseServerFormatLocation(_DataStore.LocationText, ", ");

            _ViewModel.InitialPrompt = _DataStore.ProductName;
            _ViewModel.LastDigitsLabel = GetLocalizedText("LastDigits", _DataStore.ExpectedStockCodeResponseLength.ToString());
            _ViewModel.SkipProductVisible = true;
            _ViewModel.SkipProductVocabWord = GetLocalizedText("VocabWord_SkipProduct");
            InfoGlobalWordPrompt = GetLocalizedText("InfoPrompt", location);

            _ViewModel.AcceptsVariableLengthResult = false;
            _ViewModel.ExpectedMaximumLength = _DataStore.ExpectedStockCodeResponseLength;
            _ViewModel.MaxDecimalDigits = 0;
            _ViewModel.MinDecimalDigits = 0;
            _ViewModel.MinWholeNumberDigits = _DataStore.ExpectedStockCodeResponseLength;
            _ViewModel.ResponseExpressions = _DataStore.GetResponseExpressions((int)_DataStore.ExpectedStockCodeResponseLength);

            return _ViewModel;
        }

        /// <summary>
        /// Validates the digit entry response (voice or keyed)
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        protected override bool ValidateResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                return false;
            }

            _Log.Debug($"{nameof(ValidateResponse)}: {response}");

            // Check if the response is a valid vocab word
            if (IsInUserVocab(response))
            {
                return true;
            }

            // Response is less than the minimum length - show an informative minimum length error message
            if (response.Length < _ViewModel.MinWholeNumberDigits)
            {
                _ViewModel.ErrorMessage = GetLocalizedText("Error_InvalidEntry", _ViewModel.MinWholeNumberDigits.ToString());
                _ViewModel.ValidationModel.DefaultInvalidResponseMessage = string.Empty;
                return false;
            }

            // No matches found
            if (!_DataStore.IsValidIdentifier(response))
            {
                _ViewModel.ErrorMessage = GetLocalizedText("Error_WrongProduct") + ": " + response;
                // Don't speak the keyed in digits if the response is greater than _ViewModel.MinWholeNumberDigits
                if (response.Length > _ViewModel.MinWholeNumberDigits)
                {
                    _ViewModel.ValidationModel.DefaultInvalidResponseMessage = GetLocalizedText("Error_WrongProductPrompt");
                }
                else
                {
                    _ViewModel.ValidationModel.DefaultInvalidResponseMessage = string.Empty;
                }
                return false;
            }

            // One unique match found
            _ViewModel.ErrorMessage = string.Empty;
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

        /// <summary>
        /// Function called when ValidateResponse returns false
        /// </summary>
        /// <param name="response"></param>
        protected override Task OnFailureAsync(string response)
        {
            //clear out the digit entry response field, since we are staying on this view due to validation failure
            _ViewModel.Response = string.Empty;
            return base.OnFailureAsync(response);
        }
    }
}
