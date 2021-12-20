//////////////////////////////////////////////////////////////////////////////
//     Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GuidedWorkRunner;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;

    public class OrderPickingOrderStatusController : SingleResponseController
    {
        protected OrderPickingOrderStatusViewModel _ViewModel;

        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        private OrderPickingDataStore _DataStore => OrderPickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);


        /// <summary>
        /// Initializes a new instance of the <see cref="OrderPickingAcknowledgeLocationController"/> class.
        /// </summary>
        public OrderPickingOrderStatusController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) : base(dependencies)
        {
            _GuidedWorkRunner = guidedWorkRunner;
            _GuidedWorkStore = guidedWorkStore;
            PublishesAsOptionEvent = true;
        }

        public string InfoGlobalWordPrompt { get; set; }

        public override bool ShouldAllowBackNavigation()
        {
            _GuidedWorkStore.UpdateActiveObjectExtraData("Button", "NavigateBack");
            return true;
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
            _ViewModel = (OrderPickingOrderStatusViewModel)base.CreateViewModel(viewModelName);

            _ViewModel.Header = GetLocalizedText("Header", _DataStore.OrderIdentifier);
            _ViewModel.InitialPrompt = GetLocalizedText("InitialPrompt");

            //create an empty list of ListView cells
            var listItems = new ObservableCollection<OrderPickingOrderStatusListItemViewModel>();
            //get order picking work items that have not been completed or started (in progress) from the model
            List<OrderPickingSummaryItem> orderPickingSummaryItems = _DataStore.OrderPickingSummaryItems;

            //iterate through the work items and create the list item view models
            foreach(var opwi in orderPickingSummaryItems)
            {
                OrderPickingOrderStatusListItemViewModel newListItem = CreateListItemFromWorkItem(opwi);
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
            return _ViewModel;
        }

        /// <summary>
        /// Converts an OrderPickingWorkItem object to an OrderPickingOrderStatusListItemViewModel
        /// so that it can be used by our view model
        /// </summary>
        /// <param name="opsi"> The <see cref="OrderPickingWorkItem"/> object to convert to an <see cref="OrderPickingOrderStatusListItemViewModel"/> object</param>
        /// <returns>a new <see cref="OrderPickingOrderStatusListItemViewModel"/></returns>
        private OrderPickingOrderStatusListItemViewModel CreateListItemFromWorkItem(OrderPickingSummaryItem opsi)
        {
            return new OrderPickingOrderStatusListItemViewModel()
            {
                ProductImage = opsi.ProductImage,
                ProductName = opsi.ProductName,
                RequestedQuantity = opsi.Quantity
            };
        }

        protected override void OnSuccess(string response)
        {
            var viewModel = (OrderPickingOrderStatusViewModel)ViewModel;


            if (viewModel.PossibleResponses.Contains(response))
            {
                _GuidedWorkStore.UpdateActiveObjectExtraData("Button", response);
                return;
            }

            string msg = GetLocalizedText("Error_UnexpectedResponse", response);
            throw new ArgumentException(msg);
        }

        private async void OnStoreUpdated()
        {
            await _GuidedWorkRunner.RespondAsync();
            await _GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(_GuidedWorkRunner.WorkflowEventName);
        }
    }
}
