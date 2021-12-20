//////////////////////////////////////////////////////////////////////////////
//     Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Honeywell.Firebird;
    using Honeywell.Firebird.WorkflowEngine;
    using Honeywell.Firebird.CoreLibrary;
    using GuidedWorkRunner;

    public class ReceivingItemsController : DigitEntryController
    {
        protected ReceivingItemsViewModel _ViewModel;
        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        private ReceivingDataStore _DataStore => ReceivingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        public ReceivingItemsController(
            CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner,
            IGuidedWorkStore guidedWorkStore) :
            base(dependencies)
        {
            _GuidedWorkRunner = guidedWorkRunner;
            _GuidedWorkStore = guidedWorkStore;
        }

        public string InfoGlobalWordPrompt { get; set; }

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

        public string LocationGlobalWordPrompt
        {
            get
            {
                string prompt = _DataStore.ProductLocationText;  //need to hook this in LAP
                if (string.IsNullOrEmpty(prompt))
                {
                    return GetLocalizedText("NoItemReceived");
                }

                return GetLocalizedText("LocationPrompt", prompt);

            }
        }

        public override bool ShouldAllowBackNavigation()
        {
            _GuidedWorkStore.UpdateActiveObjectExtraData("Button", "NavigateBack");
            return false;
        }

        protected override void OnStart(NavigationReason reason)
        {
            //_DataStore.QuantityLastReceived = 0;
            base.OnStart(reason);
            _GuidedWorkStore.StoreUpdated += OnStoreUpdated;
        }

        protected override void OnStop()
        {
            //_DataStore.QuantityLastReceived = 0;
            base.OnStop();
            _GuidedWorkStore.StoreUpdated -= OnStoreUpdated;
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            _ViewModel = (ReceivingItemsViewModel)base.CreateViewModel(viewModelName);

            _ViewModel.Header = GetLocalizedText("Header", "Purchase Order");
            _ViewModel.InitialPrompt = GetLocalizedText("InitialPrompt");

            //create an empty list of ListView cells
            var listItems = new ObservableCollection<ReceivingItemsListViewModel>();
            //get receiving work items that have not been completed or started (in progress) from the model
            List<ReceivingSummaryItem> ReceivingSummaryItems = _DataStore.ReceivingSummaryItems;

            //iterate through the work items and create the list item view models
            foreach(var rsi in ReceivingSummaryItems)
            {
                ReceivingItemsListViewModel newListItem = CreateListItemFromWorkItem(rsi);
                listItems.Add(newListItem);
            }

            //as long as our list as at least 1 item - assign the first index
            //as the CurrentItem so that it is "selected" in the view
            if(listItems.Any())
            {
                _ViewModel.CurrentWorkItem = listItems[0];
            }

            //bind the info prompt based on how many items we have
            InfoGlobalWordPrompt = listItems.Count == 1 ? 
                GetLocalizedText("InfoPromptSingular") : GetLocalizedText("InfoPromptPlural", listItems.Count.ToString());

            //bind our list items to the view model
            _ViewModel.CurrentAndUpcomingPicks = listItems;

            _ViewModel.OverflowMenuItems = OverflowMenuItems;
            foreach (var overflowMenuItem in OverflowMenuItems)
            {
                _ViewModel.UserVocab.Add(overflowMenuItem);
            }

            return _ViewModel;
        }

        /// <summary>
        /// Validates the user's spoken or entered response
        /// </summary>
        /// <param name="response">the spoken or keyed in response</param>
        /// <returns>True if the user's response is valid, false if the user's response is invalid</returns>
        protected override bool ValidateResponse(string response)
        {
            //make sure we have a valid response string
            if (string.IsNullOrEmpty(response))
            {
                return false;
            }

            //check if the response is in our vocab
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

            //response is less than the minimum length
            if (response.Length < _ViewModel.MinWholeNumberDigits)
            {
                //set the error message and the last invalid barcode on the model
                DisplayErrorMessage(GetLocalizedText("Error_InvalidEntry", _ViewModel.MinWholeNumberDigits.ToString()));
                //_DataStore.LastInvalidBarcode = response;
                //set what is spoken to the user to be nothing (view will show error message)
                _ViewModel.ValidationModel.DefaultInvalidResponseMessage = string.Empty;
                return false;
            }

            //response isn't a vocab word and is appropriate length - now try to match an item
            var possibleReceivingWorkItems = _DataStore.RetrieveMatchingOrders(response);

            if (possibleReceivingWorkItems.Count == 0)
            {
                string errorMessage = GetLocalizedText("Error_NotFound");
                DisplayErrorMessage(errorMessage);
                //_DataStore.LastInvalidBarcode = response;
                //set what is spoken to the user based on the length of the response
                _ViewModel.ValidationModel.DefaultInvalidResponseMessage =
                    response.Length == _ViewModel.ExpectedMaximumLength ? string.Empty : errorMessage;
                return false;
            }

            if (possibleReceivingWorkItems.Count == 1)
            {
                //there was one  
                ClearAndHideErrorMessage();
                //_DataStore.LastInvalidBarcode = string.Empty;
                //_DataStore.LastValidWorkItem = possibleReceivingWorkItems[0];
                return true;
            }
            else
            {
                // The expected product was in the matching list, but
                // the user must scan it as it is not unique.
                string errorMessage = GetLocalizedText("Error_NotUnique");
                DisplayErrorMessage(errorMessage);
                //_DataStore.LastInvalidBarcode = response;
                _ViewModel.ValidationModel.DefaultInvalidResponseMessage = errorMessage;
            }

            return false;
        }

        /// <summary>
        /// Called when ValidateResponse() returns true
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
        /// Called when ValidatedResponse() returns false
        /// </summary>
        /// <param name="response"></param>
        protected override Task OnFailureAsync(string response)
        {
            //clear the response since we are staying on this screen due to invalid user response
            _ViewModel.Response = string.Empty;
            return base.OnFailureAsync(response);
        }

        private void DisplayErrorMessage(string message)
        {
            _ViewModel.ErrorMessage = message;
        }

        private void ClearAndHideErrorMessage()
        {
            _ViewModel.ErrorMessage = string.Empty;
        }

        /// <summary>
        /// Converts an ReceivingSummaryItem object to an ReceivingListItemViewModel
        /// so that it can be used by our view model
        /// </summary>
        /// <param name="rsi"> The <see cref="ReceivingSummaryItem"/> object to convert to an <see cref="ReceivingItemsListViewModel"/> object</param>
        /// <returns>a new <see cref="ReceivingItemsListViewModel"/></returns>
        private ReceivingItemsListViewModel CreateListItemFromWorkItem(ReceivingSummaryItem rsi)
        {
            return new ReceivingItemsListViewModel()
            {
                ProductImage = rsi.ProductImage,
                ProductName = rsi.ProductName,
                ProductIdentifier = rsi.ProductIdentifier,
                RemainingQuantity = rsi.RemainingQuantity,
                IsComplete = rsi.IsComplete,
                IsDamaged = rsi.IsDamaged
             };
        }

        private async void OnStoreUpdated()
        {
            await _GuidedWorkRunner.RespondAsync();
            await _GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(_GuidedWorkRunner.WorkflowEventName);
        }
    }
}
