//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePickingArtisanModule
{
    using Honeywell.Firebird.Module;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using GuidedWork.VoiceCatalyst;
    using BasePicking;
    using GuidedWork;
    using Honeywell.Firebird;

    /// <summary>
    /// A module for the portion of the BasePicking logic that connects it to a Voice Artisan task.
    /// </summary>
    public class BasePickingArtisanModule : AppBaseModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasePickingArtisanModule"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public BasePickingArtisanModule(IAppBaseModuleContext context)
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
            Translate.LocalizationResources.AddResource(BasePickingArtisanResources.ResourceManager);

            //Register Voice Catalyst dependencies
            Context.Container.Register<IModuleVocab, BasePickingModuleVocab>();
            Context.Container.Register<IVoiceCatalystWorkflowDTO, BasePickingVoiceCatalystWorkflowDTO>().AsMultiInstance();
        }

        #endregion

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<BasePickingArtisanModule>(BasePickingModule.ModuleInfo);
    }
}
