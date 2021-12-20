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

    public class WarehousePickingPerformanceController : SingleResponseController
    {
        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        private WarehousePickingDataStore _DataStore => WarehousePickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        protected IWarehousePickingActivityTracker _ActivityTracker;
        protected WarehousePickingPerformanceViewModel _ViewModel;

        public WarehousePickingPerformanceController(
            CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore, IWarehousePickingActivityTracker warehousePickingActivityTracker) : 
            base(dependencies)
        {
            _GuidedWorkRunner = guidedWorkRunner;
            _GuidedWorkStore = guidedWorkStore;
            _ActivityTracker = warehousePickingActivityTracker;
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
            _ViewModel = (WarehousePickingPerformanceViewModel)base.CreateViewModel(viewModelName);

            _ViewModel.Header = GetLocalizedText("Header");
            _ViewModel.InitialPrompt = GetLocalizedText("InitialPrompt");

            //create an empty list of ListView cells
            var listItems = new ObservableCollection<WarehousePickingPerformanceListItemViewModel>();
            //get warehouse picking work items that have not been completed or started (in progress) from the model
            List<WarehousePickingWorkItem> WarehousePickingWorkItems = _DataStore.WarehousePickingWorkItems;

            listItems.Add(new WarehousePickingPerformanceListItemViewModel { ActivityName = "Total Trip Time", ActivityDuration = _ActivityTracker.GetTripTime().ToString(@"hh\:mm\:ss") });

            foreach (var aisle in _ActivityTracker.GetAisleTimes())
            {
                listItems.Add(new WarehousePickingPerformanceListItemViewModel { ActivityName = $"Aisle {aisle.Name}", ActivityDuration = $"{aisle.Duration.ToString(@"mm\:ss")}" });

                foreach (var slot in _ActivityTracker.GetSlotTimes())
                {
                    if (slot.Tag == aisle.Name)
                    {
                        listItems.Add(new WarehousePickingPerformanceListItemViewModel { ActivityName = $"     Slot {slot.Name}", ActivityDuration = $"{slot.Duration.ToString(@"mm\:ss")}" });
                    }
                }
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

        private async void OnStoreUpdated()
        {
            await _GuidedWorkRunner.RespondAsync();
            await _GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(_GuidedWorkRunner.WorkflowEventName);
        }
    }
}
