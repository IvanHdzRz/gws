//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using Common.Logging;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TinyIoC;

    using Operator = GuidedWorkRunner.Operator;

    public class BasePickingModuleVocab : DefaultModuleVocab
    {
        /// <summary>
        /// Gets the default platform independent vocabulary localization keys.
        /// It is constructed from the concatenation of
        /// <see cref="P:GuidedWork.DefaultModuleVocab.Numerics"/>, and
        /// workflow-specific keys.
        /// </summary>
        /// <value>The platform independent vocab.</value>
        public override VocabWordInfo[] PlatformIndependentVocab { get; } =
            new List<VocabWordInfo>(Numerics)
                .Concat(BaseRequiredVocab)
                .Concat(new []
                {
                    VocabYes,
                    VocabNo
                })
                .ToArray();

        //Application specific vocabulary, defined as static fields for easier use/reference throughout application
        public static readonly VocabWordInfo ShortProduct = new VocabWordInfo("BasePicking_VocabWord_ShortProduct");
        public static readonly VocabWordInfo SkipSlot = new VocabWordInfo("BasePicking_VocabWord_SkipSlot");
        public static readonly VocabWordInfo SignOff = new VocabWordInfo("BasePicking_Vocab_Additional_Cmd_SignOff");
        public static readonly VocabWordInfo SayAgain = new VocabWordInfo("VocabWord_SayAgain");
    }

    /// <summary>
    /// The model of the BasePicking Workflow.  The interface abstracts the retrieval of data from the 
    /// data service.  The model serves as temporary storage of information between controllers associated 
    /// with simple WFAs.
    /// </summary>
    public interface IBasePickingModel : IGenericIntentModel<IBasePickingModel>
    {
        IBasePickingDataService DataService { get; set; }
        IConfigurationDataService ConfigurationDataService { get; set; }

        void ConfigureProgressBar(WorkflowObject wfo);

        List<UIElement> GenerateUIElements(Pick pick,
                                           bool showAisle = false,
                                           bool showSlot = false,
                                           bool showProductId = false,
                                           bool showProductName = false,
                                           bool showQuantity = false);

        bool ButtonResponseIsSkipSlot();

        //Main Properties
        Operator CurrentSignedInOperator { get; set; }
        bool SignOffAllowed { get; }

        bool FirstAssignment { get; set; }
        List<Pick> Picks { get; set; }
        Pick CurrentPick { get; set; }

        int CompletedPicks { get; }
        int TotalPicks { get; }

        string LocationCheckDigitResponse { get; set; }
        string ProductBatchNumberResponse { get; set; }
        string EnteredPickQuantityString { get; set; }
        int EnteredPickQuantity { get; set; }
        bool SlotSkippedFromQuantity { get; set; }
        bool EnteredConfirmShortProductResponse { get; set; }
        string EnteredContainerVerification { get; set; }

        string ButtonResponse { get; set; }

        void ResetOperator();
    }

    /// <summary>
    /// The model of the BasePicking workflow.
    /// </summary>
    public class BasePickingModel : SimplifiedIntentModel<BasePickingStateMachine, IBasePickingModel>, IBasePickingModel
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(BasePickingStateMachine));

        public IBasePickingDataService DataService { get; set; }
        public IConfigurationDataService ConfigurationDataService { get; set; }

        //Main Properties    
        public Operator CurrentSignedInOperator { get; set; }
        public bool SignOffAllowed => CurrentSignedInOperator != null;

        public bool FirstAssignment { get; set; } = true;

        public List<Pick> Picks { get; set; }
        public Pick CurrentPick { get; set; }

        public int CompletedPicks => Picks.Where(p => p.Picked).Count();
        public int TotalPicks => Picks.Count();

        public string LocationCheckDigitResponse { get; set; }
        public string ProductBatchNumberResponse { get; set; }
        public string EnteredPickQuantityString { get; set; }
        public int EnteredPickQuantity { get; set; }
        public bool SlotSkippedFromQuantity { get; set; }
        public bool EnteredConfirmShortProductResponse { get; set; }
        public string EnteredContainerVerification { get; set; }

        public string ButtonResponse { get; set; }

        public void ResetOperator()
        {
            CurrentSignedInOperator = null;
            CurrentPick = null;
            FirstAssignment = true;
        }

        public override List<InteractiveItem> AllowedAdditionalVocab()
        {
            var result = new List<InteractiveItem>();

            // Sign off available whenever operator signed in
            if (SignOffAllowed)
            {
                result.Add(new InteractiveItem(BasePickingModuleVocab.SignOff, 
                    itemValidationDelegate: ValidateSignOffAsync, processActionAsync: ProcessSignOffAsync, confirm: true));
            }

            return result;
        }

        private string ValidateSignOffAsync()
        {
            if (!SignOffAllowed)
            {
                return Translate.GetLocalizedTextForKey("BasePicking_Signoff_Not_Allowed_Prompt");
            }
            return null;
        }

        private async Task<CoreAppSMState> ProcessSignOffAsync()
        {
            CurrentUserMessage = null;
            try
            {
                await DataService.SignOffAsync();
                ResetOperator();
                return BasePickingStateMachine.BasePickingStart;
            }
            // TODO: handle other exceptions or errors depending on communication mechanism
            catch (OperationCanceledException)
            {
                _Log.Debug("Operation canceled via communication Timeout Handler");
                CurrentUserMessage = Translate.GetLocalizedTextForKey("BasePicking_NetworkTimeout_Error");
            }

            return null;
        }

        public override async Task ExecuteBackButtonPressAsync(BackButtonEventArgs backButtonEventArgs)
        {
            await base.ExecuteBackButtonPressAsync(backButtonEventArgs);

            if (!backButtonEventArgs.BackButtonHandled && SignOffAllowed)
            {
                AdditionalVocabSpoken = AllowedAdditionalVocab().First((av) => av.Key == BasePickingModuleVocab.SignOff.IdentificationKey);
                AdditionalVocabSpokenState = CurrentState;
                await ExecuteStateAsync();
            }
        }

        /// <summary>
        /// Initializes the workflow asynchronously.
        /// </summary>
        /// <returns>The workflow async.</returns>
        public override async Task InitializeWorkflowAsync()
        {
            DataService = TinyIoCContainer.Current.Resolve<IBasePickingDataService>();
            DataService.Initialize();

            //Load BasePicking settings from file if it exists. 
            //Delete after loading because it may contain secure information.
            ConfigurationDataService = TinyIoCContainer.Current.Resolve<IConfigurationDataService>();
            ConfigurationDataService.ApplyConfigurationFromFile("BasePickingSettings.config", true);

            await base.InitializeWorkflowAsync();
        }

        /// <summary>
        /// Configures progress bar properties for WorkflowObject to display pick list progress
        /// </summary>
        /// <param name="wfo"></param>
        /// <param name="model"></param>
        public virtual void ConfigureProgressBar(WorkflowObject wfo)
        {
            wfo.ProgressBarProperties.isVisible = true;
            int count = TotalPicks;
            int completed = CompletedPicks;
            wfo.ProgressBarProperties.TotalStepCount = count;
            wfo.ProgressBarProperties.CurrentStepCount = completed;
            wfo.ProgressBarProperties.TitleLabelText = Translate.GetLocalizedTextForKey("BasePicking_ProgressBar_TitleLabel");
            wfo.ProgressBarProperties.ProgressLabelText = completed + "/" + count;
        }

        /// <summary>
        /// Generates UIElements to display values
        /// </summary>
        /// <param name="pick">the Pick to display info about</param>
        /// <param name="showAisle"></param>
        /// <param name="showSlot"></param>
        /// <param name="showProductId"></param>
        /// <param name="showProductName"></param>
        /// <param name="showQuantity"></param>
        /// <returns></returns>
        public virtual List<UIElement> GenerateUIElements(Pick pick,
                                                          bool showAisle = false,
                                                          bool showSlot = false,
                                                          bool showProductId = false,
                                                          bool showProductName = false,
                                                          bool showQuantity = false)
        {

            var uiElements = new List<UIElement>();
            if (showAisle || showSlot)
            {
                uiElements.Add(new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("BasePicking_Location_Banner_Label")
                });
                if (showAisle)
                {
                    uiElements.Add(new UIElement
                    {
                        ElementType = UIElementType.Detail,
                        Label = pick.Aisle,
                        LabelInfo = Translate.GetLocalizedTextForKey("BasePicking_Aisle_Label"),
                        LabelInfoVertical = true,
                        InlineWithNext = true
                    });
                }
                if (showSlot)
                {
                    uiElements.Add(new UIElement
                    {
                        ElementType = UIElementType.Detail,
                        Label = pick.Slot,
                        LabelInfo = Translate.GetLocalizedTextForKey("BasePicking_Slot_Label"),
                        LabelInfoVertical = true
                    });
                }
            }

            if (showProductId || showProductName || showQuantity)
            {
                uiElements.Add(new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("BasePicking_Details_Banner_Label")
                });
            }

            // Item1 represents whether it is to be displayed
            // Item2 represents the label
            // Item3 represents the value
            var displayDetails = new List<Tuple<bool, string, string>>
            {
                new Tuple<bool, string, string> (showProductId, Translate.GetLocalizedTextForKey("BasePicking_ProductId_Label"), pick.ProductScannedVerification),
                new Tuple<bool, string, string> (showProductName, Translate.GetLocalizedTextForKey("BasePicking_ProductName_Label"), pick.ProductName),
                new Tuple<bool, string, string> (showQuantity, Translate.GetLocalizedTextForKey("BasePicking_Quantity_Label"), pick.QuantityToPick.ToString()),
            };

            foreach (var detail in displayDetails)
            {
                if (detail.Item1)
                {
                    uiElements.Add(new UIElement
                    {
                        ElementType = UIElementType.Detail,
                        Label = detail.Item2,
                        Value = detail.Item3,
                        ValueInlineWithLabel = true
                    });
                }
            }
            return uiElements;
        }

        public bool ButtonResponseIsSkipSlot()
        {
            return ButtonResponse == BasePickingModuleVocab.SkipSlot.IdentificationKey;
        }
    }

    public class BasePickingStateMachine : SimplifiedBaseBusinessLogic<IBasePickingModel, BasePickingStateMachine, IBasePickingConfigRepository>
    {
        public static readonly CoreAppSMState BasePickingStart = new CoreAppSMState(nameof(BasePickingStart));
        public static readonly CoreAppSMState DisplayStartWork = new CoreAppSMState(nameof(DisplayStartWork));
        public static readonly CoreAppSMState StartPickStateMachine = new CoreAppSMState(nameof(StartPickStateMachine));

        private readonly ILog _Log = LogManager.GetLogger(nameof(BasePickingStateMachine));

        public override bool IsPrimaryStateMachine => true;

        // Properties that only this state machine need
        private PickStateMachine _PickSM;
        private PickStateMachine PickSM { get { return Manager.CreateStateMachine(ref _PickSM); } }

        public BasePickingStateMachine(SimplifiedStateMachineManager<BasePickingStateMachine, IBasePickingModel> manager, IBasePickingModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            ConfigureLogin(BasePickingStart, 
                           DisplayStartWork, 
                           "OperatorID", 
                           async (Operator newOperator) =>
                           {
                               Model.ResetOperator();

                               // Sign on
                               try
                               {
                                   bool signOnResponse = await Model.DataService.SignOnAsync(newOperator.OperatorIdentifier, newOperator.Password);
                                   if (!signOnResponse)
                                   {
                                       CurrentUserMessage = Translate.GetLocalizedTextForKey("BasePicking_SignOn_Authentication_Failed");
                                   }
                                   else
                                   {
                                       Model.CurrentSignedInOperator = newOperator;
                                       return true;
                                   }
                               }
                               // TODO: handle other exceptions or errors depending on communication mechanism
                               catch (OperationCanceledException)
                               {
                                   _Log.Debug("Operation canceled via communication Timeout Handler");
                                   Model.CurrentUserMessage = Translate.GetLocalizedTextForKey("BasePicking_NetworkTimeout_Error");
                               }

                               return false;
                           }, 
                           signOnBgndMsgKey: "BasePicking_BackgroundActivity_Header_SignOn");

            // Use a background activity to start the pick state machine since
            // the next state will use a network request to retrieve picks
            ConfigureDisplayState(DisplayStartWork,
                                  StartPickStateMachine,
                                  followedByBackgroundActivity: true,
                                  backgroundActivityHeaderKey: "BasePicking_BackgroundActivity_Header_GetPicks",
                                  encodeAction: EncodeStartWork);

            ConfigureLogicState(StartPickStateMachine,
                                async () =>
                                {
                                    Model.FirstAssignment = false;
                                    NextState = DisplayStartWork;
                                    PickSM.Reset();
                                    await PickSM.InitializeStateMachineAsync();
                                },
                                DisplayStartWork);
        }

        protected virtual WorkflowObjectContainer EncodeStartWork(IBasePickingModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();

            //Set prompt differently based on if it is first assignment or not
            var header = Translate.GetLocalizedTextForKey("BasePicking_Start_First_Assignment_Header");
            var prompt = Translate.GetLocalizedTextForKey("BasePicking_Start_First_Assignment_Prompt");
            if (!model.FirstAssignment)
            {
                header = Translate.GetLocalizedTextForKey("BasePicking_Start_Another_Assignment_Header");
                prompt = Translate.GetLocalizedTextForKey("BasePicking_Start_Another_Assignment_Prompt");
            }

            var wfo = WorkflowObjectFactory.CreateReadyIntent(header, "StartWork", prompt, model.CurrentUserMessage, initialPrompt: model.CurrentUserMessage);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }
    }
}
