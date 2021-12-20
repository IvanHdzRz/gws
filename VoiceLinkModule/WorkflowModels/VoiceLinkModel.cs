//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using TinyIoC;

    /// <summary>
    /// The model of the VoiceLink workflow.
    /// </summary>
    public class VoiceLinkModel : SimplifiedIntentModel<VoiceLinkStateMachine, IVoiceLinkModel>, IVoiceLinkModel
    {
        public IVoiceLinkDataService DataService { get; private set; }
        public IVoiceLinkConfigRepository VoiceLinkConfigRepository { get; private set; }
        public IConfigurationDataService ConfigurationDataService { get; private set; }
        public VoiceLinkLUTStateMachine VoiceLinkLUTSM { get; set; }

        //Main Properties        
        public bool SignOffAllowed { get; set; } = true;
        public VLConfig CurrentConfig { get; set; }
        public List<BreakType> AvailableBreakTypes { get; set; } = new List<BreakType>();
        public bool SelectFunctionCanceled { get; set; }
        public Function CurrentFunction { get; set; }

        //Selection Region Properties
        public List<Region> AvailableRegions { get; set; }
        public Region CurrentRegion { get; set; }

        //Assignment Issuance properties
        public string WorkId { get; set; }
        public List<string> DuplicateWorkIds { get; set; }
        public bool WorkIdScanned { get; set; } = false;

        //Direct to pick properties
        public string CurrentPreAisle { get; set; }
        public string CurrentAisle { get; set; }
        public string CurrentPostAisle { get; set; }

        //Additional Vocab properties
        public bool? PassInprogress { get; set; }
        public bool? Partial { get; set; }

        public bool AdditionalVocabEnabled { get; set; } = true;

        public LutParameter LutParameters { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:VoiceLink.VoiceLinkModel"/> class.
        /// </summary>
        /// <param name="voiceLinkDataService">A means to communicate with VoiceLink.</param>
        /// <param name="configurationDataService">Configuration data service for VoiceLink.</param>
        /// <param name="voiceLinkConfigRepository">Configuration repository for VoiceLink.</param>
        public VoiceLinkModel(IVoiceLinkDataService voiceLinkDataService,
            IVoiceLinkConfigRepository voiceLinkConfigRepository,
            IConfigurationDataService configurationDataService)
        {
            DataService = voiceLinkDataService;
            VoiceLinkConfigRepository = voiceLinkConfigRepository;

            //Register repositories for application to allow for external configuration
            ConfigurationDataService = configurationDataService;
        }

        /// <summary>
        /// Initializes the workflow.
        /// </summary>
        /// <returns>A task representing the asynchronous initialize operation.</returns>
        public override async Task InitializeWorkflowAsync()
        {
            DataService = TinyIoCContainer.Current.Resolve<IVoiceLinkDataService>();
            DataService.Initialize();

            //Load VoiceLink settings from file if it exists. 
            //Delete after loading because it may contain secure information.
            ConfigurationDataService.ApplyConfigurationFromFile("VoiceLinkSettings.config", true);

            // Initialize two lists used for tracking previous picked items
            CurrentPickList = new List<Pick>();
            PreviousPickList = new List<Pick>();

            await base.InitializeWorkflowAsync();
        }

        private List<Pick> CurrentPickList { get; set; }
        private List<Pick> PreviousPickList { get; set; }

        public void SetNextPick(List<Pick> picks)
        {
            if (CurrentPickList.Count == 0)
            {
                PreviousPickList = CurrentPickList;
            }
            else
            {
                foreach (var pick in CurrentPickList)
                {
                    if (pick.Status == "P")
                    {
                        PreviousPickList = CurrentPickList;
                        break;
                    }
                }
            }

            CurrentPickList = picks;
            if (CurrentPickList == null)
            {
                CurrentPickList = new List<Pick>();
            }
        }

        //Main Pick property that returns the current pick,
        // returns null if there are no more picks
        public Pick CurrentPick
        {
            get
            {
                if (PickAssignmentStateMachine.PickList != null && PickAssignmentStateMachine.PickList.Count > 0)
                {
                    return PickAssignmentStateMachine.PickList.First();
                }
                return null;
            }
        }


        /// <summary>
        /// Reset this instance.
        /// </summary>
        public override void Reset()
        {
            CurrentUserMessage = null;
            MessageType = UserMessageType.Error;
            CurrentPickList = new List<Pick>();
            base.Reset();
        }

        /// <summary>
        /// Resets information for current operator
        /// after signing off, also clears any 
        /// function, region, and assignment operator
        /// may have been working in.
        /// </summary>
        public void ResetOperator()
        {
            CurrentOperator = null;
            ResetFunction();
        }

        /// <summary>
        /// Clears currently selected function, region
        /// and assignment for current function
        /// </summary>
        public void ResetFunction()
        {
            CurrentFunction = null;
            ResetSelectionRegion();
        }

        /// <summary>
        /// Clears currently selected region
        /// and assignment for current region
        /// </summary>
        public void ResetSelectionRegion()
        {
            CurrentRegion = null;
            ResetSelectionAssignment();
        }

        /// <summary>
        /// Clears current assignment and picks 
        /// so a new assignment may be started
        /// </summary>
        public void ResetSelectionAssignment()
        {
            PickAssignmentStateMachine.PickList?.Clear();
            CurrentPickList.Clear();
        }

        /// <summary>
        /// Resets current location settings so worker will 
        /// be fully redirected back to pick
        /// </summary>
        public void ResetAisleDirections()
        {
            CurrentPreAisle = "";
            CurrentAisle = "";
            CurrentPostAisle = "";
        }

        /// <summary>
        /// Helper method for making data requests from server (LUT/ODRs) 
        /// Launches the state machine for make such requests
        /// </summary>
        /// <param name="lutType">Which request should be made</param>
        /// <param name="header">Header message to display on screen while request is in progress</param>
        /// <param name="ignoreErrors">Error code to ignore, and that will be handled in standard business code</param>
        /// <param name="parameters">Parameters/data to send with request</param>
        /// <param name="goToStateIfFail">State to return to if server request fails</param>
        public async Task LUTtransmit(LutType lutType, 
            string header = null, List<int> ignoreErrors = null, LutParameter parameters = null, CoreAppSMState goToStateIfFail = null)
        {
            var _ignoreErrors = new List<int> { 0 };
            if (ignoreErrors != null)
            {
                _ignoreErrors.AddRange(ignoreErrors);
            }
            LutParameters = parameters;
            await VoiceLinkLUTSM.InitializeStateMachineAsync(lutType, header, goToStateIfFail, _ignoreErrors);
        }

        #region AddtionalCommandActions
        public override List<InteractiveItem> AllowedAdditionalVocab()
        {
            var result = new List<InteractiveItem>();

            // Sign off available whenever operator signed in
            if (CurrentOperator != null && !string.IsNullOrEmpty(CurrentOperator.Password))
            {
                result.Add(new InteractiveItem(VoiceLinkModuleVocab.SignOff,
                    itemValidationDelegate: ValidateSignOff, processActionAsync: PerformSignOff,
                    confirm: true, useAsBackButtonAction: true));
            }

            if (AdditionalVocabEnabled)
            {
                //Additional picking commands
                if (CurrentPick != null)
                {
                    result.Add(new InteractiveItem(VoiceLinkModuleVocab.ItemNumber, processActionAsync: PerformItemNumber));
                    result.Add(new InteractiveItem(VoiceLinkModuleVocab.Description, processActionAsync: PerformDescription));
                    result.Add(new InteractiveItem(VoiceLinkModuleVocab.RepeatLastPick, processActionAsync: PerformRepeatLastPick));
                    result.Add(new InteractiveItem(VoiceLinkModuleVocab.PassAssignment,
                        itemValidationDelegate: ValidatePassAssignment, processActionAsync: PerformPassAssignment, confirm: true));
                }
            }

            if (AdditionalVocabEnabled || (LutType)CurrentComm == LutType.GetAssignments)
            {
                //Change Function Allowed, whenever function selected, but no current work
                if (CurrentPick == null && CurrentFunction != null && CurrentStateMachineName != nameof(PickAssignmentStateMachine))
                {
                    result.Add(new InteractiveItem(VoiceLinkModuleVocab.ChangeFunction, processActionAsync: PerformChangeFunctionKey, confirm: true));
                }

                //Change region allowed, whenever a Region has been selection, but not current work 
                if (CurrentPick == null && CurrentRegion != null && CurrentStateMachineName != nameof(PickAssignmentStateMachine))
                {
                    result.Add(new InteractiveItem(VoiceLinkModuleVocab.ChangeRegion, processActionAsync: PerformChangeRegion, confirm: true));
                }
            }

            return result;
        }

        private string ValidateSignOff()
        {
            if (!SignOffAllowed)
            {
                return Translate.GetLocalizedTextForKey("VoiceLink_Signoff_Not_Allowed_Prompt");
            }
            return null;
        }

        private Task<CoreAppSMState> PerformSignOff()
        {
            CurrentUserMessage = null;
            if (CurrentStateMachineName != "VoiceLinkLUTStateMachine")
            {
                return Task.FromResult(VoiceLinkStateMachine.SignOff);
            }

            return Task.FromResult((CoreAppSMState)null);
        }
        private Task<CoreAppSMState> PerformItemNumber()
        {
            if (string.IsNullOrEmpty(CurrentPick.ItemNumber))
            {
                CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_Generic_Not_Available", AdditionalVocabProcessing.Display);
                MessageType = UserMessageType.Warning;
            }
            else
            {
                CurrentUserMessage = CurrentPick.ItemNumber;
                MessageType = UserMessageType.Standard;
            }
            return Task.FromResult((CoreAppSMState)null);
        }

        private Task<CoreAppSMState> PerformDescription()
        {
            if (string.IsNullOrEmpty(CurrentPick.ItemDescription))
            {
                CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_Generic_Not_Available", AdditionalVocabProcessing.Display);
                MessageType = UserMessageType.Warning;
            }
            else
            {
                CurrentUserMessage = CurrentPick.ItemDescription;
                MessageType = UserMessageType.Standard;
            }
            return Task.FromResult((CoreAppSMState)null);
        }

        private Task<CoreAppSMState> PerformRepeatLastPick()
        {
            if (PreviousPickList.Count == 0)
            {
                CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_Generic_Not_Available", AdditionalVocabProcessing.Display);
                MessageType = UserMessageType.Warning;
            }
            else
            {
                int picked = 0, total = 0;
                foreach (var pick in PreviousPickList)
                {
                    picked += pick.QuantityPicked;
                    total += pick.QuantityToPick;
                }

                if (PreviousPickList[0].Aisle != null && PreviousPickList[0].Aisle != "")
                {
                    // there is Aisle information in the Pick
                    CurrentUserMessage = Translate.GetLocalizedTextForKey(
                        "VoiceLink_Repeat_Last_Pick_Prompt",
                        PreviousPickList[0].Aisle,
                        PreviousPickList[0].Slot,
                        picked + "",
                        total + ""
                        );
                }
                else
                {
                    // there is no Aisle information in the Pick
                    CurrentUserMessage = Translate.GetLocalizedTextForKey(
                        "VoiceLink_Repeat_Last_Pick_Prompt_No_Aisle",
                        PreviousPickList[0].Slot,
                        picked + "",
                        total + ""
                        );
                }
                MessageType = UserMessageType.Standard;
            }
            return Task.FromResult((CoreAppSMState)null);
        }

        private string ValidatePassAssignment()
        {
            if (!PickingRegionsResponse.CurrentPickingRegion.AllowPass ||
                (SelectionStateMachine.PickOnly && !PickingRegionsResponse.CurrentPickingRegion.PickByPick) ||
                PassInprogress == true ||
                PicksResponse.HasPicks(null, new List<string>(new string[] { "B" })) ||
                AssignmentsResponse.FirstAssignment().IsChase ||
                Partial == true)
            {
                return Translate.GetLocalizedTextForKey("VoiceLink_PassAssignment_Not_Allowed_Prompt");
            }

            return null;
        }

        private Task<CoreAppSMState> PerformPassAssignment()
        {
            PassInprogress = true;
            PicksResponse.ChangeStatus("X", "N");
            foreach (var assignment in AssignmentsResponse.CurrentResponse)
            {
                if (PicksResponse.HasPicks(assignment, new List<string>(new string[] { "X" })))
                {
                    assignment.PassAssignment = 1;
                }
                else
                {
                    assignment.PassAssignment = 0;
                }
            }
            return Task.FromResult(PickAssignmentStateMachine.CheckNextPick);
        }

        private Task<CoreAppSMState> PerformChangeFunctionKey()
        {
            if (CurrentFunction == null)
            {
                //Ignore user even spoke command.
                return Task.FromResult<CoreAppSMState>(null);
            }

            return Task.FromResult(VoiceLinkStateMachine.RetrieveFunctions);
        }

        private Task<CoreAppSMState> PerformChangeRegion()
        {
            return Task.FromResult(SelectionStateMachine.StartSelection);
        }
        #endregion
    }
}