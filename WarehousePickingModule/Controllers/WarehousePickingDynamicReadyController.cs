//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWorkRunner;

    /// <summary>
    /// A controller used to handle the process of the user indicating readiness
    /// for the next task.
    /// </summary>
    public class WarehousePickingDynamicReadyController : ReadyController
    {
        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        private WarehousePickingDataStore _DataStore => WarehousePickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        /// <summary>
        /// Initializes a new instance of the <see cref="T:WarehousePicking.WarehousePickingDynamicReadyController"/> class.
        /// </summary>
        /// <param name="dependencies">Dependencies.</param>
        /// <param name="guidedWorkRunner">Guided work runner.</param>
        /// <param name="guidedWorkStore">Guided work store.</param>
        public WarehousePickingDynamicReadyController(CoreViewControllerDependencies dependencies,
                                              IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore)
        : base(dependencies)
        {
            _GuidedWorkRunner = guidedWorkRunner;
            _GuidedWorkStore = guidedWorkStore;
        }

        /// <summary>
        /// Called by the workflow engine when hardware or software back button is pressed to determine if
        /// this IViewController is currently allowing back navigation. Defaults to true.
        /// </summary>
        /// <returns><c>true</c> if the controller should be able to navigate backwards on the stack
        /// </returns>
        public override bool ShouldAllowBackNavigation()
        {
            _GuidedWorkStore.UpdateActiveObjectExtraData("Button", "NavigateBack");
            return false;
        }

        /// <summary>
        /// Called when the WFA is created.
        /// </summary>
        /// <param name="reason">Reason.</param>
        protected override void OnStart(NavigationReason reason)
        {
            base.OnStart(reason);
            _GuidedWorkStore.StoreUpdated += OnStoreUpdated;
        }

        /// <summary>
        /// Called when the WFA is destroyed.
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();
            _GuidedWorkStore.StoreUpdated -= OnStoreUpdated;
        }

        /// <summary>
        /// Creates the <see cref="WarehousePickingDynamicReadyViewModel"/> and initializes its
        /// properties.
        /// </summary>
        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var vm = (WarehousePickingDynamicReadyViewModel)base.CreateViewModel(viewModelName);

            var qty =_DataStore.RemainingQuantity;
            
            vm.TripStandardTime = "15 min, 30 seconds";
            vm.TotalCases = _DataStore.RemainingCases;
            vm.Stats = GetLocalizedText("Stats", vm.TotalCases);
            vm.LabelPrinter = _DataStore.LabelPrinter;
            vm.InitialPrompt = GetLocalizedText("CheckDigit_Spoken") + ",,,," + GetLocalizedText("StandardTime_Spoken") + ",,,," + GetLocalizedText("InitialPrompt", vm.TotalCases);

            //InfoGlobalWordPrompt = $"{vm.Header}: {_StockRecordModel.ProductName}, {_StockRecordModel.LocationPrompt}";

            return vm;
        }

        /// <summary>
        /// The default command to execute when the user/view is ready to move on. Override
        /// this method to customize the behavior of ReadyController subclasses.
        /// </summary>
        public override void Ready()
        {
            _GuidedWorkStore.UpdateActiveObjectExtraData("Button", "Ready");
        }

        private async void OnStoreUpdated()
        {
            await _GuidedWorkRunner.RespondAsync();
            await _GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(_GuidedWorkRunner.WorkflowEventName);
        }
    }
}
