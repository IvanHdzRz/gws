//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Reflection;
    using System.Threading.Tasks;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Honeywell.Firebird.Module;
    using Retail;

    public class OrderPickingModule : GuidedWorkModule
    {
        private const string OrderPickingEventName = "StartOrderPickingWorkflow";
        private const string OrderPickingWorkflowName = "OrderPicking";

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderPickingModule"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public OrderPickingModule(IModuleContext context)
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

            Context.Container.Register<IOrderPickingRESTServiceProvider, OrderPickingRESTServiceProvider>();
            Context.Container.RegisterMultiple<IOrderPickingDataTransport>(new[] { typeof(OrderPickingFileDataTransport), typeof(OrderPickingRESTDataTransport) });
            Context.Container.Register<IOrderPickingDataProxy, OrderPickingDataProxy>().AsSingleton();

            Context.Container.Register<IOrderPickingDataService, OrderPickingDataService>().AsSingleton();
            Context.Container.Register<IOrderPickingEventsService, OrderPickingEventsService>();

            RegisterWorkflowController<OrderPickingGuidedWorkController>();

            RegisterWorkflowViewController<OrderPickingGetContainersController>();
            RegisterWorkflowViewController<OrderPickingConfirmNoMoreController>();
            RegisterWorkflowViewController<OrderPickingAcknowledgeLocationController>();
            RegisterWorkflowViewController<OrderPickingConfirmQuantityController>();
            RegisterWorkflowViewController<OrderPickingEnterProductController>();
            RegisterWorkflowViewController<OrderPickingEnterSubProductController>();
            RegisterWorkflowViewController<OrderPickingEnterQuantityController>();
            RegisterWorkflowViewController<OrderPickingAcknowledgeSubstitutionController>();
            RegisterWorkflowViewController<OrderPickingBooleanConfirmationController>();
            RegisterWorkflowViewController<OrderPickingOrderStatusController>();
            RegisterWorkflowViewController<OrderPickingReadyController>();
            RegisterWorkflowViewController<OrderPickingEnterStagingLocationController>();
            RegisterWorkflowViewController<OrderPickingConfirmStagingLocationController>();
            RegisterWorkflowViewController<OrderPickingConfirmOverflowController>();
            RegisterWorkflowViewController<OrderPickingAllDoneController>();

            RegisterWorkflowViewModel<OrderPickingAcknowledgeLocationViewModel>();
            RegisterWorkflowViewModel<OrderPickingAcknowledgeSubstitutionViewModel>();
            RegisterWorkflowViewModel<OrderPickingEnterDigitsViewModel>();
            RegisterWorkflowViewModel<OrderPickingBooleanConfirmationViewModel>();
            RegisterWorkflowViewModel<OrderPickingOrderStatusViewModel>();
            RegisterWorkflowViewModel<OrderPickingEnterStagingLocationViewModel>();
            RegisterWorkflowViewModel<OrderPickingConfirmStagingLocationViewModel>();
            RegisterWorkflowViewModel<OrderPickingConfirmOverflowViewModel>();

            RegisterView<OrderPickingReadyView>();
            RegisterView<OrderPickingView>();
            RegisterView<OrderPickingEnterProductView>();
            RegisterView<OrderPickingEnterQuantityView>();
            RegisterView<OrderPickingConfirmQuantityView>();
            RegisterView<OrderPickingOrderStatusView>();
            RegisterView<OrderPickingStagingLocationView>();
            RegisterView<OrderPickingConfirmationView>();

            RegisterWorkflowModelSingleton<IOrderPickingModel, OrderPickingModel>();

            Context.Container.Register<IOrderPickingIntentBuilder, OrderPickingIntentBuilder>();
            Context.Container.Register<IOrderPickingMobileDataExchange, OrderPickingMobileDataExchange>().AsSingleton();

            Translate.LocalizationResources.AddResource(OrderPickingResources.ResourceManager);
        }

        public override Task RunAsync()
        {
            var taskManagerModel = Context.Container.Resolve<ITaskManagerModel>();
            taskManagerModel.WorkflowNameToStartWorkflowEventName.Add(OrderPickingWorkflowName, OrderPickingEventName);
            //taskManagerModel.WorkflowNameToWorkItemType.Add(OrderPickingWorkflowName, nameof(OrderPickingWorkItem));
            //taskManagerModel.WorkItemTypeToTypeInfo.Add(nameof(OrderPickingWorkItem), new List<string> { OrderPickingWorkflowName, nameof(OrderPickingModule) });

            return base.RunAsync();
        }

        #endregion

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<OrderPickingModule>(
                RetailModule.ModuleInfo,
                GuidedWorkRunnerModule.ModuleInfo);

        /// <summary>
        /// Return the assembly that contains the OrderPickingModule, so
        /// that the GuidedWorkModule can access the resources contained
        /// herein (e.g. wfa, configuration, and data json files).
        /// </summary>
        /// <returns>The assembly</returns>
        private Assembly GetAssembly()
        {
            return typeof(OrderPickingModule).GetTypeInfo().Assembly;
        }
    }
}
