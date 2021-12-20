//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp.Artisan
{
    using GuidedWork;
    using GuidedWork.VoiceCatalyst;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Honeywell.Firebird.Module;

    public class LAppArtisanModule : AppBaseModule
    {
        public LAppArtisanModule(IAppBaseModuleContext context) : base(context)
        {
        }

        public override void RegisterServices()
        {
            base.RegisterServices();
            Translate.LocalizationResources.AddResource(LAppArtisanResources.ResourceManager);

            //Register Voice Catalyst dependencies
            Context.Container.Register<IModuleVocab, LAppModuleVocab>();
            Context.Container.Register<IVoiceCatalystWorkflowDTO, LAppVoiceCatalystWorkflowDTO>().AsMultiInstance();
        }

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<LAppArtisanModule>(LAppModule.ModuleInfo);
    }
}