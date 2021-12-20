//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2014 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace GWS.iOS
{
    using System;
    using Common.Logging;
    using Foundation;
    using Bootstrap;
    using GuidedWork;
    using GuidedWork.iOS;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.Module.Core.BlueStreak;
    using TinyIoC;
    using VoiceConsoleComm;
    using Honeywell.Firebird.Module.XamarinForms;
    using Honeywell.Firebird;

    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : GuidedWork.GuidedWorkAppDelegate
    {
        protected override ILog Log { get; } = LogManager.GetLogger(nameof(AppDelegate));

        // Get a reference to something from each of the TsiModels NuGets
        // to prevent the assemblies from being discarded by linker.
        Type _PublicRef_TsiModels_deDE = typeof(Honeywell.GuidedWork.TsiModels.de_DE.PublicReference);
        Type _PublicRef_TsiModels_enUS = typeof(Honeywell.GuidedWork.TsiModels.en_US.PublicReference);
        Type _PublicRef_TsiModels_esMX = typeof(Honeywell.GuidedWork.TsiModels.es_MX.PublicReference);
        Type _PublicRef_TsiModels_frCA = typeof(Honeywell.GuidedWork.TsiModels.fr_CA.PublicReference);

        /// <summary>
        /// Register iOS-only dependencies here.
        /// </summary>
        /// <param name="container">The app's Container.</param>
        protected override void DependencyRegistration(TinyIoCContainer container)
        {
            XplatDependencyOverrides.SetPreInitOverrides(container);
            container.Register<GuidedWork.IDeviceInfo, iOSDeviceInfo>();
            container.Register<IVoiceConsoleDeviceInfo, iOSDeviceInfo>();
            container.Register<IVolumeControl, iOSVolumeControl>();
            container.Register<ILogWriter, GuidedWork.GuidedWorkLogWriter>("GuidedWorkLogWriter");

            base.DependencyRegistration(container);

            BlueStreakModule.AddToModules(container);
            container.Register<DefaultRecognizerChoice, BlueStreakDefaultRecognizerChoice>();

            // Register the App from the GWS namespace so that it becomes the
            // application entrypoint and ensures App.xaml will be initialized
            GuidedWork.SharedDependencies.Register(container);

            container.Register<FormsApp, GWS.App>();
            container.Register<IAppBootstrap, AppBootstrap>();
            container.Register<IWorkflowActivityRegistrant, InitialWfaRegistrant>();
            container.Register<IAppWorkflowActivityRegistrant, AppWfaRegistrant>();

// The code below is provided as a sample of how to specify a callback method for validation of a server certificate.
// In the sample, the callback simply returns true to allow for testing communication with internal hosts which use 
// self signed certificates.  Note that if a handler is provided as per below, one should also consider the handler
// usage by the application's RESTHeaderUtilities class.
// #if DEBUG
//            container.Register<Honeywell.GuidedWork.Communication.IHttpClientHandlerUtility, NonValidatingHttpClientHandlerUtility>();
// #endif

#if OEDIPUS_BROKER_ENABLE
            //Note: In order to add Honeywell.Firebird.Module.OedipusBroker.iOS.dll and its dependencies to the ipa file, 
            //something in code must reference the assembly in a way that prevents optimization in Release mode.
            //A type reference will do the trick.

            var obModuleType = typeof(Honeywell.Firebird.Module.OedipusBroker.iOS.OedipusBrokerModule);

            var moduleCatalog = container.Resolve<IModuleCatalog>();

            moduleCatalog.AddModule(new ModuleInfo(obModuleType.Namespace, obModuleType.FullName));
#endif
        }
    }

    // See above - uncomment if not validating server certificates.
    //#if DEBUG
    //public class NonValidatingHttpClientHandlerUtility : Honeywell.GuidedWork.Communication.IHttpClientHandlerUtility
    //{
    //    public System.Net.Http.HttpMessageHandler Factory()
    //    {
    //        var handler = new System.Net.Http.HttpClientHandler();
    //        handler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
    //        return handler;
    //    }
    //}
    //#endif
}
