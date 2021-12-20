//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace SimpleApp.Artisan
{
    using Honeywell.Firebird.Module;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using GuidedWork.VoiceCatalyst;
    using SimpleApp;
    using GuidedWork;
    using Honeywell.Firebird;

    /// <summary>
    /// A module for the portion of the SimpleApp logic that connects it to a Voice Artisan task.
    /// </summary>
    public class SimpleAppArtisanModule : AppBaseModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAppArtisanModule"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public SimpleAppArtisanModule(IAppBaseModuleContext context) : base(context)
        {
        }

        #region IModule implementation

        /// <summary>
        /// Register the services.
        /// </summary>
        public override void RegisterServices()
        {
            base.RegisterServices();

            // Register base app requirements
            Translate.LocalizationResources.AddResource(SimpleAppArtisanResources.ResourceManager);

            //Register Voice Catalyst dependencies
            Context.Container.Register<IModuleVocab, SimpleAppModuleVocab>();
            Context.Container.Register<IVoiceCatalystWorkflowDTO, SimpleAppVoiceCatalystWorkflowDTO>().AsMultiInstance();
        }

        #endregion

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<SimpleAppArtisanModule>(SimpleAppModule.ModuleInfo);
    }
}
