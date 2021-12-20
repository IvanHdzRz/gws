//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using Common.Logging;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using RESTCommunication;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// A class that provides the vocabulary for the VoiceLink Module.
    /// </summary>
    public class VoiceLinkModuleVocab : DefaultModuleVocab
    {
        /// <summary>
        /// Gets the default platform independent vocabulary localization keys.
        /// It is constructed from the concatenation of
        /// <see cref="P:GuidedWork.DefaultModuleVocab.Numerics"/>, and
        /// <see cref="P:GuidedWork.DefaultModuleVocab.Alphas"/>. and workflow-
        /// specific keys.
        /// </summary>
        /// <value>The platform independent vocab.</value>
        public override VocabWordInfo[] PlatformIndependentVocab { get; } =
            BaseRequiredVocab
                .Concat(Numerics)
                .Concat(Alphas)
            .ToArray();

        //VoiceLink Module Platform Independent Vocabulary
        public static readonly VocabWordInfo SayAgain = new VocabWordInfo("VocabWord_SayAgain");
        public static readonly VocabWordInfo SignOff = new VocabWordInfo("VocabWord_SignOff");
        public static readonly VocabWordInfo Yes = new VocabWordInfo("VoiceLink_ButtonText_Yes");
        public static readonly VocabWordInfo No = new VocabWordInfo("VoiceLink_ButtonText_No");
        public static readonly VocabWordInfo ShortProduct = new VocabWordInfo("VoiceLink_Overflow_ShortProduct");
        public static readonly VocabWordInfo SkipAisle = new VocabWordInfo("VoiceLink_Overflow_SkipAisle");
        public static readonly VocabWordInfo SkipSlot = new VocabWordInfo("VoiceLink_Overflow_SkipSlot");
        public static readonly VocabWordInfo Partial = new VocabWordInfo("VoiceLink_Overflow_Partial");
        public static readonly VocabWordInfo ChangeFunction = new VocabWordInfo("VoiceLink_Additional_Cmd_Change_Function");
        public static readonly VocabWordInfo ChangeRegion = new VocabWordInfo("VoiceLink_Additional_Cmd_Change_Region");
        public static readonly VocabWordInfo Description = new VocabWordInfo("VoiceLink_Additional_Cmd_Description");
        public static readonly VocabWordInfo ItemNumber = new VocabWordInfo("VoiceLink_Additional_Cmd_Item_Number");
        public static readonly VocabWordInfo PassAssignment = new VocabWordInfo("VoiceLink_Additional_Cmd_Pass_Assignment");
        public static readonly VocabWordInfo RepeatLastPick = new VocabWordInfo("VoiceLink_Additional_Cmd_Repeat_Last_Pick");
    }

    public class VoiceLinkStateMachine : SimplifiedBaseBusinessLogic<IVoiceLinkModel, VoiceLinkStateMachine, IVoiceLinkConfigRepository>
    {
        public static readonly CoreAppSMState VoiceLinkStart = new CoreAppSMState(nameof(VoiceLinkStart));
        public static readonly CoreAppSMState CommRequestInitialData = new CoreAppSMState(nameof(CommRequestInitialData));
        public static readonly CoreAppSMState CommRequestBreakTypes = new CoreAppSMState(nameof(CommRequestBreakTypes));
        public static readonly CoreAppSMState ExecuteSignOn = new CoreAppSMState(nameof(ExecuteSignOn));
        public static readonly CoreAppSMState CollectVehicleInfo = new CoreAppSMState(nameof(CollectVehicleInfo));
        public static readonly CoreAppSMState RetrieveFunctions = new CoreAppSMState(nameof(RetrieveFunctions));
        public static readonly CoreAppSMState PerformFunctionsLUTResponse = new CoreAppSMState(nameof(PerformFunctionsLUTResponse));
        public static readonly CoreAppSMState DisplayFunctionSelection = new CoreAppSMState(nameof(DisplayFunctionSelection));
        public static readonly CoreAppSMState SelectFunction = new CoreAppSMState(nameof(SelectFunction));
        public static readonly CoreAppSMState SignOff = new CoreAppSMState(nameof(SignOff));
        public static readonly CoreAppSMState CommSignOff = new CoreAppSMState(nameof(CommSignOff));

        private readonly ILog _Log = LogManager.GetLogger(nameof(VoiceLinkStateMachine));

        protected LoginStateMachine _LoginSM;
        protected LoginStateMachine LoginSM { get { return Manager.CreateStateMachine(ref _LoginSM); } }

        protected VoiceLinkLUTStateMachine _VoiceLinkLUTSM;
        protected VoiceLinkLUTStateMachine VoiceLinkLUTSM { get { return Manager.CreateStateMachine(ref _VoiceLinkLUTSM); } }

        // Properties that only this state machine need
        private SelectionStateMachine _SelectionSM;
        private SelectionStateMachine SelectionSM { get { return Manager.CreateStateMachine(ref _SelectionSM); } }

        public VoiceLinkStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
            model.VoiceLinkLUTSM = VoiceLinkLUTSM;
        }

        public override void ConfigureStates()
        {
            ConfigureDisplayState(VoiceLinkStart, CommRequestInitialData, followedByBackgroundActivity: true, backgroundActivityHeaderKey: "VoiceLink_BackgroundActivity_Header_InitialData");

            ConfigureLogicState(CommRequestInitialData,
                                async () =>
                                {
                                    NextState = CommRequestInitialData;

                                    _OperatorUpdateService.ClearOperator();
                                    if (Model.VoiceConsoleEnabled)
                                    {
                                        try
                                        {
                                            // Send an initial terminal properties.
                                            await _VoiceConsoleComm.SendTerminalPropertiesAsync();
                                        }
                                        catch (RESTResponseException)
                                        {
                                            _Log.Warn("Error sending initial messages to VoiceConsole.  Trying again.");
                                            return;
                                        }
                                    }

                                    // Send device configuration
                                    NextState = CommRequestBreakTypes;
                                    await Model.LUTtransmit(LutType.Config, "VoiceLink_BackgroundActivity_Header_Loading_Config",
                                        goToStateIfFail: CommRequestInitialData
                                    );
                                },
                                CommRequestInitialData,
                                CommRequestBreakTypes);

            ConfigureLogicState(CommRequestBreakTypes,
                                async () =>
                                {
                                    // Get break types
                                    Model.AvailableBreakTypes.Clear();
                                    NextState = ExecuteSignOn;
                                    await Model.LUTtransmit(LutType.BreakTypes, "VoiceLink_BackgroundActivity_Header_Loading_BreakTypes",
                                        goToStateIfFail: CommRequestInitialData
                                    );
                                },
                                ExecuteSignOn);

            ConfigureLogin(ExecuteSignOn,
                           CollectVehicleInfo,
                           "OperIdent",
                           async (GuidedWorkRunner.Operator oper) =>
                           {
                               LoginSM.Reset();
                               await LoginSM.InitializeStateMachineAsync(oper);

                               return true;
                           },
                           retrieveOpersBgndMsgKey: "VoiceLink_BackgroundActivity_Header_RetrievingOperators",
                           signOnBgndMsgKey: "VoiceLink_BackgroundActivity_Header_SignOn");

            ConfigureLogicState(CollectVehicleInfo,
                                async () =>
                                {
                                    // TODO: check CollectVehicleInfo application property
                                    var collectVehicleInfo = false;
                                    NextState = RetrieveFunctions;
                                    if (collectVehicleInfo)
                                    {
                                        // TODO: implement vehicle task
                                        await Task.CompletedTask;
                                    }
                                },
                                RetrieveFunctions);

            ConfigureLogicState(RetrieveFunctions,
                                async () =>
                                {
                                    NextState = PerformFunctionsLUTResponse;

                                    // Retrieve available functions
                                    await Model.LUTtransmit(LutType.GetFunctions, "VoiceLink_BackgroundActivity_Header_Loading_Functions",
                                        goToStateIfFail: ExecuteSignOn
                                    );
                                },
                                PerformFunctionsLUTResponse);

            ConfigureLogicState(PerformFunctionsLUTResponse,
                                () =>
                                {
                                    NextState = DisplayFunctionSelection;
                                    if (FunctionsResponse.ValidFunctionCount == 0)
                                    {
                                        CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_SignOn_Failed_NoValidFunctions");
                                        NextState = SignOff;
                                    }
                                },
                                SignOff,
                                DisplayFunctionSelection);

            ConfigureDisplayState(DisplayFunctionSelection,
                                  SelectFunction,
                                  onEntryAction: () => Model.ResetFunction(),
                                  encodeAction: EncodeSelectFunction,
                                  decodeAction: DecodeSelectFunction,
                                  followedByBackgroundActivity: true,
                                  backgroundActivityHeaderKey: "VoiceLink_BackgroundActivity_Header_StartFunction",
                                  backgroundActivityHeaderArgs: new string[] { Model.CurrentFunction?.Description });

            ConfigureLogicState(SelectFunction,
                                async () =>
                                {
                                    NextState = SignOff;

                                    if (Model.SelectFunctionCanceled)
                                    {
                                        // Cancel pressed
                                        return;
                                    }

                                    // Trigger entry into the selected function
                                    if (Model.CurrentFunction.Number == 3)
                                    {
                                        SelectionSM.Reset();
                                        await SelectionSM.InitializeStateMachineAsync();
                                    }
                                    else
                                    {
                                        CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_SelectFunction_Failed_NotImplemented", Model.CurrentFunction.Description);
                                        NextState = DisplayFunctionSelection;
                                    }
                                },
                                SignOff,
                                DisplayFunctionSelection);

            ConfigureLogicState(SignOff,
                                () =>
                                {
                                    backgroundActivityNextState = CommSignOff;
                                    CurrentBackGroundActivityHeader = Translate.GetLocalizedTextForKey("VoiceLink_Signing_Off_Header");
                                    NextState = CoreAppStates.BackgroundActvity;
                                },
                                CoreAppStates.BackgroundActvity);

            /// <summary>
            /// Execute operator signoff on the server and clear any workflow data
            /// </summary>
            /// <returns></returns>
            ConfigureLogicState(CommSignOff,
                                async () =>
                                {
                                    NextState = CommRequestInitialData;
                                    Model.ResetOperator();
                                    await Model.LUTtransmit(LutType.SignOff,
                                        ignoreErrors: new List<int>() { 97 }
                                    );
                                },
                                CommRequestInitialData);
        }


        public override void Reset()
        {
            base.Reset();
            SelectionSM.Reset();
        }

        protected virtual WorkflowObjectContainer EncodeSelectFunction(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            WorkflowObject wfo = null;
            var functions = FunctionsResponse.CurrentResponse;
            if (functions.Count > 1)
            {
                wfo = WorkflowObjectFactory.CreateMenuItemsIntent(Translate.GetLocalizedTextForKey("VoiceLink_SelectFunction_Header"),
                                                                  "selectedFunction",
                                                                  Translate.GetLocalizedTextForKey("VoiceLink_SelectFunction_Prompt"),
                                                                  message: model.CurrentUserMessage,
                                                                  initialPrompt: model.CurrentUserMessage,
                                                                  isPriorityPrompt: false);

                //Add functions to menu item list, with function number as spoken key value.
                foreach (var f in functions)
                {
                    wfo.MenuItemsProperties.AddMenuItem(f.Description, key: f.Number.ToString());
                }
                wfo.MenuItemsProperties.DisplayKeys = true;
            }
            else if (functions.Count == 1)
            {
                wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("VoiceLink_SelectFunctionSingle_Header"),
                                                              "readyOne",
                                                              Translate.GetLocalizedTextForKey("VoiceLink_SelectFunctionSingle_Prompt", functions[0].Description),
                                                              model.CurrentUserMessage,
                                                              initialPrompt: model.CurrentUserMessage,
                                                              isPriorityPrompt: false);
                wfo.UIElements = new List<UIElement>
                {
                    new UIElement
                    {
                        ElementType = UIElementType.Detail,
                        Value = Translate.GetLocalizedTextForKey("VoiceLink_SelectFunctionSingle_Prompt", functions[0].Description),
                        Centered = true,
                        Bold = true
                    }
                };

            }

            if (wfo != null)
            {
                wfo.MessageType = model.MessageType;
                wfoContainer.Add(wfo);
            }
            return wfoContainer;
        }

        private void DecodeSelectFunction(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            //If 1 function, and got here, then user selected "ready"
            //so set to first function/only function
            var selectedFunctionName = FunctionsResponse.CurrentResponse[0].Description;
            model.SelectFunctionCanceled = false;
            if (FunctionsResponse.CurrentResponse.Count > 1)
            {
                (var menuItems, var cancelled) = GenericBaseEncoder<IVoiceLinkModel>.DecodeMenuItems(slotContainer);
                selectedFunctionName = menuItems.First((i) => i.Selected).DisplayName;
                model.SelectFunctionCanceled = cancelled;
            }
            
            model.CurrentFunction = FunctionsResponse.CurrentResponse.FirstOrDefault(f => f.Description == selectedFunctionName);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
