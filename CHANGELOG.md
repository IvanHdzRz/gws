# What's new in DevKit 1.4

## Features
1. New fields added to the InteractiveItems objects used in the overflow menu to make
   setting up of additional vocabulary and their functionality easier
      * itemValidationDelgate - A validation message can now be added to an overflow menu item 
        when it is not currently available. It is a voice best practice to include additional 
        commands when a user may expect them, but they are not currently valid. (i.e. If "Skip Aisle"
        command is allowed, but not allowed when worker is on last Aisle). By including the
        command, but speak a validation message to user (i.e. "Cannot skip when on last aisle"),
        lets worker know command was recognized, but cannot currently be performed. 
        When creating an InteractiveItem object to add to overflow menu, you can now
        set the itemValidationDelgate. If set the message will be spoken
        to worker if command selected then instead of returning the intent and requiring
        the business logic to re-issue the intent.
      * useAsBackButtonAction - boolean flag. When set to true and the back button is pressed
        when running intent, then the command with the flag set will be returned/executed after
        it is validated and confirmed (if also set). If more than one item is set as back button
        action, than only the first one will be used.
2. Certificate public key pinning added for security. Configured off by default. If the server's public key is updated the app must be
   reinstalled to trust the new public key
    * PublicKeyPinning - string value to be set to "true" or "false" in Server Settings Config. When set to "true" first use will trust the
    public key from server and save it to config and then will compare server public key to saved key on every subsequent use. If set to
    "false" the app will not enforce public key pinning
    * The mechanism for bypassing certificate validation for debugging has changed.  See GWS.Android\SplashScreen.cs
    and GWS.iOS\AppDelegate.cs for the new mechanism.
3. Ability to disable screenshots added for security. Configured to allow screenshots by default
    * AllowScreenshots - string value to be set to "true" or "false" in the Application Preferences Config when set to "true" screenshots are
    enabled. When set to "false" the screenshot functionality is disabled and the app thumbnail is a blank screen
4. Ability to set application level settings for vocabulary word. User can now set the recognition decode sensitivty for vocabulary words,
   and disable vocabulary that is not being used by end user.
5. Ability to download BSX2 models
    * BSX2 models can now be downloaded from Enterprise Voice via the languages menu. There are three different icons that represent the
    status of BSX2:
        * Microphone without checkmark: BSX2 model for that language is not on the device and is not available to download.
        * Microphone with checkmark: BSX2 is available for that language and is on the device.
        * Cloud download: A new version of BSX2 model is available to download from Enterprise Voice.
6. Ability to load BSX2 models to the device manually
    * BSX2 models can now be loaded to the device manually by pushing the model's .zip file to `/sdcard/Android/data/[app bundle id]/files/AsrModels/`
7. Ability to disable BSX2 models via a config
    * BSX2 models that are on the device can be manually disabled via a config with `"PickUpAndGoModelsEnabledOverride"`.
        * For example, to disable es-MX and fr-CA set their values to `"false"`:

            ```json
            "PickUpAndGoModelsEnabledOverride": {
                "es-MX": "false",
                "fr-CA": "false"
            }
            ```

        * To re-enable, change the value in the config to `"true"`:

            ```json
            "PickUpAndGoModelsEnabledOverride": {
                "es-MX": "true",
                "fr-CA": "false"
            }
            ```
8. Ability to make UI element banners, labels, label infos, and values hyperlinks
    * Three new properties have been added to the UI element object (ElementURL, LabelInfoURL, and ValueURL).
        * These properties are used to specify URLs for particular element types.
    * When UI element text becomes a link it will automatically be underlined.
        * The color of the text is not changed automatically but can be changed when creating the intent using the TextColor or Style properties of the element.

9. Anchor Words support added for A700x devices. Anchor words are now supported on Android, iOS, and A700x devices. Anchor words are
   vocabulary words that can optionally be provided to float, value, or long value intents enabling an operator to provide context to their
   input. For example, given a value intent requesting the weight of an object, anchor words would require the operator to specify "ounces" or
   "pounds" after the number.

## Changes

1. When using the simplified pattern, InteractiveItems specified as arguments in 
   the ConfigureDisplayState helper methods as well as those specified in the model's 
   AllowedAdditionalVocab method will be included.  Previously, if InteractiveItems were 
   specified in the helper method those specified in the AllowedAdditionalVocab would not be included.
2. When using the ConfigureLogin helper method with the simplified pattern two new named arguments 
   are available to specify the message displayed on the background spinner while retrieving operators 
   from VoiceConsole and while signing in the operator.  These named arguments are 
   retrieveOpersBgndMsgKey and signOnBgndMsgKey respectively.
3. How to define that Overflow menu items get confirmed has changed. In work flow objects, the List property
   AdditionalProperties.ConfirmOverflowItem no longer exists. To configure an overflow menu item to be confirmed
   you now set the new "Confirm" property when creating an InteractiveItem to add to overflow menu. This
   allows for defining the overflow menu item all in one place.
