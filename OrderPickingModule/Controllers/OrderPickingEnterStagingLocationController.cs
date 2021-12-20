//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using GuidedWorkRunner;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;

    /// <summary>
    /// Controller for a WFA where the user enters the staging location.
    /// </summary>
    public class OrderPickingEnterStagingLocationController : DigitEntryController
    {
        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        private OrderPickingEnterStagingLocationViewModel _ViewModel;

        private OrderPickingDataStore _DataStore => OrderPickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        public OrderPickingEnterStagingLocationController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) :
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
            _ViewModel = (OrderPickingEnterStagingLocationViewModel)base.CreateViewModel(viewModelName);
            
            var container = _DataStore.CurrentStagingContainer;
            _ViewModel.AcceptsVariableLengthResult = true;
            _ViewModel.Container = container.Identifier;
            _ViewModel.ExpectedMaximumLength = _DataStore.StagingMaxSpokenLength;
            _ViewModel.InitialPrompt = GetLocalizedText("InitialPrompt", container.Identifier);
            _ViewModel.OrderIdentifier = container.OrderIdentifier;
            
            InfoGlobalWordPrompt = GetLocalizedText("Instructions");

            return _ViewModel;
        }

        protected override bool ValidateResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
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

        private async void OnStoreUpdated()
        {
            await _GuidedWorkRunner.RespondAsync();
            await _GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(_GuidedWorkRunner.WorkflowEventName);
        }
    }
}
