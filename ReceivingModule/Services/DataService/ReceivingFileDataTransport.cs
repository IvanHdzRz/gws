//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Threading.Tasks;
    using GuidedWork;

    public class ReceivingFileDataTransport : WorkflowFileDataTransport, IReceivingDataTransport
    {
        public ReceivingFileDataTransport(IWorkflowParameterService workflowParameterService,
            IWorkflowResourceRegistry workflowResourceRegistry) : base(workflowParameterService, workflowResourceRegistry)
        {
        }

        /// <summary>
        /// Fetches a JSON-encoded string from the workflow specific
        /// data JSON files.
        /// </summary>
        /// <returns>A Task to indicate the availabily of the JSON-encoded OrdersDTO instance.</returns>
        public Task<string> FetchReceivingDTOAsync()
        {
            return Task.FromResult(GetJsonValue("fetchReceivingAssignments"));
        }
    }
}