4. On Android devices through Android 7, the presence of a DevKit-based app prevents audio from 
   playing in a previously opened media app when you connect a headset.  This feature was
   broken on Android 8 and newer, but it is now fixed for devices that have apps based on
   DevKit 1.4 and newer.
5. DevKit has been updated from the Android 9 SDK (API Level 28) to the Android 10 SDK (API Level 29).
   See https://docs.microsoft.com/en-us/xamarin/android/platform/android-10 for an overview of the
   new SDK and its support in Xamarin.Android.
   To build DevKit 1.4 you must install the new SDK in Visual Studio 2019.
   Be aware of behavior changes documented at https://developer.android.com/about/versions/10/behavior-changes-10
      * An important change in the Android 10 SDK is the concept of scoped (external) storage.  DevKit has
        historically used unscoped external storage to cache certain files between application installs
        By default, apps compiled against the Android 10 SDK do not permit the use of unscoped external storage.
        DevKit currently opts out of this feature.  See
        https://developer.android.com/training/data-storage/use-cases#opt-out-scoped-storage for details
        In a future release, DevKit will move that data to an internal location and update to the
        Android 11 SDK (which makes the move mandatory).
      * If you are currently using any file access APIs that write to external storage outside of your
        application's scoped area, begin migrating to scoped storage in preparation for a future
        update to the Android 11 (API Level 30) SDK.  See
        https://developer.android.com/about/versions/11/privacy/storage#scoped-storage  for details.
6. DevKit has migrated from using the obsolete Android Support Libraries to Android Jetpack.  See
   https://docs.microsoft.com/en-us/xamarin/xamarin-forms/platform/android/androidx-migration for
   details.
7. Xamarin.Forms has been updated.  Any projects with explicit Xamarin.Forms references need
   to update to the version referenced in the example module projects (e.g. ReceivingModule, Bootstrap).
   At the moment that is version 4.8.0.1451.  See:
   https://docs.microsoft.com/en-us/xamarin/xamarin-forms/release-notes/4.8/4.8.0-sr2 for details. 
8. We now test DevKit with Visual Studio 2019 version 16.7.5 and recommend using it or a later
   version for development.
9. We explicitly set the AndroidManifest.xml's android:extractNativeLibs to "true".  Visual Studio 15.7
   defaults to "false", but when an APK is loaded Xamarin.Android code embedded in it tries to locate
   and load the mono native library using the wrong approach, and fails to find the library where it
   expects it to be.  This causes install-time errors.  The workaround is to restore the old behavior
   of Visual Studio 15.6 explicitly.  See https://github.com/xamarin/xamarin-android/pull/5021 and the
   bugs linked to it for details, and https://developer.android.com/guide/topics/manifest/application-element#extractNativeLib
   for the Android reference on the attribute.
10. Improved defining of vocabulary and use throughout a module's code. In prior releases VocabWordInfo objects where
    defined from resource keys, and then later in the code having to reference a resource again to use in places like
    overflow menu. This was prone to typos and maintanence issues when something was changed. Now when defining your
    vocabulary, instead of adding new VocabWordInfo objects to the platform independent vocabulary list, you can
    now create a public static readonly property in your ModuleVocab class. These static properties will automatically
    be added to the module's vocab and easily used through your module's code "YourModuleVocab.DesiredVocabularyWord".
    In addition the InteractiveItem object can now take a VocabWordInfo object in the constructor making it easier
    to assign your vocabulary to the overflow menu items.

    Example from Devkit LApp sample:

```c#
   //Defining vocab in module vocab class
   public class LAppModuleVocab : DefaultModuleVocab
   {
       //Build common platform independent vocab, from default vocabulary
       public override VocabWordInfo[] PlatformIndependentVocab { get; } =
           Numerics
               .Concat(Alphas)
               .Concat(BaseRequiredVocab)
               .Concat(Common)
           .ToArray();

       //Defined platform independent, app specific vocab as static for easy reference later
       public static readonly VocabWordInfo ChangeWareHouse = new VocabWordInfo("LApp_Overflow_ChangeWarehouse");
       public static readonly VocabWordInfo SkipProduct = new VocabWordInfo("LApp_Overflow_SkipProduct");
 }
```

```c#
   //using vocabulary in overflow menu
   var wfo = WorkflowObjectFactory.CreateGetLongValueIntent(...);
   wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(LAppModuleVocab.ChangeWareHouse, confirm: true));
```

11. When doing voice template training, the speaking of the vocabulary words is now muted. This is considered a best
    practice so workers pronounce the words during training as they would normally speak the words during the use of the
    application, instead of repeating the words how they heard them spoken via the TTS. The speaking of the vocabulary
    can be turned on/off by tapping the speaker icon on the screen, but the default is now always to not speak the vocabulary
    word.
