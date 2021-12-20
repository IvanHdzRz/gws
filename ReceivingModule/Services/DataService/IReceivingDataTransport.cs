//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Threading.Tasks;

    // Extend the IWorkflowDataTransport to include the opertions required
    // of the order picking module.
    public interface IReceivingDataTransport 
    {
        string Name { get; }

        /// <summary>
        /// Fetches a JSON-encoded string that corresponds to an receiving assignment list for the
        /// currently selected worker.
        /// </summary>
        /// <returns>A Task to indicate the availabily of the JSON-encoded OrdersDTO instance.</returns>
        Task<string> FetchReceivingDTOAsync();
    }
}
