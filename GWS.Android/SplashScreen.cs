// <copyright company="HONEYWELL INTERNATIONAL INC">
//---------------------------------------------------------------------
//   COPYRIGHT (C) 2014-2015 HONEYWELL INTERNATIONAL INC. and/or one of
//   its wholly-owned subsidiaries, including Hand Held Products, Inc.,
//   Intermec, Inc., and/or Vocollect, Inc.
//   UNPUBLISHED - ALL RIGHTS RESERVED UNDER THE COPYRIGHT LAWS.
//   PROPRIETARY AND CONFIDENTIAL INFORMATION. DISTRIBUTION, USE
//   AND DISCLOSURE RESTRICTED BY HONEYWELL INTERNATIONAL INC.
//---------------------------------------------------------------------
// </copyright>

namespace GWS.Droid
{
    using Android.App;
    using Android.Content.PM;
    using Bootstrap;
    using GuidedWork;
    using GuidedWork.Devices.Droid;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.Module.Core.BlueStreak;
    using Honeywell.Firebird.Module.XamarinForms;
    using System;
    using System.Threading.Tasks;
#if OEDIPUS_BROKER_ENABLE
    using Honeywell.Firebird;
#endif
    using TinyIoC;
    using VoiceConsoleComm;

    /// <summary>
    /// An activity for a splash screen.  This is the first activty that will
    /// run.  When it finishes it will publish an Intent to transition to
    /// <see cref="SplashActivity"/>.
    /// </summary>
	[Activity(Theme = "@style/Theme.Splash", MainLauncher = true, NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity : GuidedWork.GuidedWorkInitActivity
    {
        protected override Type DestinationActivityType => typeof(MainActivity);

        /// <summary>
        /// Register Android-only dependencies here. They must be able to be
        /// safely registered prior to starting the <see cref="MainActivity"/>.
        /// </summary>
        /// <returns>
        /// A Task indicating the completion of the initialization
        /// operation.
        /// </returns>
        /// <param name="container">The app's Container.</param>
        protected override async Task Initialize(TinyIoCContainer container)
        {
            XplatDependencyOverrides.SetPreInitOverrides(container);
            container.Register<GuidedWork.IDeviceInfo, GuidedWork.Droid.AndroidDeviceInfo>();
            container.Register<IVoiceConsoleDeviceInfo, GuidedWork.Droid.AndroidDeviceInfo>();
            container.Register<IVolumeControl, AndroidVolumeControl>();
            container.Register<ILogWriter, GuidedWork.GuidedWorkLogWriter>("GuidedWorkLogWriter");

            await base.Initialize(container);

            // Register the App from the GWS namespace so that it becomes the
            // application entrypoint and ensures App.xaml will be initialized
            GuidedWork.SharedDependencies.Register(container);

            container.Register<FormsApp, GWS.App>();
            container.Register<IAppBootstrap, AppBootstrap>();
            container.Register<IWorkflowActivityRegistrant, InitialWfaRegistrant>();
            container.Register<IAppWorkflowActivityRegistrant, AppWfaRegistrant>();

            BlueStreakModule.AddToModules(container);

            container.Register<DefaultRecognizerChoice, BlueStreakDefaultRecognizerChoice>();

// The code below is provided as a sample of how to specify a callback method for validation of a server certificate.
// In the sample, the callback simply returns true to allow for testing communication with internal hosts which use 
// self signed certificates.  Note that if a handler is provided as per below, one should also consider the handler
// usage by the application's RESTHeaderUtilities class.
// #if DEBUG
//            container.Register<Honeywell.GuidedWork.Communication.IHttpClientHandlerUtility, NonValidatingHttpClientHandlerUtility>();
// #endif

#if OEDIPUS_BROKER_ENABLE
            var moduleCatalog = container.Resolve<IModuleCatalog>();
            moduleCatalog.AddModule(new ModuleInfo("Honeywell.Firebird.Module.OedipusBroker.Droid",
                                                   "Honeywell.Firebird.Module.OedipusBroker.Droid.OedipusBrokerModule"));
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
