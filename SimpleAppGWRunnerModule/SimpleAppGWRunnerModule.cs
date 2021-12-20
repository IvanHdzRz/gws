//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace SimpleApp
{
    using System.Reflection;
    using System.Threading.Tasks;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.Module;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird;

    /// <summary>
    /// A module for the portion of the SimpleApp logic that connects it to
    /// the the Guided Work Runner.
    /// </summary>
    public class SimpleAppGWRunnerModule : BaseModule
    {
        private const string SimpleAppEventName = "StartSimpleAppWorkflow";

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAppGWRunnerModule"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public SimpleAppGWRunnerModule(IModuleContext context)
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

            RegisterWorkflowController<GenericGuidedWorkController<ISimpleAppModel>>("SimpleAppWorkflowController");

            // Register base app requirements
            RegisterWorkflowViewController<SimpleAppSettingsController>();
            RegisterWorkflowViewModel<SimpleAppSettingsViewModel>();
            RegisterView<SimpleAppSettingsView>();
        }

        /// <summary>
        /// Subclasses can use this method to start background threads or other long-running tasks.
        /// </summary>
        /// <returns>A Task representing the asynchronous Run operation.</returns>
        public override Task RunAsync()
        {
            var taskManagerModel = Context.Container.Resolve<ITaskManagerModel>();
            taskManagerModel.WorkflowNameToStartWorkflowEventName.Add(SimpleAppModule.SimpleAppWorkflowName, SimpleAppEventName);

            return base.RunAsync();
        }

        #endregion

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<SimpleAppGWRunnerModule>(
                GuidedWorkRunnerModule.ModuleInfo,
                SimpleAppModule.ModuleInfo);

        /// <summary>
        /// Return the assembly that contains the SimpleAppGWRunnerModule, so
        /// that the GuidedWorkModule can access the resources contained
        /// herein (e.g. wfa, configuration, and data json files).
        /// </summary>
        /// <returns>The assembly</returns>
        private Assembly GetAssembly()
        {
            return typeof(SimpleAppGWRunnerModule).GetTypeInfo().Assembly;
        }
    }
}
