//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Bootstrap
{
    using DemoData;
    using GuidedWork;
    using GuidedWorkRunner;
    using GWS;
    using Honeywell.Firebird;
    using BasePickingExample;
    /// <summary>
    /// Bootstrap modules to register additional modules
    /// for application based on DevKit.
    /// </summary>
    /// <seealso cref="GuidedWork.GuidedWorkAppBootstrap" />
    public class AppBootstrap : GuidedWorkAppBootstrap
    {
        /// <summary>
        /// registers additional modules
        /// </summary>
        /// <param name="moduleCatalog">module catalog</param>
        protected override void RegisterModules(IModuleCatalog moduleCatalog)
        {
            // TODO: You can remove registrations for the following modules
            // if you would not like them to be selectable options in your application.
            // You can also remove the references to the corresponding projects
            // if you would like your application size to be smaller.
            moduleCatalog.AddModules(DemoDataModule.ModuleInfo,
               BasePickingExampleModule.ModuleInfo);

            // TODO: Register any new Module(s) for them to be available in the application.

            // Registration of the TCPSocketCommunicationModule is required if any
            // of your Modules use the Honeywell.GuidedWork.TCPSocketCommunication library.
            // By default VoiceLinkModule supplies that dependency.  If you remove
            // VoiceLinkModule from your app but need TCPSocketCommunicationModule, either
            // add it here or to your new module's dependencies.
            // moduleCatalog.AddModule(TCPSocketCommunicationModule.ModuleInfo);

            // Registration of the GWSModule will cause the application to use the common
            // DevKitResource resource files that contain keys from the underlying libraries.
            // If you change the values associated with those keys or add new languages,
            // this module needs to be registered for those changes to be reflected.
            // This module should always be added last so the DevKit resource files will
            // override those from the underlying libraries.
            moduleCatalog.AddModule(GWSModule.ModuleInfo);
        }
    }
}