12. Honeywell.Firebird.StaticModuleInfo has a new `Create<T>` method that faciliates the creation
    of loadable Module objects.  StaticModuleInfo now supports (optional) specification of
    dependencies.  This removes some boilerplate from applications and isolates them from the
    addition of extra dependencies in the future.
13. The Honeywell.Firebird.IModuleCatalog interface has a new method, `AddModules`.  It is a
    convenience method allowing the addition of multiple `IModuleInfo` instances in a single
    call.
14. In order to improve modularity, the Honeywell.Firebird.CoreLibrary.BaseModule class has
    replaced the `RegisterXamarinPageView<TXPV>`, `RegisterScannerView<TSV>`, and
    `RegisterDialogueView<TDV>` methods with a single `RegisterView<TV>` method that accepts
    all view types. For most devkit apps this only means replacing calls of
    `RegisterXamarinPageView<TXPV>` with `RegisterView<TV`.
15. Some classes referenced from C# files have had their namespaces change:
    - GuidedWork.Droid.BluetoothAdapterReceiver is now GuidedWork.Devices.Droid.BluetoothAdapterReceiver.
    - GuidedWork.Droid.AndroidVolumeControl is now GuidedWork.Devices.Droid.AndroidVolumeControl.
16. Some classes referenced from XAML files have had namespaces/assembly changes. This may require changes
    in certain XAML views, e.g.:
    * xmlns:hfmcl="clr-namespace:Honeywell.Firebird.CoreLibrary;assembly=Honeywell.GuidedWork.Base" to
      xmlns:hfmcl="clr-namespace:Honeywell.Firebird.CoreLibrary;assembly=Honeywell.GuidedWork.XF"
    * xmlns:i18n="clr-namespace:Honeywell.Firebird.CoreLibrary.Localization;assembly=Honeywell.GuidedWork.Base" to
      xmlns:i18n="clr-namespace:Honeywell.Firebird.CoreLibrary.Localization.Xaml;assembly=Honeywell.GuidedWork.XF"

## Breaking Changes

1. The processAction argument name in InteractiveItem object constructors has changed to 
   processActionAsync.  The return type of the delegates passed as that argument has changed 
   from CoreAppSMState to Task<CoreAppSMState>.  This allows a workflow to make asynchronous calls 
   during execution of the InteractiveItem.
2. The ConfigureDisplayState and ConfigureDisplayStateFollowedByBackgroundActivity methods have been 
   combined into the ConfigureDisplayState method.  The ConfigureDisplayState method now contains 3 
   additional optional arguments (followedByBackgroundActivity, activityHeader, activityHeadersArgs), 
   2 of which were in the ConfigureDisplayStateFollowedByBackgroundActivity method and have the same functions.
3. AllowCancel or AllowCancelEntry properties were removed from the GetValue, GetLongValue, GetFloat, and MenuItem intents. As
   a result the default behavior was also changed in that AllowCancel is disabled. This was done to allow end applications more
   control over this particular vocabulary, such as what vocab word to use, or whether to confirm the entry. 
   If code was already setting the AllowCancel or AllowCancelEntry to "false" you can simply remove the setting. If AllowCancel 
   was enabled (was previous default) then to enable it, simply add it to the Workflow intent's AdditionalOverflowMenuOptions 
   
   ```c#
       //Add cancel to overflow menu, can also set confirmation as well, or use different vocabulary
       wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(DefaultModuleVocab.Vocab_Cancel, confirm: false));
   ```
   
   **IMPORTANT:** If cancel was enabled, then where code was checking for the cancel would also need to be updated
                   this would not show as a compile error since it is most likely a simple string comparison
   ```c#
       //Before may have been something like 
       if (result != Translate.GetLocalizedTextForKey("Abortable_AbortBtnText"))
	   
       //After, if using default Vocab_Cancel, check would be
       if (result != DefaultModuleVocab.Vocab_Cancel.IdentificationKey)
   ```
   For the value entry intents, adding the cancel vocabulary to overflow menu was all that was done, and 
   by adding as described above will result in same behavior, allowing for more flexibility. However the AllowCancel
   in the MenuItem intents also would return cancel if user spoke "no" to all options. To support this functionality
   a new property was added "ReturnValueIfNoOptionsSelected". If this property is set, then the value of the property
   is returned, if not set, then user is left in dialogue and may review the options again. This can be set to a value
   defined in the overflow menu item (i.e. cancel) or to a totally seperate string value so it can be distinguished 
   seperatly from user speaking cancel. To get the same behaviour as before in the MenuItem intent, simply add cancel
   to the overflow menu items (like above) and set new property to the cancel vocabulary key
   
   ```c#
       //To get same cancel behavior as before
       wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(DefaultModuleVocab.Vocab_Cancel));
       wfo.MenuItemsProperties.ReturnValueIfNoOptionsSelected = DefaultModuleVocab.Vocab_Cancel.IdentificationKey;	   
   ```
