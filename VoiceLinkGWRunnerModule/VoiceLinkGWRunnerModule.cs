//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLinkGWRunnerModule
{
    using System.Reflection;
    using System.Threading.Tasks;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.Module;
    using GuidedWork;
    using GuidedWorkRunner;
    using VoiceLink;
    using Honeywell.Firebird;

    /// <summary>
    /// A module for the portion of the VoiceLink logic that connects it to
    /// the the Guided Work Runner.
    /// </summary>
    public class VoiceLinkGWRunnerModule : BaseModule
    {
        private const string VoiceLinkEventName = "StartVoiceLinkWorkflow";

        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceLinkGWRunnerModule"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public VoiceLinkGWRunnerModule(IModuleContext context)
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

            RegisterWorkflowController<GenericGuidedWorkController<IVoiceLinkModel>>("VoiceLinkWorkflowController");

            // Register base app requirements
            RegisterWorkflowViewController<VoiceLinkServerSettingsController>();
            RegisterWorkflowViewModel<VoiceLinkServerSettingsViewModel>();
            RegisterView<VoiceLinkServerSettingsView>();
        }

        /// <summary>
        /// Subclasses can use this method to start background threads or other long-running tasks.
        /// </summary>
        /// <returns>A Task representing the asynchronous Run operation.</returns>
        public override Task RunAsync()
        {
            var taskManagerModel = Context.Container.Resolve<ITaskManagerModel>();
            taskManagerModel.WorkflowNameToStartWorkflowEventName.Add(VoiceLinkModule.VoiceLinkWorkflowName, VoiceLinkEventName);

            return base.RunAsync();
        }

        #endregion

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<VoiceLinkGWRunnerModule>(
                GuidedWorkRunnerModule.ModuleInfo,
                VoiceLinkModule.ModuleInfo);

        /// <summary>
        /// Return the assembly that contains the VoiceLinkGWRunnerModule, so
        /// that the GuidedWorkModule can access the resources contained
        /// herein (e.g. wfa, configuration, and data json files).
        /// </summary>
        /// <returns>The assembly</returns>
        private Assembly GetAssembly()
        {
            return typeof(VoiceLinkGWRunnerModule).GetTypeInfo().Assembly;
        }
    }
}
