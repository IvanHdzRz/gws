//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GuidedWork;
    using GuidedWorkRunner;

    /// <summary>
    /// The model of the VoiceLink Workflow.  The interface abstracts the retrieval of data from the 
    /// data service.  The model serves as temporary storage of information between controllers associated 
    /// with simple WFAs.
    /// </summary>
    public interface IVoiceLinkModel : IGenericIntentModel<IVoiceLinkModel>
    {
        IVoiceLinkDataService DataService { get; }
        IVoiceLinkConfigRepository VoiceLinkConfigRepository { get; }
        IConfigurationDataService ConfigurationDataService { get; }
        VoiceLinkLUTStateMachine VoiceLinkLUTSM { get; set; }

        //Main Properties        
        bool SignOffAllowed { get; set; }
        VLConfig CurrentConfig { get; set; }
        List<BreakType> AvailableBreakTypes { get; set; }
        bool SelectFunctionCanceled { get; set; }
        Function CurrentFunction { get; set; }

        //Selection Region Properties
        List<Region> AvailableRegions { get; set; }
        Region CurrentRegion { get; set; }

        //Assignment Issuance properties
        string WorkId { get; set; }
        bool WorkIdScanned { get; set; }
        List<string> DuplicateWorkIds { get; set; }

        //Direct to pick properties
        string CurrentPreAisle { get; set; }
        string CurrentAisle { get; set; }
        string CurrentPostAisle { get; set; }

        //Additional Vocab properties
        bool? PassInprogress { get; set; }
        bool? Partial { get; set; }

        bool AdditionalVocabEnabled { get; set; }

        LutParameter LutParameters { get; set; }

        Pick CurrentPick { get; }
        void SetNextPick(List<Pick> picks);

        /// <summary>
        /// Resets information for current operator
        /// after signing off, also clears any 
        /// function, region, and assignment operator
        /// may have been working in.
        /// </summary>
        void ResetOperator();

        /// <summary>
        /// Clears currently selected function, region
        /// and assignment for current function
        /// </summary>
        void ResetFunction();

        /// <summary>
        /// Clears currently selected region
        /// and assignment for current region
        /// </summary>
        void ResetSelectionRegion();

        /// <summary>
        /// Clears current assignment and picks 
        /// so a new assignment may be started
        /// </summary>
        void ResetSelectionAssignment();

        /// <summary>
        /// Resets current location settings so worker will 
        /// be fully redirected back to pick
        /// </summary>
        void ResetAisleDirections();

        /// <summary>
        /// Helper method for making data requests from server (LUT/ODRs) 
        /// Launches the state machine for make such requests
        /// </summary>
        /// <param name="lutType">Which request should be made</param>
        /// <param name="header">Header message to display on screen while request is in progress</param>
        /// <param name="ignoreErrors">Error code to ignore, and that will be handled in standard business code</param>
        /// <param name="parameters">Parameters/data to send with request</param>
        /// <param name="goToStateIfFail">State to return to if server request fails</param>
        Task LUTtransmit(LutType lutType, 
            string header = null, List<int> ignoreErrors = null, LutParameter parameters = null, CoreAppSMState goToStateIfFail = null);
    }
}
