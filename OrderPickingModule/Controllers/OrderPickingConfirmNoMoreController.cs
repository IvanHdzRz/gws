//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using GuidedWorkRunner;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;

    /// <summary>
    /// Controller for a WFA that confirms ending an order
    /// </summary>
    public class OrderPickingConfirmNoMoreController : BooleanConfirmationController
    {
        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        protected OrderPickingDataStore _DataStore => OrderPickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        /// <summary>
        /// The names of the events that this workflow publishes.
        /// </summary>
        private const string EndWorkflowEventName = "EndWorkflow";
        private const string StagingEventName = "Staging";

        public OrderPickingConfirmNoMoreController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore)
            : base(dependencies)
        {
            _GuidedWorkRunner = guidedWorkRunner;
            _GuidedWorkStore = guidedWorkStore;
        }

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
