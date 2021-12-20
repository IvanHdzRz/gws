﻿//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp.Artisan
{
    using GuidedWork.Devices.NetCore;
    using Honeywell.Firebird;

    /// <summary>
    /// Bootstrap modules that must be initialized to use their servcices.
    /// </summary>
    public class AppBootstrap : BaseNetCoreAppBootstrap
    {
        /// <summary>
        /// Initialize the specificed app.
        /// </summary>
        /// <param name="app">The app instance to initialize</param>
        public override void Initialize(App app)
        {
            base.Initialize(app);

            var moduleCatalog = app.Container.Resolve<IModuleCatalog>();
            moduleCatalog.AddModule(LAppArtisanModule.ModuleInfo);
        }
    }
}
