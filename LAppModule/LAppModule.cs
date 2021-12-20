//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System.Reflection;
    using System.Threading.Tasks;
    using Honeywell.Firebird.Module;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird;

    public class LAppModule : SimplifiedBaseModule<ILAppModel, LAppModel, LAppBusinessLogic>
    {
        /// <summary>
        /// The name of the LAppModule workflow.
        /// </summary>
        public const string LAppWorkflowName = "LApp";

        private const string LAppModuleEventName = "StartLAppWorkflow";

        /// <summary>
        /// Initializes a new instance of the <see cref="LAppModule"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public LAppModule(IModuleContext context)
                : base(context)
        {
        }

        public LAppModule(IAppBaseModuleContext context)
                : base(context)
        {
        }

        /// <summary>
        /// Register the services.
        /// </summary>
        public override void RegisterServices()
        {
            base.RegisterServices();

            // Register base app requirements
            var workflowResourceRegistry = Context.Container.Resolve<IWorkflowResourceRegistry>();
            workflowResourceRegistry.AddSearchAssembly(GetAssembly());

            RegisterWorkflowController<GenericGuidedWorkController<ILAppModel>>("LAppWorkflowController");

            // Register base app requirements
            RegisterWorkflowViewController<LAppSettingsController>();
            RegisterWorkflowViewModel<LAppSettingsViewModel>();
            RegisterView<LAppSettingsView>();

            Context.Container.Register<ILAppConfigRepository, LAppConfigRepository>();
            RegisterVocabModule<LAppModuleVocab>(LAppWorkflowName);
            InsertResourceManager(LAppResources.ResourceManager);

            // Register Comm Services
            Context.Container.Register<ILAppRESTHeaderUtilities, LAppRESTHeaderUtilities>();
            Context.Container.Register<ILAppRESTServicePropChangeManager, LAppRESTServicePropChangeManager>();
            Context.Container.Register<ILAppRESTService, LAppRESTService>();
            Context.Container.Register<ILAppRESTServiceProvider, LAppRESTServiceProvider>();
            Context.Container.RegisterMultiple<ILAppDataTransport>(new[] { typeof(LAppFileDataTransport), typeof(LAppRESTDataTransport) });
            Context.Container.Register<ILAppDataProxy, LAppDataProxy>().AsSingleton();
            Context.Container.Register<ILAppDataService, LAppDataService>().AsSingleton();
        }

        public override Task RunAsync()
        {
            var taskManagerModel = Context.Container.Resolve<ITaskManagerModel>();
            taskManagerModel.WorkflowNameToStartWorkflowEventName.Add(LAppWorkflowName, LAppModuleEventName);

            return base.RunAsync();
        }

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<LAppModule>(GuidedWorkRunnerModule.ModuleInfo);

        /// <summary>
        /// Return the assembly that contains the LAppModule, so
        /// that the GuidedWorkModule can access the resources contained
        /// herein (e.g. wfa, configuration, and data json files).
        /// </summary>
        /// <returns>The assembly</returns>
        private Assembly GetAssembly()
        {
            return typeof(global::LApp.LAppModule).GetTypeInfo().Assembly;
        }
    }
}
