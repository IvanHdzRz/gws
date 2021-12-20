//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace SimpleApp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Logging;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using TinyIoC;

    public class SimpleAppModuleVocab : DefaultModuleVocab
    {
        public override VocabWordInfo[] PlatformIndependentVocab { get; } =
            BaseRequiredVocab
                .Concat(Numerics).ToArray();
    }

    public interface ISimpleAppModel : IGenericIntentModel<ISimpleAppModel>
    {
        string EnteredName { get; set; }

        string PhoneNumber { get; set; }
        
        IList<IndexedListCellViewModel> MenuResult { get; set; }

        bool? BooleanResult { get; set; }

        DateTime DateResult { get; set; }

        List<Uri> PhotoResult { get; set; }

        (string, bool) AudioResult { get; set; }
    }

    public class SimpleAppModel : SimplifiedIntentModel<SimpleAppBusinessLogic, ISimpleAppModel>, ISimpleAppModel
    {
        // This method can be overridden if additional logic is required to determine overflow menu items for
        // states.  If the list stays the same for a state then the list can be specified during the configuration
        // of the state.
        public override List<InteractiveItem> AllowedAdditionalVocab()
        {
            var result = new List<InteractiveItem>();

            if (CurrentState == SimpleAppBusinessLogic.DisplayShowNameAndNumber)
            {
                result.Add(new InteractiveItem("test_overflow_key", Translate.GetLocalizedTextForKey("test_overflow_key"), processActionAsync: ProcessTestOverflowAsync, confirm: true));
            }

            return result;
        }

        private Task<CoreAppSMState> ProcessTestOverflowAsync()
        {
            return Task.FromResult(SimpleAppBusinessLogic.DisplayGetName);
        }

        public string EnteredName { get; set; }

        public string PhoneNumber { get; set; }
        
        public IList<IndexedListCellViewModel> MenuResult { get; set; }

        public bool? BooleanResult { get; set; }

        public DateTime DateResult { get; set; }

        public List<Uri> PhotoResult { get; set; }

        public (string, bool) AudioResult { get; set; }
    }

    public class SimpleAppBusinessLogic : SimplifiedBaseBusinessLogic<ISimpleAppModel, SimpleAppBusinessLogic, ISimpleAppConfigRepository>
    {
        public static readonly CoreAppSMState StartOperLogin = new CoreAppSMState(nameof(StartOperLogin));
        public static readonly CoreAppSMState DisplayGetName = new CoreAppSMState(nameof(DisplayGetName));
        public static readonly CoreAppSMState ExecuteProcessName = new CoreAppSMState(nameof(ExecuteProcessName));
        public static readonly CoreAppSMState DisplayShowNameAndNumber = new CoreAppSMState(nameof(DisplayShowNameAndNumber));

        public static readonly CoreAppSMState DisplayMenu = new CoreAppSMState(nameof(DisplayMenu));
        public static readonly CoreAppSMState ExecuteMenu = new CoreAppSMState(nameof(ExecuteMenu));

        public static readonly CoreAppSMState StartBoolean = new CoreAppSMState(nameof(StartBoolean));
        public static readonly CoreAppSMState StartDate = new CoreAppSMState(nameof(StartDate));
        public static readonly CoreAppSMState StartPhoto = new CoreAppSMState(nameof(StartPhoto));
        public static readonly CoreAppSMState StartReady = new CoreAppSMState(nameof(StartReady));
        public static readonly CoreAppSMState StartAudio = new CoreAppSMState(nameof(StartAudio));

        // Boolean States
        public static readonly CoreAppSMState DisplayGetBoolean = new CoreAppSMState(nameof(DisplayGetBoolean));
        public static readonly CoreAppSMState ExecuteGetBoolean = new CoreAppSMState(nameof(ExecuteGetBoolean));

        // Date Entry States
        public static readonly CoreAppSMState DisplayGetDate = new CoreAppSMState(nameof(DisplayGetDate));
        public static readonly CoreAppSMState ExecuteGetDate = new CoreAppSMState(nameof(ExecuteGetDate));

        // Photo Entry States
        public static readonly CoreAppSMState DisplayGetPhoto = new CoreAppSMState(nameof(DisplayGetPhoto));
        public static readonly CoreAppSMState ExecuteGetPhoto = new CoreAppSMState(nameof(ExecuteGetPhoto));

        // Ready Entry States
        public static readonly CoreAppSMState DisplayGetReady = new CoreAppSMState(nameof(DisplayGetReady));
        public static readonly CoreAppSMState ExecuteGetReady = new CoreAppSMState(nameof(ExecuteGetReady));

        // Audio Entry States
        public static readonly CoreAppSMState DisplayGetAudio = new CoreAppSMState(nameof(DisplayGetAudio));
        public static readonly CoreAppSMState ExecuteGetAudio = new CoreAppSMState(nameof(ExecuteGetAudio));

        private readonly ISimpleAppRESTServiceProvider _ServiceProvider;

        private readonly ILog _Log = LogManager.GetLogger(nameof(SimpleAppBusinessLogic));

        public override bool IsPrimaryStateMachine => true;

        private SimpleAppSecondStateMachine _secondSM;
        private SimpleAppSecondStateMachine SecondSM { get { return Manager.CreateStateMachine(ref _secondSM); } }

        public SimpleAppBusinessLogic(SimplifiedStateMachineManager<SimpleAppBusinessLogic, ISimpleAppModel> manager, ISimpleAppModel model) : base(manager, model)
        {
            _ServiceProvider = TinyIoCContainer.Current.Resolve<ISimpleAppRESTServiceProvider>();
        }

        public override void ConfigureStates()
        {
            ConfigureLogin(StartOperLogin, 
                           DisplayGetName,
                           SimpleAppConfigRepository.OPERATOR_ID,
                           ExecuteLoginAsync);
            

            ConfigureDisplayState(DisplayGetName, 
                                 ExecuteProcessName, 
                                 followedByBackgroundActivity: true,
                                 backgroundActivityHeaderKey: "SimpleApp_Background_Header_Processing_Name", 
                                 encodeAction: EncodeDisplayGetName, 
                                 decodeAction: DecodeDisplayGetName,
                                 backButtonDestinationState: StartOperLogin);

            ConfigureLogicState(ExecuteProcessName,
                                ProcessNameAsync,
                                DisplayShowNameAndNumber);

            ConfigureDisplayState(DisplayShowNameAndNumber, 
                                  DisplayMenu, 
                                  encodeAction: EncodeDisplayShowNameAndNumber, 
                                  backButtonDestinationState: DisplayGetName);

            ConfigureDisplayState(DisplayMenu, 
                                  ExecuteMenu, 
                                  encodeAction: EncodeDisplayMenu, 
                                  decodeAction: DecodeDisplayMenu,
                                  backButtonDestinationState: DisplayGetName);

            ConfigureLogicState(ExecuteMenu,
                                SelectIntent,
                                DisplayGetName,
                                DisplayGetAudio,
                                DisplayGetBoolean,
                                DisplayGetDate,
                                DisplayGetPhoto,
                                DisplayGetReady);

            ConfigureDisplayState(DisplayGetBoolean, 
                                  ExecuteGetBoolean, 
                                  encodeAction: EncodeDisplayGetBoolean, 
                                  decodeAction: DecodeDisplayGetBoolean,
                                  backButtonDestinationState: DisplayMenu);

            ConfigureLogicState(ExecuteGetBoolean,
                                GetBooleanScreenStatus,
                                DisplayMenu,
                                DisplayGetBoolean);

            ConfigureDisplayState(DisplayGetPhoto, 
                                  ExecuteGetPhoto, 
                                  encodeAction: EncodeDisplayGetPhoto, 
                                  decodeAction: DecodeDisplayGetPhoto,
                                  backButtonDestinationState: DisplayMenu);

            ConfigureLogicState(ExecuteGetPhoto,
                                GetPhotoEntryStatus,
                                DisplayMenu,
                                DisplayGetPhoto);

            ConfigureDisplayState(DisplayGetDate, 
                                  ExecuteGetDate, 
                                  encodeAction: EncodeDisplayGetDate, 
                                  decodeAction: DecodeDisplayGetDate,
                                  backButtonDestinationState: DisplayMenu);

            ConfigureLogicState(ExecuteGetDate,
                                GetDateEntryStatus,
                                DisplayMenu,
                                DisplayGetDate);

            ConfigureDisplayState(DisplayGetAudio, 
                                  ExecuteGetAudio, 
                                  encodeAction: EncodeDisplayGetAudio, 
                                  decodeAction: DecodeDisplayGetAudio,
                                  backButtonDestinationState: DisplayMenu);

            ConfigureLogicState(ExecuteGetAudio,
                                GetAudioEntryStatus,
                                DisplayMenu,
                                DisplayGetAudio);

            ConfigureDisplayState(DisplayGetReady, 
                                  DisplayMenu, 
                                  encodeAction: EncodeDisplayGetReady,
                                  backButtonDestinationState: DisplayMenu);
        }

        private async Task<bool> ExecuteLoginAsync(GuidedWorkRunner.Operator newOperator)
        {
            var response = await _ServiceProvider.SignOnAsync(newOperator);
            if (response.ErrorCode == 1)
            {
                Model.CurrentUserMessage = Translate.GetLocalizedTextForKey("SimpleApp_LoginFailed_Message");
                return false;
            }
            else
            {
                return true;
            }
        }

        #region GetNameConfiguration
        private WorkflowObjectContainer EncodeDisplayGetName(ISimpleAppModel model)
        {
            /*var wfoContainer = EncodeVoiceCentricValueIntent(model, Translate.GetLocalizedTextForKey("SimpleApp_GetName_Prompt"));
            wfoContainer.WorkflowObjects[0].ValueProperties.AllowedCharacters = CharacterSet.AlphaNumeric;
            return wfoContainer;*/

            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("SimpleApp_GetName_Header", model.CurrentOperator.OperatorIdentifier),
                                                                 "getName",
                                                                 Translate.GetLocalizedTextForKey("SimpleApp_GetName_Label"),
                                                                 Translate.GetLocalizedTextForKey("SimpleApp_GetName_Prompt"),
                                                                 null,
                                                                 model.CurrentUserMessage);

            wfo.ValueProperties.AllowedCharacters = CharacterSet.AlphaNumeric;

            wfoContainer.Add(wfo);

            return wfoContainer;
        }

        private void DecodeDisplayGetName(SlotContainer slotContainer, ISimpleAppModel model)
        {
            model.EnteredName = GenericBaseEncoder<ISimpleAppModel>.DecodeValueEntry(slotContainer);
        }

        private async Task ProcessNameAsync()
        {
            Model.EnteredName = Model.EnteredName.ToUpper();

            var response = await _ServiceProvider.DoSimpleAppRequestAsync(Model.EnteredName);
            Model.EnteredName = response.ResponseData;

            SecondSM.Reset();
            await SecondSM.InitializeStateMachineAsync();

            NextState = DisplayShowNameAndNumber;
        }
        #endregion

        private WorkflowObjectContainer EncodeDisplayShowNameAndNumber(ISimpleAppModel model)
        {
            string header;
            string prompt;
            if (string.IsNullOrEmpty(model.PhoneNumber))
            {
                header = Translate.GetLocalizedTextForKey("SimpleApp_ShowNameOnly_Header", model.EnteredName);
                prompt = Translate.GetLocalizedTextForKey("SimpleApp_ShowNameOnly_Prompt", model.EnteredName);
            }
            else
            {
                header = Translate.GetLocalizedTextForKey("SimpleApp_ShowNameAndNumber_Header", model.EnteredName, model.PhoneNumber);
                prompt = Translate.GetLocalizedTextForKey("SimpleApp_ShowNameAndNumber_Prompt", model.EnteredName, model.PhoneNumber);
            }

            //return EncodeVoiceCentricReadyIntent(model, prompt);

            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateReadyIntent(header,
                                                              "getName",
                                                              prompt,
                                                              model.CurrentUserMessage);

            wfoContainer.Add(wfo);

            return wfoContainer;
        }

        #region MenuConfiguration
        private WorkflowObjectContainer EncodeDisplayMenu(ISimpleAppModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();

            var menuItems =
                new List<string>
                {
                    "SimpleApp_Boolean",
                    "SimpleApp_Date",
                    "SimpleApp_Photo",
                    "SimpleApp_Ready",
                    "SimpleApp_Audio"
                };

            var wfo = WorkflowObjectFactory.CreateMenuItemsIntent(Translate.GetLocalizedTextForKey("SimpleApp_Menu_Header"),
                                                              "getMenu",
                                                              Translate.GetLocalizedTextForKey("SimpleApp_Menu_Header"),
                                                              (from item in menuItems
                                                               select Translate.GetLocalizedTextForKey(item)).ToList(),
                                                              model.CurrentUserMessage, 
                                                              initialPrompt: model.CurrentUserMessage);

            wfo.MenuItemsProperties.MaxSelection = 1;
            wfo.MenuItemsProperties.DisplayKeys = true;
            wfo.MenuItemsProperties.SelectionMethod = MenuSelectionTypes.OptionsOnly;
            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(DefaultModuleVocab.VocabCancel));
            wfo.MenuItemsProperties.ReturnValueIfNoOptionsSelected = DefaultModuleVocab.VocabCancel.IdentificationKey;

            wfoContainer.Add(wfo);

            return wfoContainer;
        }

        private void DecodeDisplayMenu(SlotContainer slotContainer, ISimpleAppModel model)
        {
            model.MenuResult = GenericBaseEncoder<ISimpleAppModel>.DecodeMenuItems(slotContainer, DefaultModuleVocab.VocabCancel.IdentificationKey).Item1;
        }

        private void SelectIntent()
        {
            if (Model.MenuResult == null)
            {
                NextState = DisplayGetName;
                return;
            }

            var intent = (from item in Model.MenuResult
                          where item.Selected
                          select item).First();

            if (intent.DisplayName == Translate.GetLocalizedTextForKey("SimpleApp_Boolean"))
            {
                NextState = DisplayGetBoolean;
            }
            else if (intent.DisplayName == Translate.GetLocalizedTextForKey("SimpleApp_Date"))
            {
                NextState = DisplayGetDate;
            }
            else if (intent.DisplayName == Translate.GetLocalizedTextForKey("SimpleApp_Photo"))
            {
                NextState = DisplayGetPhoto;
            }
            else if (intent.DisplayName == Translate.GetLocalizedTextForKey("SimpleApp_Ready"))
            {
                NextState = DisplayGetReady;
            }
            else if (intent.DisplayName == Translate.GetLocalizedTextForKey("SimpleApp_Audio"))
            {
                NextState = DisplayGetAudio;
            }
        }
        #endregion

        #region BOOLEAN_ENTRY_CONFIGURATION
        private WorkflowObjectContainer EncodeDisplayGetBoolean(ISimpleAppModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateBooleanIntent(Translate.GetLocalizedTextForKey("SimpleApp_Boolean_Header_Message"),
                                                                "getBool",
                                                                Translate.GetLocalizedTextForKey("SimpleApp_Boolean_Header_Message"),
                                                                model.CurrentUserMessage,
                                                                affirmativeVocab: new VocabWordInfo("SimpleApp_Boolean_True_Display"),
                                                                negativeVocab: new VocabWordInfo("SimpleApp_Boolean_False_Display"));

            wfoContainer.Add(wfo);

            return wfoContainer;
        }

        private void DecodeDisplayGetBoolean(SlotContainer slotContainer, ISimpleAppModel model)
        {
            model.BooleanResult = GenericBaseEncoder<ISimpleAppModel>.DecodeBooleanPrompt(slotContainer);
        }

        private void GetBooleanScreenStatus()
        {
            NextState = DisplayMenu;
            if (Model.BooleanResult == null)
            {
                CurrentUserMessage = Translate.GetLocalizedTextForKey("SimpleApp_Boolean_InputFailed_Message");
                MessageType = UserMessageType.Error;
                NextState = DisplayGetBoolean;
            }
        }
        #endregion

        #region DATE_ENTRY_CONFIGURATION
        private WorkflowObjectContainer EncodeDisplayGetDate(ISimpleAppModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateDateEntryIntent(Translate.GetLocalizedTextForKey("SimpleApp_Date_Header_Message"),
                                                                "getDate",
                                                                Translate.GetLocalizedTextForKey("SimpleApp_Date_Header_Message"),
                                                                Translate.GetLocalizedTextForKey("SimpleApp_Date_Header_Message"),
                                                                model.CurrentUserMessage);

            wfo.DateProperties.MaximumDate = new DateTime(2020, 12, 31);
            wfo.DateProperties.MinimumDate = new DateTime(2000, 12, 31);
            wfo.DateProperties.PreSetDateValue = DateTime.Now;

            wfoContainer.Add(wfo);

            return wfoContainer;
        }

        private void DecodeDisplayGetDate(SlotContainer slotContainer, ISimpleAppModel model)
        {
            model.DateResult = GenericBaseEncoder<ISimpleAppModel>.DecodeDateEntry(slotContainer);
        }

        private void GetDateEntryStatus()
        {
            NextState = DisplayMenu;
            if (Model.DateResult == null)
            {
                CurrentUserMessage = Translate.GetLocalizedTextForKey("SimpleApp_Date_InputFailed_Message");
                MessageType = UserMessageType.Error;
                NextState = DisplayGetDate;
            }
        }
        #endregion

        #region PHOTO_ENTRY_CONFIGURATION
        private WorkflowObjectContainer EncodeDisplayGetPhoto(ISimpleAppModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreatePhotoCaptureIntent(Translate.GetLocalizedTextForKey("SimpleApp_Photo_Header_Message"),
                                                                    "getPhoto",
                                                                    Translate.GetLocalizedTextForKey("SimpleApp_Photo_Header_Message"),
                                                                    model.CurrentUserMessage);

            wfo.PhotoCaptureProperties.RequirePhoto = true;

            wfoContainer.Add(wfo);

            return wfoContainer;
        }

        private void DecodeDisplayGetPhoto(SlotContainer slotContainer, ISimpleAppModel model)
        {
            //Get Photo not supported on all platforms, check if intent was supported before setting result
            if (!model.UnsupportedIntents.Contains(WorkflowIntent.PhotoCapturePrompt))
            {
                model.PhotoResult = GenericBaseEncoder<ISimpleAppModel>.DecodePhotoEntry(slotContainer);
            }
        }

        private void GetPhotoEntryStatus()
        {
            NextState = DisplayMenu;
            //First check if intent was supported, if not set unsupported message and return to display menu
            if (Model.UnsupportedIntents.Contains(WorkflowIntent.PhotoCapturePrompt))
            {
                CurrentUserMessage = Translate.GetLocalizedTextForKey("SimpleApp_Intent_Not_Supported");
                MessageType = UserMessageType.Error;
            }
            else if (Model.PhotoResult == null)
            {
                CurrentUserMessage = Translate.GetLocalizedTextForKey("SimpleApp_Photo_InputFailed_Message");
                MessageType = UserMessageType.Error;
                NextState = DisplayGetPhoto;
            }
        }
        #endregion

        #region AUDIO_ENTRY_CONFIGURATION
        private WorkflowObjectContainer EncodeDisplayGetAudio(ISimpleAppModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateRecordMemoIntent(Translate.GetLocalizedTextForKey("SimpleApp_Audio_Header_Message"),
                                                                    "getAudio",
                                                                    Translate.GetLocalizedTextForKey("SimpleApp_Audio_Header_Message"),
                                                                    model.CurrentUserMessage);
            wfoContainer.Add(wfo);

            return wfoContainer;
        }

        private void DecodeDisplayGetAudio(SlotContainer slotContainer, ISimpleAppModel model)
        {
            //Record memo not supported on all platforms, check if intent was supported before setting result
            if (!model.UnsupportedIntents.Contains(WorkflowIntent.RecordMemo))
            {
                model.AudioResult = GenericBaseEncoder<ISimpleAppModel>.DecodeAudioCapture(slotContainer);
            }
        }

        private void GetAudioEntryStatus()
        {
            NextState = DisplayMenu;
            //First check if intent was supported, if not set unsupported message and return to display menu
            if (Model.UnsupportedIntents.Contains(WorkflowIntent.RecordMemo))
            {
                CurrentUserMessage = Translate.GetLocalizedTextForKey("SimpleApp_Intent_Not_Supported");
                MessageType = UserMessageType.Error;
            }
            else if (Model.AudioResult.Item1 == null)
            {
                CurrentUserMessage = Translate.GetLocalizedTextForKey("SimpleApp_Audio_InputFailed_Message");
                MessageType = UserMessageType.Error;
                NextState = DisplayGetAudio;
            }
        }
        #endregion

        #region READY_ENTRY_CONFIGURATION
        public WorkflowObjectContainer EncodeDisplayGetReady(ISimpleAppModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("SimpleApp_Ready_Header_Message"),
                                                            "getReady",
                                                            Translate.GetLocalizedTextForKey("SimpleApp_Ready_Header_Message"),
                                                            model.CurrentUserMessage);

            wfoContainer.Add(wfo);

            return wfoContainer;
        }
        #endregion
    }
}
