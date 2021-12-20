//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GuidedWorkRunner;

    public class SelectionRegionStateMachine : SingleRegionStateMachine
    {
        public SelectionRegionStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
        }
        
        protected override async Task CommGetValidRegionsAsync()
        {
            NextState = AfterGetValidRegionsTransmit;
            Model.AvailableRegions.Clear();
            SelectionStateMachine.InProgressWork = false;

            await Model.LUTtransmit(LutType.GetRegionPermissions, "VoiceLink_BackgroundActivity_Header_Loading_Regions",
                new List<int>() { ERROR_CODE_IN_PROGRESS_SPECIFIC_REGION, ERROR_CODE_IN_PROGRESS_ANOTHER_FUNCTION },
                goToStateIfFail: VoiceLinkStateMachine.DisplayFunctionSelection
            );
        }

        protected override async Task CommGetRegionConfigAsync()
        {
            if (Model.AvailableRegions.Count == 1)
            {
                Model.CurrentRegion = Model.AvailableRegions[0];
            }
            if (Model.CurrentRegion == null)
            {
                // Cancel pressed
                NextState = VoiceLinkStateMachine.DisplayFunctionSelection;
                return;
            }

            await Model.LUTtransmit(LutType.GetPickingRegion, "VoiceLink_BackgroundActivity_Header_Loading_Region_Config",
                new List<int>() { ERROR_CODE_IN_PROGRESS_SPECIFIC_REGION, ERROR_CODE_IN_PROGRESS_ANOTHER_FUNCTION },
                goToStateIfFail: DisplayRegionSelection
            );
        }
    }
}