4. A number of methods that are not used have been removed from the XamarinPageView class.  The removed method names
   are ClearPrimaryToolbarItems, ClearOverflowMenu, AddOverflowMenuOption, AddToolbarItem, AddToolbarItems, and
   ReplaceToolbarItem.
5. ReadyPrompt has been modfied to change how the main vocab word, and button, are modified if needed. 
   * Before - these where both overridden by setting a single string in the WorkFlowObject.ExtraData["ReadyButtonText"] property. 
     This only allowed setting a single string for both vocab and button text, and did not allow for setting a phonetic value 
	 if needed. Also this ran the risk of setting a string value that did not match the vocabulary defintion, or if 
	 vocabulary defintion changed it would be easy to miss updating this value as well
   * Now - The WorkFlowObject now has a ReadyProperties.ReadyWord property that may be set to a VocabWordInfo defined in the 
     ModuleVocab class. This provides all the information needed in the intent for both display and voice and allows the information
     to be maintained in a single place. Although not recommended you can sent the new ReadyWord property to a new VocabWordInfo object
     passing the current ExtraData string in as constructor. 
   
   Although the response to a ReadyPrompt is not typically checked for the main button/vocab, the IdentificationKey of the new VocabWordInfo
   object is now consistently returned, regardless of whether a user spoke the word, or selected the button on the screen.
6. BooleanPrompt intents, and the WorkFlowObjectFactory helper methods for creating BooleanPrompt intents were modified in a similar
   manner to the ReadyPrompt, and now takes VocabWordInfo objects for the affirmative and negative button text and vocab words. Again 
   it is recommended that you use VocabWordInfo objects defined in your ModuleVocab class, but you can create new VocabWordInfo objects
   using the same string currently being used. 
   
   Because of the parameter changes to the WorkflowObjectFactory methods, errors will appear in your code wherever you are creating
   BooleanPrompt intents. Simply remove the Affirative and Negative button text strings. if you are using yes and no, and using the 
   definitions of the vocab from DefaultModuleVocab base class (Vocab_Yes and Vocab_No) then you do not need to set the new parameters, 
   as these are the defaults. if you defined your own Yes/No vocab, or are using other vocabulary for your intent, then you
   can set the new affirmativeVocab and negativeVocab parameters to VocabWordInfo objects that you wish to use
   
   The results from a BooleanPrompt remain unchanges and still return Affirmitive or Negative, based on button or vocab user selected,
   therefore decode methods for BooleanPrompt intents do not need updated. 
 

# What's new in DevKit 1.3.7

## Bug Fixes

1. On slower devices (including but not limited to the WT-6000) Guided Work
   could incorrectly delay the execution of some state machine transitions
   at the beginning of a workflow.  The problem was resolved by removing
   logic that (on slower devices) would wait for the confirmed workflow
   server to respond to a generally invalid network request.  The delay
   time would depend on how long it took for the request to complete or
   time out, sometimes taking as long as 2.5 minutes.

# What's new in DevKit 1.3.6

## Bug Fixes

1. An issue was resolved where, when a custom response handler causes a retry, the data stream is closed preventing the request data from
   being accessible.
2. Custom response handlers for requests aren't/can't be persisted. When persisted requests are sent after an app restart the custom handlers
   are not being used. The ability to set a universal response handler for the RESTQueue has been added and should be used instead of the
   request specific handlers when requests are being persisted. This property can be set in the workflow's RESTService implementation similar
   to the LogFormatData property.
3. An issue was resolved where calling ClearQueueAsync to clear the RESTQueue resulted in subsequent requests not being sent until the app
   was restarted.

# What's new in DevKit 1.3.5

## Features

