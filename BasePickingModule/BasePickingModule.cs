//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using Honeywell.Firebird.Module;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using GuidedWorkRunner;
    using Honeywell.Firebird;
    using TCPSocketCommunication;

    public class BasePickingModule : SimplifiedCoreModule<IBasePickingModel, BasePickingModel, BasePickingStateMachine>
    {
        public const string BasePickingWorkflowName = "BasePicking";

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePickingModule"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public BasePickingModule(IAppBaseModuleContext context)
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

            RegisterConfigRepository<IBasePickingConfigRepository, BasePickingConfigRepository>();

            Context.Container.Register<IBasePickingTCPSocketQueue, BasePickingTCPSocketQueue>();
            Context.Container.Register<IBasePickingTCPSocketService, BasePickingTCPSocketService>();
            Context.Container.Register<IBasePickingTCPSocketServicePropChangeManager, BasePickingTCPSocketServicePropChangeManager>();
            Context.Container.Register<IBasePickingTCPSocketServiceProvider, BasePickingTCPSocketServiceProvider>();

            Context.Container.Register<IBasePickingRESTServicePropChangeManager, BasePickingRESTServicePropChangeManager>();
            Context.Container.Register<IBasePickingRESTService, BasePickingRESTService>();
            Context.Container.Register<IBasePickingRESTServiceProvider, BasePickingRESTServiceProvider>();
            Context.Container.RegisterMultiple<IBasePickingDataTransport>(new[] { typeof(BasePickingFileDataTransport), typeof(BasePickingRESTDataTransport), typeof(BasePickingTCPSocketDataTransport) });
            Context.Container.Register<IBasePickingDataProxy, BasePickingDataProxy>().AsSingleton();
            Context.Container.Register<IBasePickingDataService, BasePickingDataService>().AsSingleton();

            RegisterVocabModule<BasePickingModuleVocab>(BasePickingWorkflowName);

            // Register base app requirements
            Translate.LocalizationResources.AddResource(BasePickingResources.ResourceManager);
        }

        #endregion

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<BasePickingModule>(
                TCPSocketCommunicationModule.ModuleInfo,
                GuidedWorkRunnerModule.ModuleInfo);
    }
}
