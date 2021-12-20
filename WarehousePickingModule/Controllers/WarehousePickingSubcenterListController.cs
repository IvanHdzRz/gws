//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2015 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Collections.Generic;
    using Honeywell.Firebird;
    using Honeywell.Firebird.WorkflowEngine;
    using Honeywell.Firebird.CoreLibrary;
    using GuidedWorkRunner;

    /// <summary>
    /// The workflow controller for the workflow selection activity that sends a message
    /// to GuidedWork to unassign all auto assigned assigments.  This will happen whenever
    /// this activity is presented or when popping back prior to this activity.
    /// </summary>
    public class WarehousePickingSubcenterListController : PublishingSelectionController<SelectionViewModel>
    {
        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        private WarehousePickingDataStore _DataStore => WarehousePickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        private bool _FeedbackComplete;
        private string _Selection;

        public WarehousePickingSubcenterListController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore)
            : base(dependencies)
        {
            _GuidedWorkRunner = guidedWorkRunner;
            _GuidedWorkStore = guidedWorkStore;
        }

        private IReadOnlyList<string> _SelectionOptions;
        public override IList<string> GetSelectionOptions() => _DataStore.SelectionOptions;

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

        /// <summary>
        /// Creates the view model used by the WorkflowListController.
        /// </summary>
        /// <param name="viewModelName">Name of the view model to create</param>
        /// <returns>An instance of SelectionViewModel</returns>
        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var viewModel = (SelectionViewModel)base.CreateViewModel(viewModelName);

            _SelectionOptions = new List<string> { "Dry", "Freezer", "Produce" };

            var selectionEventMap = new Dictionary<string, SelectionEvent>();
            foreach (var option in _SelectionOptions)
            {
                string key = GetLocalizedText(option);
                selectionEventMap.Add(key, new SelectionEvent() { Name = SelectedOptionEventName, Option = option });
            }

            SelectionEventMap = selectionEventMap;
            viewModel.SelectionNames = SelectionEventMap.Keys;

            return viewModel;
        }

        public override void SelectionCompleted()
        {
            if (!_FeedbackComplete)
            {
                PublishSelected(_Selection);
            }
        }

        protected override void OnItemSelected(string selection)
        {
            // Only process the selection if it is not "blank"
            if (!string.IsNullOrWhiteSpace(selection))
            {
                _Selection = selection;
                if (!ProvideSelectionFeedback)
                {
                    SelectionCompleted();
                }
            }
        }

        protected override void PublishSelected(string selection)
        {
            _FeedbackComplete = true;

            _GuidedWorkStore.UpdateActiveObjectExtraData("Button", selection);
        }

        private async void OnStoreUpdated()
        {
            await _GuidedWorkRunner.RespondAsync();
            await _GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(_GuidedWorkRunner.WorkflowEventName);
        }
    }
}
