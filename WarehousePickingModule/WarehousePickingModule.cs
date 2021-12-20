//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Reflection;
    using System.Threading.Tasks;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Honeywell.Firebird.Module;

    public class WarehousePickingModule : GuidedWorkModule
    {
        private const string WarehousePickingEventName = "StartWarehousePickingWorkflow";
        private const string WarehousePickingWorkflowName = "WarehousePicking";

        public WarehousePickingModule(IModuleContext context)
            : base(context)
        {
        }

        #region IModule implementation

        /// <summary>
        /// Register the services.
        /// </summary>
        public override void RegisterServices()
        {
            base.RegisterServices();

            var workflowResourceRegistry = Context.Container.Resolve<IWorkflowResourceRegistry>();
            workflowResourceRegistry.AddSearchAssembly(GetAssembly());

            Context.Container.Register<IWarehousePickingRESTServiceProvider, WarehousePickingRESTServiceProvider>();
            Context.Container.RegisterMultiple<IWarehousePickingDataTransport>(new[] { typeof(WarehousePickingFileDataTransport), typeof(WarehousePickingRESTDataTransport) });
            Context.Container.Register<IWarehousePickingDataProxy, WarehousePickingDataProxy>().AsSingleton();

            Context.Container.Register<IWarehousePickingDataService, WarehousePickingDataService>().AsSingleton();
            Context.Container.Register<IWarehousePickingActivityTracker, WarehousePickingActivityTracker>().AsSingleton();

            Context.Container.Register<IModuleVocab, WarehousePickingModuleVocab>("WarehousePicking");

            RegisterWorkflowController<WarehousePickingGuidedWorkController>();

            RegisterWorkflowViewController<WarehousePickingSubcenterListController>();
            RegisterWorkflowViewController<WarehousePickingEnterLabelPrinterController>();
            RegisterWorkflowViewController<WarehousePickingAcknowledgeLocationController>();
            RegisterWorkflowViewController<WarehousePickingConfirmQuantityController>();
            RegisterWorkflowViewController<WarehousePickingEnterProductController>();
            RegisterWorkflowViewController<WarehousePickingEnterQuantityController>();
            RegisterWorkflowViewController<WarehousePickingBooleanConfirmationController>();
            RegisterWorkflowViewController<WarehousePickingOrderStatusController>();
            RegisterWorkflowViewController<WarehousePickingOrderSummaryController>();
            RegisterWorkflowViewController<WarehousePickingDynamicReadyController>();
            RegisterWorkflowViewController<WarehousePickingLastPickController>();
            RegisterWorkflowViewController<WarehousePickingPerformanceController>();

            RegisterWorkflowViewModel<WarehousePickingAcknowledgeLocationViewModel>();
            RegisterWorkflowViewModel<WarehousePickingEnterDigitsViewModel>();
            RegisterWorkflowViewModel<WarehousePickingBooleanConfirmationViewModel>();
            RegisterWorkflowViewModel<WarehousePickingOrderStatusViewModel>();
            RegisterWorkflowViewModel<WarehousePickingOrderSummaryViewModel>();
            RegisterWorkflowViewModel<WarehousePickingDynamicReadyViewModel>();
            RegisterWorkflowViewModel<WarehousePickingLastPickViewModel>();
            RegisterWorkflowViewModel<WarehousePickingPerformanceViewModel>();

            RegisterView<WarehousePickingSubcenterSelectView>();
            RegisterView<WarehousePickingEnterLabelPrinterView>();
            RegisterView<WarehousePickingDynamicReadyView>();
            RegisterView<WarehousePickingLastPickView>();
            RegisterView<WarehousePickingView>();
            RegisterView<WarehousePickingAcknowledgeLocationView>();
            RegisterView<WarehousePickingEnterProductView>();
            RegisterView<WarehousePickingEnterQuantityView>();
            RegisterView<WarehousePickingOrderStatusView>();
            RegisterView<WarehousePickingOrderSummaryView>();
            RegisterView<WarehousePickingPerformanceView>();

            RegisterView<WarehousePickingSingleResponseDialogueView>();

            RegisterWorkflowModelSingleton<IWarehousePickingModel, WarehousePickingModel>();
            Context.Container.Register<IWarehousePickingIntentBuilder, WarehousePickingIntentBuilder>();
            Context.Container.Register<IWarehousePickingMobileDataExchange, WarehousePickingMobileDataExchange>().AsSingleton();

            Translate.LocalizationResources.AddResource(WarehousePickingResources.ResourceManager);
        }

        public override Task RunAsync()
        {
            var taskManagerModel = Context.Container.Resolve<ITaskManagerModel>();
            taskManagerModel.WorkflowNameToStartWorkflowEventName.Add(WarehousePickingWorkflowName, WarehousePickingEventName);

            return base.RunAsync();
        }

        #endregion

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<WarehousePickingModule>(GuidedWorkRunnerModule.ModuleInfo);

        /// <summary>
        /// Return the assembly that contains the WarehouseModule, so
        /// that the GuidedWorkModule can access the resources contained
        /// herein (e.g. wfa, configuration, and data json files).
        /// </summary>
        private Assembly GetAssembly()
        {
            return typeof(WarehousePicking.WarehousePickingModule).GetTypeInfo().Assembly;
        }
    }
}
