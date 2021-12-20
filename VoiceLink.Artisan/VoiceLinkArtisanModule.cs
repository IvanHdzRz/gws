//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLinkArtisanModule
{
    using Honeywell.Firebird.Module;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using GuidedWork.VoiceCatalyst;
    using VoiceLink;
    using GuidedWork;
    using Honeywell.Firebird;

    /// <summary>
    /// A module for the portion of the VoiceLink logic that connects it to a Voice Artisan task.
    /// </summary>
    public class VoiceLinkArtisanModule : AppBaseModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceLinkArtisanModule"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public VoiceLinkArtisanModule(IAppBaseModuleContext context)
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

            // Register base app requirements
            Translate.LocalizationResources.AddResource(VoiceLinkArtisanResources.ResourceManager);

            //Register Voice Catalyst dependencies
            Context.Container.Register<IVoiceCatalystRequiredVocab, VoiceCatalystRequiredVocab>();
            Context.Container.Register<IModuleVocab, VoiceLinkModuleVocab>();
            Context.Container.Register<IVoiceCatalystWorkflowDTO, VoiceLinkVoiceCatalystWorkflowDTO>().AsMultiInstance();
        }

        #endregion

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<VoiceLinkArtisanModule>(VoiceLinkModule.ModuleInfo);
    }
}
