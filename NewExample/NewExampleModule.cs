using GuidedWork;
using GuidedWorkRunner;
using Honeywell.Firebird;
using Honeywell.Firebird.Module;
using RESTCommunication;
using System.Reflection;
using System.Threading.Tasks;

namespace NewExample
{
    public class NewExampleModule : SimplifiedBaseModule<INewExampleModel, NewExampleModel, NewExampleBusinessLogic>
    {
        /// <summary>
        /// The name of the NewExample workflow.
        /// </summary>
        public const string NewExampleWorkflowName = "NewExample";

        private const string NewExampleEventName = "StartNewExampleWorkflow";

        /// <summary>
        /// Initializes a new instance of the <see cref="NewExampleModule"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public NewExampleModule(IModuleContext context)
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

            RegisterWorkflowController<GenericGuidedWorkController<INewExampleModel>>("NewExampleWorkflowController");

            // Register base app requirements
            RegisterWorkflowViewController<NewExampleSettingsController>();
            RegisterWorkflowViewModel<NewExampleSettingsViewModel>();
            RegisterView<NewExampleSettingsView>();

            Context.Container.Register<INewExampleConfigRepository, NewExampleConfigRepository>();
            RegisterVocabModule<NewExampleModuleVocab>(NewExampleWorkflowName);
            InsertResourceManager(NewExampleResources.ResourceManager);

            // Register Comm Services
            Context.Container.Register<INewExampleRESTServicePropChangeManager, NewExampleRESTServicePropChangeManager>();
            Context.Container.Register<INewExampleRESTService, NewExampleRESTService>();
            Context.Container.Register<INewExampleRESTServiceProvider, NewExampleRESTServiceProvider>();
        }

        public override Task RunAsync()
        {
            var taskManagerModel = Context.Container.Resolve<ITaskManagerModel>();
            taskManagerModel.WorkflowNameToStartWorkflowEventName.Add(NewExampleWorkflowName, NewExampleEventName);

            return base.RunAsync();
        }

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<NewExampleModule>(GuidedWorkRunnerModule.ModuleInfo);

        /// <summary>
        /// Return the assembly that contains the NewExampleModule, so
        /// that the GuidedWorkModule can access the resources contained
        /// herein (e.g. wfa, configuration, and data json files).
        /// </summary>
        /// <returns>The assembly</returns>
        private Assembly GetAssembly()
        {
            return typeof(NewExample.NewExampleModule).GetTypeInfo().Assembly;
        }
    }

    /// <summary>
    /// NewExample REST service
    /// NOTE: This is only used as a marker for the RESTQueue and does not need modified.
    /// </summary>
    public interface INewExampleRESTService : IRESTService
    {
    }

    /// <summary>
    /// NewExample property change manager
    /// NOTE: This is only used as a marker for the RESTQueue and does not need modified.
    /// </summary>
    public interface INewExampleRESTServicePropChangeManager : IRESTServicePropChangeManager
    {
    }
}
