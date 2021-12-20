//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace SimpleApp
{
    using GuidedWorkRunner;
    using Honeywell.Firebird;
    using Honeywell.Firebird.Module;
    using Honeywell.GuidedWork.Core;

    /// <summary>
    /// The SimpleApp module.
    /// </summary>
    public class SimpleAppModule : SimplifiedCoreModule<ISimpleAppModel, SimpleAppModel, SimpleAppBusinessLogic>
    {
        /// <summary>
        /// The name of the SimpleApp workflow.
        /// </summary>
        public const string SimpleAppWorkflowName = "SimpleApp";

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAppModule"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public SimpleAppModule(IAppBaseModuleContext context)
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

            Context.Container.Register<ISimpleAppConfigRepository, SimpleAppConfigRepository>();

            RegisterVocabModule<SimpleAppModuleVocab>(SimpleAppWorkflowName);

            InsertResourceManager(SimpleAppResources.ResourceManager);

            //Register Comm Services
            Context.Container.Register<ISimpleAppRESTServicePropChangeManager, SimpleAppRESTServicePropChangeManager>();
            Context.Container.Register<ISimpleAppRESTService, SimpleAppRESTService>();
            Context.Container.Register<ISimpleAppRESTServiceProvider, SimpleAppRESTServiceProvider>();
        }

        #endregion

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<SimpleAppModule>(GuidedWorkCoreModule.ModuleInfo);
    }
}
