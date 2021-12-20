//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2014 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace GWS.Droid
{
    using Android.App;
    using Android.Bluetooth;
    using Android.Content;
    using Android.Content.PM;
    using Android.OS;
    using Common.Logging;
    using GuidedWork;
    using GuidedWork.Devices.Droid;
    using GuidedWork.Droid;
    using Honeywell.Firebird.CoreLibrary.Droid;

    [Activity(Theme = "@style/CoreTheme", Label = "GuidedWork", MainLauncher = false,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : GuidedWorkMainActivity
    {
        protected override ILog Log { get; } = LogManager.GetLogger(nameof(MainActivity));

        private readonly GuidedWorkAndroidExceptionLogger _ExceptionLogger = new GuidedWorkAndroidExceptionLogger();
        protected override AndroidExceptionLogger ExceptionLogger => _ExceptionLogger;

        private BluetoothAdapterReceiver _BluetoothReceiver;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Info(m => m("MainActivity.OnCreate"));

            _BluetoothReceiver = new BluetoothAdapterReceiver();
            RegisterReceiver(_BluetoothReceiver, new IntentFilter(BluetoothAdapter.ActionStateChanged));

            // Set up things that base class requires to be done from
            // this assembly or from the derived class.
            ToolbarResource = Resource.Layout.core_toolbar;

            ContentResolver.RegisterContentObserver(Android.Provider.Settings.System.ContentUri, true, VolumeObserver.Instance);
            base.OnCreate(savedInstanceState);
        }

        protected override void OnResume()
        {
            Log.Info(m => m("MainActivity.OnResume"));
            base.OnResume();
        }

        protected override void OnPause()
        {
            Log.Info(m => m("MainActivity.OnPause - IsFinishing={0}", IsFinishing));
            base.OnPause();
        }

        protected override void OnDestroy()
        {
            Log.Info(m => m("MainActivity.OnDestroy"));

            //un-register our bluetooth receiver so this application isn't
            //forcing bluetooth to be on when it's not running.
            UnregisterReceiver(_BluetoothReceiver);

            ContentResolver.UnregisterContentObserver(VolumeObserver.Instance);
            base.OnDestroy(); // Note that the base class terminates the app hard here.
        }
    }
}
