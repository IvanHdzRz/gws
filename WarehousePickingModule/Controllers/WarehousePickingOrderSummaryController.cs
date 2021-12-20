//////////////////////////////////////////////////////////////////////////////
//     Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWorkRunner;

    public class WarehousePickingOrderSummaryController : SingleResponseController
    {
        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        private WarehousePickingDataStore _DataStore => WarehousePickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);
        protected WarehousePickingOrderSummaryViewModel _ViewModel;

        public WarehousePickingOrderSummaryController(
            CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) : 
            base(dependencies)
        {
            _GuidedWorkRunner = guidedWorkRunner;
            _GuidedWorkStore = guidedWorkStore;
        }

        public string InfoGlobalWordPrompt { get; set; }

        public override bool ShouldAllowBackNavigation()
        {
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
            _ViewModel = (WarehousePickingOrderSummaryViewModel)base.CreateViewModel(viewModelName);

            _ViewModel.Header = GetLocalizedText("Header");
            _ViewModel.InitialPrompt = GetLocalizedText("InitialPrompt");

            //create an empty list of ListView cells
            var listItems = new ObservableCollection<WarehousePickingOrderSummaryListItemViewModel>();
            //get warehouse picking work items that have not been completed or started (in progress) from the model
            List<WarehousePickingSummaryItem> WarehousePickingSummaryItems = _DataStore.WarehousePickingSummaryItems;

            //iterate through the work items and create the list item view models
            foreach(var opwi in WarehousePickingSummaryItems)
            {
                WarehousePickingOrderSummaryListItemViewModel newListItem = CreateListItemFromWorkItem(opwi);
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
        /// Converts an WarehousePickingSummaryItem object to an WarehousePickingOrderSummaryListItemViewModel
        /// so that it can be used by our view model
        /// </summary>
        /// <param name="wpsi"> The <see cref="WarehousePickingSummaryItem"/> object to convert to an <see cref="WarehousePickingOrderSummaryListItemViewModel"/> object</param>
        /// <returns>a new <see cref="WarehousePickingOrderSummaryListItemViewModel"/></returns>
        private WarehousePickingOrderSummaryListItemViewModel CreateListItemFromWorkItem(WarehousePickingSummaryItem wpsi)
        {
            return new WarehousePickingOrderSummaryListItemViewModel()
            {
                ProductImage = wpsi.ProductImage,
                ProductName = wpsi.ProductName,
                PickQuantity = wpsi.PickQuantity,
                Aisle = wpsi.Aisle,
                SlotID = wpsi.SlotID,
                ShortedQuantity = wpsi.ShortedQuantity,
                IsComplete = wpsi.IsComplete
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
