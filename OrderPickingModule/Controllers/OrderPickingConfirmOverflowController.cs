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
    /// Controller for a WFA where the user confirms overflow for a container.
    /// </summary>
    public class OrderPickingConfirmOverflowController : BooleanConfirmationController
    {

        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        private OrderPickingDataStore _DataStore => OrderPickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        public OrderPickingConfirmOverflowController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) :
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
            var viewModel = (OrderPickingConfirmOverflowViewModel)base.CreateViewModel(viewModelName);

            viewModel.Container = _DataStore.CurrentPickingContainer.Identifier;
            viewModel.InitialPrompt = GetLocalizedText("InitialPrompt", _DataStore.CurrentPickingContainer.Identifier);
            viewModel.OrderIdentifier = _DataStore.OrderIdentifier;

            InfoGlobalWordPrompt = GetLocalizedText("Instructions");

            return viewModel;
        }

        /// <summary>
        /// Sets the extra data as affirmative.
        /// </summary>
        public override void Affirmative()
        {
            _GuidedWorkStore.UpdateActiveObjectExtraData("Button",
                ((BooleanConfirmationViewModel)ViewModel).AffirmativeWord);
        }

        /// <summary>
        /// Sets the extra data as negative.
        /// </summary>
        public override void Negative()
        {
            _GuidedWorkStore.UpdateActiveObjectExtraData("Button",
                ((BooleanConfirmationViewModel)ViewModel).NegativeWord);
        }

        private async void OnStoreUpdated()
        {
            await _GuidedWorkRunner.RespondAsync();
            await _GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(_GuidedWorkRunner.WorkflowEventName);
        }
    }
}
