//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using Honeywell.Firebird.Module;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird;
    using RESTCommunication;
    using Honeywell.GuidedWork.Core;
    using TCPSocketCommunication;

    /// <summary>
    /// The VoiceLink module.
    /// </summary>
    public class VoiceLinkModule : SimplifiedCoreModule<IVoiceLinkModel, VoiceLinkModel, VoiceLinkStateMachine>
    {
        /// <summary>
        /// The name of the VoiceLink workflow.
        /// </summary>
        public static string VoiceLinkWorkflowName { get; } = "VoiceLink";

        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceLinkModule"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public VoiceLinkModule(IAppBaseModuleContext context)
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

            RegisterConfigRepository<IVoiceLinkConfigRepository, VoiceLinkConfigRepository>();

            Context.Container.Register<IVoiceLinkRESTServicePropChangeManager, VoiceLinkRESTServicePropChangeManager>();
            Context.Container.Register<IVoiceLinkRESTService, VoiceLinkRESTService>();
            Context.Container.Register<IVoiceLinkRESTServiceProvider, VoiceLinkRESTServiceProvider>();
            Context.Container.Register<IVoiceLinkTCPSocketServicePropChangeManager, VoiceLinkTCPSocketServicePropChangeManager>();
            Context.Container.Register<IVoiceLinkTCPSocketService, VoiceLinkTCPSocketService>();
            Context.Container.Register<IVoiceLinkTCPSocketServiceProvider, VoiceLinkTCPSocketServiceProvider>();
            Context.Container.Register<IVoiceLinkTCPSocketQueue, VoiceLinkTCPSocketQueue>().AsMultiInstance();
            Context.Container.RegisterMultiple<IVoiceLinkDataTransport>(new[] { typeof(VoiceLinkRESTDataTransport),
                                                                                typeof(VoiceLinkFileDataTransport),
                                                                                typeof(VoiceLinkTCPSocketDataTransport)});

            Context.Container.Register<IVoiceLinkDataProxy, VoiceLinkDataProxy>().AsSingleton();
            Context.Container.Register<IVoiceLinkDataService, VoiceLinkDataService>().AsSingleton();

            RegisterVocabModule<VoiceLinkModuleVocab>(VoiceLinkWorkflowName);

            Translate.LocalizationResources.AddResource(VoiceLinkResources.ResourceManager);
        }

        #endregion

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<VoiceLinkModule>(
                RESTCommunicationModule.ModuleInfo,
                TCPSocketCommunicationModule.ModuleInfo,
                GuidedWorkCoreModule.ModuleInfo);
    }
}
