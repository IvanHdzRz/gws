//////////////////////////////////////////////////////////////////////////////
//     Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Honeywell.Firebird;
    using Honeywell.Firebird.WorkflowEngine;
    using Honeywell.Firebird.CoreLibrary;
    using GuidedWorkRunner;

    public class ReceivingSummaryController : SingleResponseController
    {
        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        private ReceivingDataStore _DataStore => ReceivingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        protected ReceivingSummaryViewModel _ViewModel;

        public ReceivingSummaryController(
            CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) : 
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
            _ViewModel = (ReceivingSummaryViewModel)base.CreateViewModel(viewModelName);

            _ViewModel.Header = GetLocalizedText("Header", "Purchase Order");
            _ViewModel.InitialPrompt = GetLocalizedText("InitialPrompt");

            //create an empty list of ListView cells
            var listItems = new ObservableCollection<ReceivingSummaryListViewModel>();
            //get receiving work items that have not been completed or started (in progress) from the model
            List<ReceivingSummaryItem> ReceivingSummaryItems = _DataStore.ReceivingSummaryItems;

            //iterate through the work items and create the list item view models
            foreach (var rsi in ReceivingSummaryItems)
            {
                ReceivingSummaryListViewModel newListItem = CreateListItemFromWorkItem(rsi);
                listItems.Add(newListItem);
            }

            //as long as our list as at least 1 item - assign the first index
            //as the CurrentItem so that it is "selected" in the view
            if (listItems.Any())
            {
                _ViewModel.CurrentWorkItem = listItems[0];
            }

            //bind the info prompt based on how many items we have
            InfoGlobalWordPrompt = listItems.Count == 1 ? 
                GetLocalizedText("InfoPromptSingular") : GetLocalizedText("InfoPromptPlural", listItems.Count.ToString());

            //bind our list items to the view model
            _ViewModel.CurrentAndUpcomingPicks = listItems;
            return _ViewModel;
        }

        protected override void OnSuccess(string response)
        {
            _GuidedWorkStore.UpdateActiveObjectExtraData("Button", response);
        }

        /// <summary>
        /// Converts an ReceivingSummaryItem object to an ReceivingItemsListViewModel
        /// so that it can be used by our view model
        /// </summary>
        /// <param name="rsi"> The <see cref="ReceivingSummaryItem"/> object to convert to an <see cref="ReceivingSummaryListViewModel"/> object</param>
        /// <returns>a new <see cref="ReceivingSummaryListViewModel"/></returns>
        private ReceivingSummaryListViewModel CreateListItemFromWorkItem(ReceivingSummaryItem rsi)
        {
            return new ReceivingSummaryListViewModel()
            {
                ProductImage = rsi.ProductImage,
                ProductName = rsi.ProductName,
                ProductIdentifier = rsi.ProductIdentifier,
                RequestedQuantity = rsi.RequestedQuantity,
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
