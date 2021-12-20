//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace GWS
{
    using GuidedWork;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Honeywell.Firebird.Module;

    public class GWSModule : GuidedWorkModule
    {
        public GWSModule(IModuleContext context) : base(context)
        {
        }

        /// <summary>
        /// Register the services.
        /// </summary>
        public override void RegisterServices()
        {
            base.RegisterServices();

            Translate.LocalizationResources.AddResource(DevKitResources.ResourceManager);
        }

        /// <summary>
        /// A <see cref="IModuleInfo"/> that references this module.  Add it to
        /// a module catalog either directly or indirectly as a dependency of
        /// another module.
        /// </summary>
        public static readonly IModuleInfo ModuleInfo =
            StaticModuleInfo.Create<GWSModule>();
    }
}