1. Android 10 no longer provides device serial numbers to unprivileged apps. When access to the device serial number is not available, DevKit
   has been updated to use a 64-bit number (expressed as a hexadecimal string) unique to each combination of app-signing key, user, and
   device known as the ANDROID_ID (https://developer.android.com/reference/android/provider/Settings.Secure.html#ANDROID_ID) for device
   identification purposes.

# What's new in DevKit 1.3.4

## Bug Fixes

1. Three issues were resolved with Anchor Words
    - An anchor word present on a confirmation screen no longer results in a null reference exception
    - Float value intents will now create decimal points in the grammar when max whole digits are greater than 10.
    - Get value intents will now properly return the anchor word when submitting the screen via the enter button on the keyboard.
2.  Localization fix was added for float entries to allow the specification of a whole number seperator instead of a fixed seperator of "."

# What's new in DevKit 1.3.3

## Features

1. Anchor Words support added for Android/iOS devices. Anchor words are vocabulary words that can optionally be provided to float, value, or
   long value intents enabling an operator to provide context to their input. For example, given a value intent requesting the weight of an
   object, anchor words would require the operator to specify "ounces" or "pounds" after the number.
2. An Application Settings feature has been added to GuidedWork to provide users access to various voice settings via configuration. Settings
   include BlueStreak and Pick Up and Go decode sensitivity mappings, disabling vocabulary words, phonetic and display substitutation
   capabilities, and the ability to adjust a speech output/input delay after TTS priority prompts.

# What's new in DevKit 1.3.2

## Features

1. TTS speed is now configurable in increments of .25 versus the previous
   increment of 1 on Android and iOS. This allows more precision in the
   TTS speed selectable by users.
2. Honeywell Bluetooth scanners are now supported when the scanner is the
   host of the Bluetooth connection. This adds support for scanners paired
   with the Honeywell Scanner Edge application.

## Bug Fixes

1. An issue was resolved that could cause dialogues to become unresponsive. This issue was more prevalent on Android 6.

# What's new in DevKit 1.3.1

## Features

1. Android Pie background execution is supported.  Prior to DevKit 1.3.1,
   if you execute a Guided Work app in the background or turn off the
   screen, it would quickly stop recognizing speech and would not recover.
   This is no longer an issue due to the addition of a Foreground Service.
   If you need to disable the built-in Foreground Service (e.g., because
   you have your own foreground service), set
   GuidedWork.Devices.Droid.GuidedWorkRunnerForegroundService.Enabled to
   false before calling
   GuidedWork.Devices.Droid.GuidedWorkMainActivity.OnCreate from your
   application's subclass of
   GuidedWork.Devices.Droid.GuidedWorkMainActivity.
2. A new virtual method called StopWorkflowAsync has been added to
   IWorkflowModel and its implementations.  Override it to clean up when a
   user switches from one workflow to another.  It won't be called in single
   workflow applications derived from DevKit because the system will never
   need to change the active workflow. It will be called in applications that
   register multiple workflows and give the user a way to switch between them
   at runtime, e.g., the default DevKit application.

## Changes

1. The construction of IWorkflowModel implementations is now deferred
   until after the home screen is first presented.  This is a small
   performance improvement.
2. The Honeywell.GuidedWork nuget no longer supports .NET Framework.  It was primarily available for
   unit test projects targeting .NET Framework.  If you add unit tests that need symbols from that
   nuget, use .NET Core as the unit test project type instead.  A side-effect of this change is
   that DevKit no longer requires .NET Framework (4.6.2) to be installed.  The .NET Core requirement
   for A700x is the only requirement for any non-Xamarin framework.
3. A background activity state in Base Picking was moved to correctly preclude pick retrieval instead of the DisplayStartWork state.
4. New intent (NoiseSampleActivity) initiates a noise sample via an intent without the additional oper prep activities like
   downloading templates.
   If trying to use this intent with a workflow that was created with an earlier version of devkit a new activity will have to be
   added to the workflow's wfa.json file:

        {
          "WfaReferenceName": "NoiseSampleCompositeWorkflowActivityWithReturnActivity",
          "Name": "{WorkflowName}ReturnFromOperPrep",
          "Actions": [
            {
              "Type": "PushReplaceNavigation",
              "Params": {
                "Destination": "{WorkflowName}ProcessingWorkflowReturnFromOperPrepActivity"
              }
            }
          ]
        }

## Breaking Changes
1. The ConfigCategoryName property from the ConfigRepository class
   has changed from protected to public.  The property has also been
   added to the IConfigRepository interface.  You will need to adjust
   any ConfigRepository classes in your app accordingly.
2. Some changes have been made in the way that config repository's
   are registered.  You will need to make these changes to workflow
   module code.  Using LAppModule as an example...

   Change the line that registers the config repository:

        Context.Container.Register<ILAppConfigRepository, LAppConfigRepository>();

   to:

        RegisterConfigRepository<ILAppConfigRepository, LAppConfigRepository>();

   Change the line that registers the workflow model singleton:

        RegisterWorkflowModelSingleton<ILAppModel, LAppModel>(Context.Container, nameof(LAppModel));

   to:

        RegisterWorkflowModelSingleton<ILAppModel, LAppModel>();

   You can then remove the lines that explicitly registered with the ConfigurationDataService in LAppModel.cs:

        ConfigurationDataService.RegisterRepository<ILAppConfigRepository>("LAppConfig");

   If you don't make the changes for your workflows, entries in your
   OnetimeStartupSettings.config file will not be applied and you will
   see messages in your log files like:
   [WARN]  ConfigurationDataService - [1] Repository LAppConfig not registered, configurations were not applied
3. WorkflowObject.MenuItemsProperties.AllowByKey has been changed to WorkflowObject.MenuItemsProperties.SelectionMethod
   which is an enum of 3 different selection types.  IndexOnly allows a user to speak the index or name of an item.
   OptionsOnly reads through the list and the user can respond "yes" or "no" to each item.  Both allows the user to
   speak the index or the name or speak the options command to have the list read one by one.

# What's new in DevKit 1.3

## Features

1. Added BasePickingModule with a starter picking workflow with choice of discrete or cluster picking and options for including verification steps.

## Changes

1. Changed all .csproj DebugType elements to 'portable' to improve build times for Xamarin.Android.
2. Fixed bug preventing iOS apps from starting on iOS 13.
3. Moved the GuidedWorkRunner.PhoneticUtil class to GuidedWork.PhoneticUtil.
4. ResponseExpressions are passed as hint to the speech recognizer.  See LAppStateEncoder for an example.
5. Release license feature is now available when using VoiceConsole.
6. All references to "Welcome" screen changed to "Home".  This includes views, controllers, wfa text etc.
7. Automatic release license on home screen feature.
8. New intent (LeaveGuidedWork) to go back to the home screen from a workflow.
9. Preview support for running the VoiceLink workflow on A700x has been added.  You can use this approach in your own modules to make size-optimized VADs.
10. Preview support for running the LApp workflow on A700x has been added.  Note minor changes to the LAppModule that permit it to run on
    A700x (an extra constructor override accepting different argument).  You can use this approach to add A700x support in your own modules,
    without changing much code.  Note that size will not be optimal, because your VAD will include unused Android dependencies.
11. Preview support for generating VAD files has been added to Visual Studio.  For a given Artisan application project (VoiceLink.Artisan,
    LApp.Artisan) right click on the project file in Visual Studio's Solution Explorer and select "Publish". Make sure that the "vad.
    linux-arm64.pubxml" file is selected in the UI and press the "Publish" button.  The resulting VAD file will be generated in the project's
    "bin\$(Configuration)\Artisan\" directory and can be deployed to Voice Console like any other VAD.  The VAD can be loaded onto any A700x
    device with Voice Catalyst 4.1.3 installed.
12. Preview support for running VoiceLink, LApp, and BasePicking workflows in "Mock Catalyst" has been added, allowing you to test/debug your
    A700x business logic directly from Visual Studio without deploying a VAD file to A700x.  If you make your artisan project the "default
    startup" project, select the dropdown button on the green "play" button to change from the default Development profile, and select
    "MockCatalyst".  This will run MockCatalyst using the settings in the "debug.json" file in your project.
13. The `CoreAppServerCommStateMachine` now takes an optional parameter `ReturnToStateOnBackButtonPress` during initialization.  Users can
    provided a `CoreAppSMState` to specify which state the application returns to in the event the back button is pressed while at the
    `DisplayCommMessage` state.
14. Fixed bug where DevKit users must add an explicit nuget dependency to Android/iOS apps in order to build, even if the app does not
    explicitly use VoiceConsole.  Note that the Guided Work Android/iOS framework implicitly depends on VoiceConsoleComm, so the dependency
    has been added to the Android/iOS dependency list for the Honeywell.GuidedWork.Devices nuget.

## Breaking Changes
1. All REST and TCP Socket Service implementations will now need to pass in an `ITimeoutHandler` to the base class.
    1.  The default `TimeoutHandler` implementation allows a user to request a cancellation token with a timeout duration for which the
        request will be cancelled after the specified time span (in seconds). The default time span is 90 seconds.
2. Fixed typo: `UIEelement`. Changed all instances to `UIElement`.
3. Moved Android initialization of Xamarin.Forms and Acr.UserDialogs into Honeywell.GuidedWork nugets.
   Android Applications must not call Xamarin.Forms.Init or Acr.UserDialogs.Init any more.
4. Xamarin.Forms has been updated.  Any projects with explicit Xamarin.Forms references need
   to update to tme version referenced in the example module projects (e.g. VoiceLink, LApp).
   At the moment that is version 4.3.0.991211.
5. Honeywell.Firebird.CoreLibrary.IDeviceInfo changed to Honeywell.Firebird.CoreLibrary.IUnstructuredDeviceInfo
   to avoid confusion with GuidedWork.IDeviceInfo.
6. IPortablePath and PortablePath have been removed.  Use System.IO.Path instead.
7. The GetDigits intent has been removed and converted into three separate intents (GetValue, GetFloat, and GetLongValue).
    1. All accompanying helper methods like intent encoders have been renamed/added.
    2. The default character set has changed from alpha-numeric to numeric only.
    3. Each intent has its own subset of properties in the WorkflowObject (ValueProperties, FloatProperties, LongValueProperties).
    4. The AllowAlphas property in AdditionalProperties object should no longer be used for configuring these intents.
8. Changed all `GuidedWork.GuidedWorkResources.GetResourceByType` references to `GuidedWork.EmbeddedResourceUtil.GetResourceByType`.
9. Remove the unused IFileExtensions symbol and all derived types.
10. IFirebirdAppBootstrap has changed its name to IAppBootstrap.
11. The VoiceLink sample worklflow module has been split into two parts, and a third part has been added to support A700x.  The main module
    (VoiceLinkModule) has had its Android/iOS GuidedWorkRunner code moved to another project (VoiceLinkGWRunnerModule).  The code that
    remains in the VoiceLinkModule is the platform-independent state machine portion of the code.  In addition to the Android/iOS module, the
    VoiceLinkModule is referenced in the modules that works on A700x (VoiceLinkArtisanModule).  This split results in a smaller A700x
    application (VAD file) size, due to the removal of unneeded Android dependenices (like Xamarin.Forms).
12. Workflow modules like VoiceLink and LApp that support A700x can no longer use the TranslateExtension class.  Use the Translsate class instead.
13. Applications that need VoiceConsoleComm code must explicitly reference the Honeywell.GuidedWork.VoiceConsoleComm nuget. The
    VoiceLinkGWRunnerModule and the GWS.Android/iOS apps provided in this version of DevKit do so as an example.
14. The Nuance recognizer module is no longer supported. Remove the line of code that adds the Nuance module in the SplashScreen.cs file.
    Also the NuGet Honeywell.GuidedWork.Vocon is no longer provided. Remove any references to the NuGet from projects.
15. Any class extending IViewController (Generally Setting screen controllers) that implement IDispose interface will need to make sure to
    change dispose methods to override base class's methods, and call the base class dispose methods
# What's new in DevKit 1.2

## Bug Fixes
1. VoiceLink "Log Off" option no longer causes the app to crash
2. Improved speaker independent voice recognition stability
3. Now returns the appropriate error message if the app fails to retrieve data from the server post-login
4. All overflow menu items have been made lowercase
5. Changing focus via a long press on the keyboard no longer crashes the app
6. Handle unrecognized language/country codes in Android
7. Honeywell logo no longer disappears from the navigation bar
8. Disabled Voice during the "Sign On" step
9. Fixed various warnings
10. Fixed app crash where after making an unsuccessful login attempt, pressing "back" would crash the app
11. Ready intent with UI elements now draws the UI elements
12. Whitespace surrounding the `UserID` is now trimmed before sending it over to the server
13. User settings will now always get loaded in on application startup
    1. The `ConfigRepositories` have had their creation moved from the `DataServiceInitializer` to the `WelcomeController`
14. Tightened space between header and body for ready intents
15. SRX behavior changed while the camera is open
    1. User can no longer speak "no more" if they are in either the camera or the gallery
    2. Raising the mic no longer puts the app on standby if either the camera or gallery is open
16. The UserId field on the Sign in intent has been made nullable
17. Improved stability and performance optimizations
18. Visual elements now have automation IDs

## Features
1. Added extra configurability to intents
    1. The `GetDigitsIntent` now has the ability to enforce a max an min value
    2. The `GuidedWorkViewModel` has been updated to add extra functionality to parse response such as an `IsResponseValidHandler` as well as internal screens for confirmation and validity checking.
    3. New parameters to tweak via the `AdditionalPropertiesObject`: `ConfirmVoiceInput`, `ConfirmScreenInput`, `ConfirmOverflowItem`, `MinimumValue`, `MaximumValue`, `ExpectedSpokenValues`, `ExpectedScannedValues`
    4. Get characters is implemented via the `CreateGetDigitsIntent` by setting the `AdditionalPropertiesObject.MinimumValue = AdditionalPropertiesObject.MinimumValue = null`
2. Added multiple selection menu intents
3. The `OrderPickingModule` now has quantity confirmation
4. New `LAppModule` module
    1. Examples of Multiple Select Menu items
    2. Examples of character entries (via `CreateGetDigitsIntent`)
    3. Examples of photo entry via the `CreatePhotoCaptureIntent`
    4. Examples of `CreateReadyUIElementIntent` that make use of new `UIElements` that can draw images on screen
    5. Examples of numeric entry via `CreateGetDigitsIntent`
5. Confirmation prompts' formatting is now handled in the view file instead of the controller
6. New `UIElement` type, `Image`, which holds the byte data of an image file and draws the corresponding image on-screen
    1. The user can now tap to zoom on the `Image UIElement` as well as images in the `CreatePhotoCaptureIntent`
7. Add the ability to modify the http timeout to something less than 30 seconds
8. Additional REST service functionality
    1. Added a REST function to support Multipart form data
9. Introduced new `GenericServerCommStateMachine` to handle sending and receiving requests to the server without copying lots of boiler plate code
10. Bind the customer name to application setting view
11. New prompt animations enabled via the `ViewModel` with a setting in Voice and Audio Settings to toggle it for all screens
12. Implemented the "skip aisle" command in `PickAssignmentStateMachine`
13. Added new `PhotoIntent` with example in `LAppModule`
14. Implemented the "repeat last pick" command in the `Selection` workflow
15. New `DateEntryIntent`
16. Implemented `PassAssignment` feature
17. New app icons
18. A new progress bar during assignments, can be configured via the `AdditionalPropertiesObject`
19. Add style option to `UIElements` and allow users to override a default style
20. Add new utility class `FloatEqualityUtil` to provide equality helper methods for decimal values

## Changes
1. Module specific vocabulary has been moved out from the `GuidedWork` and into their respective modules
2. Welcome screen tweaked
    1. The default background has been changed to the Guided Work Logo
    2. Changed `stacklayout` to a grid where the spacing between items is proportionate to the size of the screen
    3. Welcome screen background image added
3. Update Honeywell branding

## Breaking Changes
1. All API calls to `TranslateExtension` need to be replaced with calls to the new `Translate` API except for the `TranslateExtension.GetLocalizedTextForBaseKey`
    1. Easiest way is to "find and replace a string in multiple files" (ctrl + shift + h on visual studio) `TranslateExtension` ->
      `Translate`, then `Translate.GetLocalizedTextForBaseKey` -> `TranslateExtension.GetLocalizedTextForBaseKey` for all file types
2. Uninstall `Honeywell.Firebird.CoreLibrary.NuanceVocon` from `GWS.Android` project and install `Honeywell.GuidedWork.NuanceVocon`
3. Update NuGets
    1. Replace all NuGet package files with new NuGet package files
    2. Update all Honeywell NuGets
    3. Update `Xamarin.Android.Support` NuGets to the ones listed in the new GWS.Android.csproj file.
4. Update XAMLs
    1. In View XAMLs change `CustomFontPicker` to `CustomPicker`
    2. In View XAMLs change `xmlns:hfmcl` header assembly from `Honeywell.Firebird.CoreLibrary to Honeywell.GuidedWork.Base`
    3. In View XAMLs change `xmlns:i18n` header assembly from `Honeywell.Firebird.CoreLibrary.Localization to Honeywell.GuidedWork.Base`
5. Use the `availableOperators` argument for the `CreateSignInIntent` method rather than adding available operators to the `WorkflowObject.Items` property
6. Change all `GuidedWorkResources` static references to `LocalizationHelper` static references
7. Change references to `AdditionalOverflowMenuOptions` to be Lists of `InteractiveItem` objects (call single argument `InteractiveItem` constructor)
8. Change references of `WorkflowObject.UseMenuItemIndex` to `WorkflowObject.MenuItemsProperties.AllowByKey`
9. Change references of `WorkflowObject.AdditionalProperties.IncludeCancelButton` to `WorkflowObject.MenuItemsProperties.AllowCancel`
10. It is no longer possible to compile for armeabi, and you must only compile for armeabi-v7a. You may run the resulting app on armeabi-v7a and arm64-v8a devices
11. Use `Honeywell.Firebird.CoreLibrary.VocabWordInfo` object instead of a string when defining module vocab
    1. This allows there to be separation between the spelling of a word and the pronunciation in cases where one might want to tweak the phonetics of text on screen
    2. Now `DefaultModuleVocab.Numerics`, `DefaultModuleVocab.Alphas`, `DefaultModuleVocab.BaseRequiredVocab`, and `DefaultModuleVocab.Common` now return a `VocabWordInfo` array instead of a string array
    3. When creating IModuleVocab.PlatformIndependentVocab, IModuleVocab.AndroidVocab, and IModuleVocab.IosVocab, use VocabWordInfo arrays instead of string arrays
12. Combine `CreateMenuItems` and `CreateListSelection` intents into the `MenuIntent`
13. Removed `VoiceLink_Aisle_Button_Ready` and `VoiceLink_Aisle_Button_SkipAisle` from the vocab list.
14. In the XAML view cs files `style` should now be a static reference to `Application.Current.Resources[styleKey]` or else styles may not be saved
15. The `GuidedWork.SharedDependencies.Register<AppBootstrap, GuidedWork.InitialWfaRegistrant, GWS.App>(container);` syntax no longer exists
    1. Register the `AppBootstrap`, `GuidedWork.InitialWfaRegistrant`, and `GWS.App` as part of the `TinyIoCContainer` object
    2. Change `GuidedWork.SharedDependencies.Register<AppBootstrap, GuidedWork.InitialWfaRegistrant, GWS.App>(container);` -> `GuidedWork.SharedDependencies.Register(container);`
    3. Add `container.Register<FormsApp, GWS.App>(); container.Register<IFirebirdAppBootstrap, AppBootstrap>(); container.Register<IWorkflowActivityRegistrant, InitialWfaRegistrant>(); container.Register<IAppWorkflowActivityRegistrant, AppWfaRegistrant>();`
    4. Add `using Honeywell.Firebird.Module.XamarinForms;` and `using Honeywell.Firebird;` if not already doing so
16. The minimum Android SDK version for GWS.Android is now set to 23, which requires Android 6.0 or higher
