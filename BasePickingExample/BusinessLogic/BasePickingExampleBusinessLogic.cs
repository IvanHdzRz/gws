using Common.Logging;
using GuidedWork;
using GuidedWorkRunner;
using Honeywell.Firebird.CoreLibrary;
using System.Linq;
using TinyIoC;
using Honeywell.Firebird.CoreLibrary.Localization;
using System;
using Operator = GuidedWorkRunner.Operator;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace BasePickingExample
{
    public class BasePickingExampleModuleVocab : DefaultModuleVocab
    {
        public override VocabWordInfo[] PlatformIndependentVocab { get; } =
            BaseRequiredVocab
                .Concat(Numerics).ToArray();
        public static readonly VocabWordInfo SkipSlot = new VocabWordInfo("BasePicking_VocabWord_SkipSlot");
        public static readonly VocabWordInfo ShortProduct = new VocabWordInfo("BasePicking_VocabWord_ShortProduct");
        public static readonly VocabWordInfo SignOff = new VocabWordInfo("BasePicking_Vocab_Additional_Cmd_SignOff");
    }

    public interface IBasePickingExampleModel : IGenericIntentModel<IBasePickingExampleModel>
    {
        // TODO: Add model property interfaces here
        //Main Properties
        public Operator CurrentSignedInOperator { get; set; }
        bool FirstAssignment { get; set; }
        IBasePickingExampleRESTServiceProvider ServiceProvider { get; set; }
        List<Pick> Picks { get; set; }
        Pick CurrentPick { get; set; }
        bool EnteredConfirmShortProductResponse { get; set; }

        int CompletedPicks { get; }
        int TotalPicks { get; }
        string LocationCheckDigitResponse { get; set; }
        string ProductBatchNumberResponse { get; set; }
        string EnteredPickQuantityString { get; set; }
        int EnteredPickQuantity { get; set; }
        bool SignOffAllowed { get; }
        string idContainer { get; set; }
        string idAssignament { get; set; }
        GetContainerResponce Containers { get; set; }
        OpenContainerResponce StatusOpenContainer { get; set; }
        List<UIElement> GenerateUIElements(Pick pick,
                           bool showAisle = false,
                           bool showSlot = false,
                           bool showProductId = false,
                           bool showProductName = false,
                           bool showQuantity = false);
        void ConfigureProgressBar(WorkflowObject wfo);


        public void ResetOperator();
       
    }

    public class BasePickingExampleModel : SimplifiedIntentModel<BasePickingExampleBusinessLogic, IBasePickingExampleModel>, IBasePickingExampleModel
    {
        // TODO: Add model property implemenations here
        private readonly ILog _Log = LogManager.GetLogger(nameof(BasePickingExampleModel));

        public Operator CurrentSignedInOperator { get; set; }
        public bool FirstAssignment { get; set; } = true;
        public IBasePickingExampleRESTServiceProvider ServiceProvider { get; set; }
        public List<Pick> Picks { get; set; }
        public Pick CurrentPick { get; set; }

        public int CompletedPicks => Picks.Where(p => p.Picked).Count();
        public int TotalPicks => Picks.Count();
        public string LocationCheckDigitResponse { get; set; }
        public string ProductBatchNumberResponse { get; set; }

        public string EnteredPickQuantityString { get; set; }
        public int EnteredPickQuantity { get; set; }
        public bool EnteredConfirmShortProductResponse { get; set; }
        public bool SignOffAllowed => CurrentSignedInOperator != null;
        //my custom props for examSM
        public string idContainer { get; set; }
        public string idAssignament { get; set; }
        public GetContainerResponce Containers { get; set; }
        public OpenContainerResponce StatusOpenContainer { get; set; }
        public override List<InteractiveItem> AllowedAdditionalVocab()
        {
            var result = new List<InteractiveItem>();

            result.Add(new InteractiveItem(BasePickingExampleModuleVocab.SignOff,
                itemValidationDelegate: ValidateSignOffAsync, processActionAsync: ProcessSignOffAsync, confirm: true));

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
                await ServiceProvider.SignOffAsync(CurrentOperator.OperatorIdentifier);
                ResetOperator();
                return BasePickingExampleBusinessLogic.BasePickingStart;
            }
            // TODO: handle other exceptions or errors depending on communication mechanism
            catch (OperationCanceledException)
            {
                _Log.Debug("Operation canceled via communication Timeout Handler");
                CurrentUserMessage = Translate.GetLocalizedTextForKey("BasePicking_NetworkTimeout_Error");
            }

            return null;
        }

        public void ResetOperator()
        {
            CurrentSignedInOperator = null;
            CurrentPick = null;
            FirstAssignment = true;
        }

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
    }

    public class BasePickingExampleBusinessLogic : SimplifiedBaseBusinessLogic<IBasePickingExampleModel, BasePickingExampleBusinessLogic, BasePickingExampleConfigRepository>
    {
        // TODO: Add state machine states here
        public static readonly CoreAppSMState BasePickingStart = new CoreAppSMState(nameof(BasePickingStart));
        public static readonly CoreAppSMState DisplayStartWork = new CoreAppSMState(nameof(DisplayStartWork));
        public static readonly CoreAppSMState StartPickStateMachine = new CoreAppSMState(nameof(StartPickStateMachine));
        //add new state for initialize my new custom SM
        public static readonly CoreAppSMState StartExamSM = new CoreAppSMState(nameof(StartExamSM));



        private readonly ILog _Log = LogManager.GetLogger(nameof(BasePickingExampleBusinessLogic));

        public override bool IsPrimaryStateMachine => true;
        //Nueva maquina de estados 
        private BasePickingExamplePickStateMachine _PickSM;
        //Getter de la maquina de estados
        private BasePickingExamplePickStateMachine PickSM { get { return Manager.CreateStateMachine(ref _PickSM); } }
        //my Custom StateMachine for exam
        private ExamStateMachine _ExamSM;
        private ExamStateMachine ExamSM { get { return Manager.CreateStateMachine(ref _ExamSM); } }
        //Constructor
        public BasePickingExampleBusinessLogic(SimplifiedStateMachineManager<BasePickingExampleBusinessLogic, IBasePickingExampleModel> manager, IBasePickingExampleModel model) : base(manager, model)
        {
            Model.ServiceProvider = TinyIoCContainer.Current.Resolve<IBasePickingExampleRESTServiceProvider>();
        }
        //En este metodo se configuran las transiciones entre estados de la maquina de estados 
        public override void ConfigureStates()
        {
            // TODO: Add state configuration here
            ConfigureLogin(BasePickingStart,
               DisplayStartWork,
               "OperatorID",
               async (Operator newOperator) =>
               {
                   Model.ResetOperator();
                   // Sign on
                   try
                   {
                       var signOnResponse = await Model.ServiceProvider.SignOnAsync(newOperator.OperatorIdentifier, newOperator.Password);
                       if (signOnResponse.Status != "success")
                       {
                           CurrentUserMessage                                                                                                                                                                                                                                                                                                                                = Translate.GetLocalizedTextForKey("BasePicking_SignOn_Authentication_Failed");
                       }
                       else
                       {
                           Model.CurrentSignedInOperator = newOperator;
                           return true;
                       }
                   }
                   catch (OperationCanceledException)
                   {
                       _Log.Debug("Operation canceled via communication Timeout Handler");
                       Model.CurrentUserMessage = Translate.GetLocalizedTextForKey("BasePicking_NetworkTimeout_Error");
                   }
                   return false;
               },
               signOnBgndMsgKey: "BasePicking_BackgroundActivity_Header_SignOn");

            ConfigureDisplayState(DisplayStartWork,
                      StartExamSM,
                      followedByBackgroundActivity: true,
                      backgroundActivityHeaderKey: Translate.GetLocalizedTextForKey("ExamSM_Starting"),
                      encodeAction: EncodeStartWork);
            // once complete my custom State Machine return to normal aplication flow
            // to pickStateMachine
            ConfigureLogicState(StartExamSM,
                async ()=> {
                    Model.FirstAssignment = false;
                    NextState = StartPickStateMachine;
                    ExamSM.Reset();
                    await ExamSM.InitializeStateMachineAsync();
                },
                StartPickStateMachine);

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

        // TODO: Add state encoder, decoder and additional logic methods here
        protected virtual WorkflowObjectContainer EncodeStartWork(IBasePickingExampleModel model)
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
