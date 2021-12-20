//////////////////////////////////////////////////////////////////////////////
//     Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Threading.Tasks;
    using Common.Logging;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWorkRunner;

    /// <summary>
    /// Controller for a WFA where the user enters the Identifier for the product to pick.
    /// </summary>
    public class WarehousePickingEnterLabelPrinterController : DigitEntryController
    {
        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        private WarehousePickingDataStore _DataStore => WarehousePickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        private ILog _Log = LogManager.GetLogger(nameof(WarehousePickingEnterLabelPrinterController));
        protected DigitEntryViewModel _ViewModel;
        protected bool _CommandExecuted;

        public WarehousePickingEnterLabelPrinterController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) : 
        base(dependencies)
        {
            _GuidedWorkRunner = guidedWorkRunner;
            _GuidedWorkStore = guidedWorkStore;
        }

        public string InfoGlobalWordPrompt { get; set; }

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

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            _ViewModel = (DigitEntryViewModel)base.CreateViewModel(viewModelName);

            var dataStore = _DataStore;

            //InfoGlobalWordPrompt = GetLocalizedText("InfoPrompt", location);

            _ViewModel.AcceptsVariableLengthResult = false;
            _ViewModel.ExpectedMaximumLength = 3;
            _ViewModel.MaxDecimalDigits = 0;
            _ViewModel.MinDecimalDigits = 0;
            _ViewModel.MinWholeNumberDigits = 3;
           // _ViewModel.ResponseExpressions = Model.GetResponseExpressions((int)_ViewModel.ExpectedMaximumLength);

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

            //// No matches found
            //if (!Model.ValidProductNumber(response))
            //{
            //    _ViewModel.ErrorMessage = GetLocalizedText("Error_WrongProduct") + ": " + response;
            //    // Don't speak the keyed in digits if the response is greater than _ViewModel.MinWholeNumberDigits
            //    if (response.Length > _ViewModel.MinWholeNumberDigits)
            //    {
            //        _ViewModel.ValidationModel.DefaultInvalidResponseMessage = GetLocalizedText("Error_WrongProductPrompt");
            //    }
            //    else
            //    {
            //        _ViewModel.ValidationModel.DefaultInvalidResponseMessage = string.Empty;
            //    }
            //    return false;
            //}

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
            _ViewModel.Response = response;
            _GuidedWorkStore.UpdateActiveObjectExtraData("Button", response);
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


        protected override void OnResume(NavigationReason reason)
        {
            _CommandExecuted = false;
            base.OnResume(reason);
        }

        private async void OnStoreUpdated()
        {
            await _GuidedWorkRunner.RespondAsync();
            await _GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(_GuidedWorkRunner.WorkflowEventName);
        }
    }
}
