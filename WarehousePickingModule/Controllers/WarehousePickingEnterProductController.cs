//////////////////////////////////////////////////////////////////////////////
//     Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Logging;
    using Honeywell.DialogueRunner;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWork;
    using GuidedWorkRunner;

    /// <summary>
    /// Controller for a WFA where the user enters the Identifier for the product to pick.
    /// </summary>
    public class WarehousePickingEnterProductController : WarehousePickingEnterValueController
    {
        private ILog _Log = LogManager.GetLogger(nameof(WarehousePickingEnterProductController));

        protected WarehousePickingEnterDigitsViewModel _ViewModel;

        public WarehousePickingEnterProductController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) : 
        base(dependencies, guidedWorkRunner, guidedWorkStore)
        {
        }

        public string InfoGlobalWordPrompt { get; set; }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var dataStore = DataStore;

            _ViewModel = (WarehousePickingEnterDigitsViewModel)base.CreateViewModel(viewModelName);
            string location = LocationParser.ParseServerFormatLocation(dataStore.DetailedLocationText, ", ");

            _ViewModel.InitialPrompt = GetInitialPrompt(dataStore.SlotID);
            _ViewModel.SkipProductVisible = true;
            _ViewModel.QuantityEntryVisible = false;
            _ViewModel.OrderedQuantityVisible = false;
            _ViewModel.SkipProductVocabWord = GetLocalizedText("VocabWord_SkipProduct");
            InfoGlobalWordPrompt = GetLocalizedText("InfoPrompt", location);

            _ViewModel.AcceptsVariableLengthResult = false;
            _ViewModel.ExpectedMaximumLength = 3;
            _ViewModel.MaxDecimalDigits = 0;
            _ViewModel.MinDecimalDigits = 0;
            _ViewModel.MinWholeNumberDigits = 3;
            _ViewModel.ResponseExpressions = dataStore.GetResponseExpressions((int)_ViewModel.ExpectedMaximumLength);

            //_ViewModel.StockCodeEntryPlaceholder = GetLocalizedText("WarehousePicking_QuantityEntryPlaceholder");
            _ViewModel.StockCodeEntryPlaceholder = dataStore.CheckDigit;

            _ViewModel.OverflowMenuItems = OverflowMenuItems;
            foreach (var overflowMenuItem in OverflowMenuItems)
            {
                _ViewModel.UserVocab.Add(overflowMenuItem);
            }

            return _ViewModel;
        }

        /// <summary>
        /// Since we are adding our leaf WFA's into the BaldEagleWorkflowActivity composite,
        /// we cannot use the normal global word / overflow mechanism. So we have added a new
        /// OverflowMenuItems WFA param that sets these (which is nice now that we add them to
        /// the WFA that needs them, and don't have to mark them as ignored everywhere else).
        /// The controller now decides what to do with the menu item, whether it is spoken or
        /// selected in overflow menu.In most cases we send the menu item as a "button" in the
        /// ExtraData of the slot.In the case of "info" we had to treat it as an invalid entry,
        /// so that we could use the ValidatingWorkflowViewController's SignalInvalidResponseAsync
        /// command to cause the InfoGlobalWordPrompt to be spoken as the error message. This
        /// allows us to stay on the same WFA (screen) and restart the dialogue.
        /// </summary>
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
                if (GetLocalizedText("VocabWord_Info") == response)
                {
                    _ViewModel.ErrorMessage = string.Empty;
                    _ViewModel.ValidationModel.DefaultInvalidResponseMessage = InfoGlobalWordPrompt;
                    return false;
                }

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
            if (!DataStore.ValidCheckDigit(response))
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
            var activeWorkflowObject = GuidedWorkStore.GetActiveWorkflowObject();

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
            GuidedWorkStore.UpdateCompleted();
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

        private IReadOnlyList<string> _OverflowMenuItems;

        private IReadOnlyList<string> OverflowMenuItems
        {
            get
            {
                if (_OverflowMenuItems == null)
                {
                    _OverflowMenuItems = GetWorkflowParamAs<List<string>>("OverflowMenuItems");
                    if (null == _OverflowMenuItems)
                    {
                        _OverflowMenuItems = new List<string>();
                    }
                    else
                    {
                        var translatedOverflowMenuItems = new List<string>();
                        foreach (var overflowMenuItem in _OverflowMenuItems)
                        {
                            string translatedOverflowMenuItem = GetLocalizedText(overflowMenuItem);
                            translatedOverflowMenuItems.Add(translatedOverflowMenuItem);
                        }
                        _OverflowMenuItems = translatedOverflowMenuItems;
                    }
                }

                return _OverflowMenuItems;
            }
        }

        private string GetInitialPrompt(string aisle)
        {
            var words = new List<string>();
            CommonDialogueUtils.SplitNumberStringIntoWords(aisle, ref words);

            string locationString = string.Empty;
            foreach (var word in words)
            {
                locationString += " " + word;
            }

            return GetLocalizedText("InitialPrompt", locationString);
        }
    }
}
