//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Reflection;
    using System.Threading.Tasks;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Honeywell.Firebird.Module;

    public class ReceivingModule : GuidedWorkModule
    {
        private const string ReceivingEventName = "StartReceivingWorkflow";
        private const string ReceivingWorkflowName = "Receiving";

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivingModule"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public ReceivingModule(IModuleContext context)
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

            //Context.Container.Register<IReceivingConfigRepository, ReceivingConfigRepository>();
            //Context.Container.Register<IReceivingRESTHeaderUtilities, ReceivingRESTHeaderUtilities>();
            //Context.Container.Register<IReceivingRESTServicePropChangeManager, ReceivingRESTServicePropChangeManager>();
            //Context.Container.Register<IReceivingRESTService, ReceivingRESTService>();
            Context.Container.Register<IReceivingRESTServiceProvider, ReceivingRESTServiceProvider>();
            Context.Container.RegisterMultiple<IReceivingDataTransport>(new[] { typeof(ReceivingFileDataTransport), typeof(ReceivingRESTDataTransport) });
            Context.Container.Register<IReceivingDataProxy, ReceivingDataProxy>().AsSingleton();
            Context.Container.Register<IReceivingDataService, ReceivingDataService>().AsSingleton();

            Context.Container.Register<IModuleVocab, ReceivingModuleVocab>("Receiving");

            RegisterWorkflowController<ReceivingGuidedWorkController>();

            // Register base app requirements
            //RegisterWorkflowViewController<IgnitoServerSettingsController>();
            //RegisterWorkflowViewModel<IgnitoServerSettingsViewModel>();
            //RegisterView<IgnitoServerSettingsView>();
            Translate.LocalizationResources.AddResource(ReceivingResources.ResourceManager);

            RegisterWorkflowViewController<ReceivingItemsController>();
            RegisterWorkflowViewController<ReceivingSummaryController>();
            RegisterWorkflowViewController<ReceivingPrintLabelController>();
            RegisterWorkflowViewController<ReceivingConfirmConditionController>();
            RegisterWorkflowViewController<ReceivingConfirmQuantityController>();
            RegisterWorkflowViewController<ReceivingEnterHiQuantityController>();
            RegisterWorkflowViewController<ReceivingEnterTiQuantityController>();
            RegisterWorkflowViewController<ReceivingDamageListController>();
            RegisterWorkflowViewController<ReceivingBooleanConfirmationController>();
            RegisterWorkflowViewController<ReceivingReadyController>();

            RegisterWorkflowViewModel<ReceivingEnterDigitsViewModel>();
            RegisterWorkflowViewModel<ReceivingBooleanConfirmationViewModel>();
            RegisterWorkflowViewModel<ReceivingConfirmConditionViewModel>();
            RegisterWorkflowViewModel<ReceivingItemsViewModel>();
            RegisterWorkflowViewModel<ReceivingSummaryViewModel>();

            RegisterView<ReceivingView>();
            RegisterView<ReceivingEnterHiQuantityView>();
            RegisterView<ReceivingEnterTiQuantityView>();
            RegisterView<ReceivingItemsView>();
            RegisterView<ReceivingDamageSelectView>();
            RegisterView<ReceivingSummaryView>();

            RegisterWorkflowModelSingleton<IReceivingModel, ReceivingModel>();
            Context.Container.Register<IReceivingIntentBuilder, ReceivingIntentBuilder>();
            Context.Container.Register<IReceivingMobileDataExchange, ReceivingMobileDataExchange>().AsSingleton();
        }

        public override Task RunAsync()
        {
            var taskManagerModel = Context.Container.Resolve<ITaskManagerModel>();
            taskManagerModel.WorkflowNameToStartWorkflowEventName.Add(ReceivingWorkflowName, ReceivingEventName);

            return base.RunAsync();
        }

        #endregion

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<ReceivingModule>(GuidedWorkRunnerModule.ModuleInfo);

        /// <summary>
        /// Return the assembly that contains the ReceivingModule, so
        /// that the GuidedWorkModule can access the resources contained
        /// herein (e.g. wfa, configuration, and data json files).
        /// </summary>
        /// <returns>The assembly</returns>
        private Assembly GetAssembly()
        {
            return typeof(Receiving.ReceivingModule).GetTypeInfo().Assembly;
        }
    }
}
