//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using GuidedWorkRunner;
    using System;
    using System.Threading.Tasks;

    public class GetAssignmentAutoStateMachine : GetAssignmentStateMachine
    {
        public GetAssignmentAutoStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
        }

        protected override void PerformStartGetAssignment()
        {
            _NumberOfAssignmentsToRequest = 1;
            NextState = CommGetAssignments;

            if (SelectionStateMachine.InProgressWork)
            {
                return;
            }

            var numberOfAssignmentsAllowed = PickingRegionsResponse.CurrentPickingRegion.NumAssignsAllowed;
            if (numberOfAssignmentsAllowed > 1)
            {
                // TODO add support for multiple assignments
                throw new NotImplementedException("Regions with multiple assignments not yet implemented");
            }
        }

        protected override async Task CommPerformGetAssignmentsAsync()
        {
            await base.CommPerformGetAssignmentsAsync();

            // TODO: prompt for when multiple assignments supported but not enough available.
        }
    }
}
