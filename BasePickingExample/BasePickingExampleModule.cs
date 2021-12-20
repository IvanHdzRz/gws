using GuidedWork;
using GuidedWorkRunner;
using Honeywell.Firebird;
using Honeywell.Firebird.Module;
using RESTCommunication;
using System.Reflection;
using System.Threading.Tasks;

namespace BasePickingExample
{
    public class BasePickingExampleModule : SimplifiedBaseModule<IBasePickingExampleModel, BasePickingExampleModel, BasePickingExampleBusinessLogic>
    {
        /// <summary>
        /// The name of the BasePickingExample workflow.
        /// </summary>
        public const string BasePickingExampleWorkflowName = "BasePickingExample";

        private const string BasePickingExampleEventName = "StartBasePickingExampleWorkflow";

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePickingExampleModule"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public BasePickingExampleModule(IModuleContext context)
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

            RegisterWorkflowController<GenericGuidedWorkController<IBasePickingExampleModel>>("BasePickingExampleWorkflowController");

            // Register base app requirements
            RegisterWorkflowViewController<BasePickingExampleSettingsController>();
            RegisterWorkflowViewModel<BasePickingExampleSettingsViewModel>();
            RegisterView<BasePickingExampleSettingsView>();

            Context.Container.Register<IBasePickingExampleConfigRepository, BasePickingExampleConfigRepository>();
            RegisterVocabModule<BasePickingExampleModuleVocab>(BasePickingExampleWorkflowName);
            InsertResourceManager(BasePickingExampleResources.ResourceManager);

            // Register Comm Services
            Context.Container.Register<IBasePickingExampleRESTServicePropChangeManager, BasePickingExampleRESTServicePropChangeManager>();
            Context.Container.Register<IBasePickingExampleRESTService, BasePickingExampleRESTService>();
            Context.Container.Register<IBasePickingExampleRESTServiceProvider, BasePickingExampleRESTServiceProvider>();
        }

        public override Task RunAsync()
        {
            var taskManagerModel = Context.Container.Resolve<ITaskManagerModel>();
            taskManagerModel.WorkflowNameToStartWorkflowEventName.Add(BasePickingExampleWorkflowName, BasePickingExampleEventName);

            return base.RunAsync();
        }

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<BasePickingExampleModule>(GuidedWorkRunnerModule.ModuleInfo);

        /// <summary>
        /// Return the assembly that contains the BasePickingExampleModule, so
        /// that the GuidedWorkModule can access the resources contained
        /// herein (e.g. wfa, configuration, and data json files).
        /// </summary>
        /// <returns>The assembly</returns>
        private Assembly GetAssembly()
        {
            return typeof(BasePickingExample.BasePickingExampleModule).GetTypeInfo().Assembly;
        }
    }

    /// <summary>
    /// BasePickingExample REST service
    /// NOTE: This is only used as a marker for the RESTQueue and does not need modified.
    /// </summary>
    public interface IBasePickingExampleRESTService : IRESTService
    {
    }

    /// <summary>
    /// BasePickingExample property change manager
    /// NOTE: This is only used as a marker for the RESTQueue and does not need modified.
    /// </summary>
    public interface IBasePickingExampleRESTServicePropChangeManager : IRESTServicePropChangeManager
    {
    }
}
