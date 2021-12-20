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
    /// Controller for a WFA that waits for a worker to be ready
    /// </summary>
    public class OrderPickingReadyController : ReadyController
    {
        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        private OrderPickingDataStore _DataStore => OrderPickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);


        public OrderPickingReadyController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore)
            : base(dependencies)
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
            var viewModel = base.CreateViewModel(viewModelName);

            InfoGlobalWordPrompt = GetLocalizedText("Instructions");

            return viewModel;
        }

        public override void Ready()
        {
            var viewModel = (ReadyViewModel)ViewModel;

            _GuidedWorkStore.UpdateActiveObjectExtraData("Button", GetLocalizedText("next_entry_word"));
        }

        private async void OnStoreUpdated()
        {
            await _GuidedWorkRunner.RespondAsync();
            await _GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(_GuidedWorkRunner.WorkflowEventName);
        }
    }
}
