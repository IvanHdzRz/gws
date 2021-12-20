//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Threading.Tasks;
    using GuidedWork;

    public class OrderPickingFileDataTransport : WorkflowFileDataTransport, IOrderPickingDataTransport
    {
        public OrderPickingFileDataTransport(IWorkflowParameterService workflowParameterService,
            IWorkflowResourceRegistry workflowResourceRegistry) : base(workflowParameterService, workflowResourceRegistry)
        {
        }

        /// <summary>
        /// Fetches a JSON-encoded string from the common data and workflow specific
        /// data JSON files.
        /// </summary>
        /// <returns>A Task to indicate the availabily of the JSON-encoded OrdersDTO instance.</returns>
        public Task<string> FetchOrderPickingDTOAsync()
        {
            return Task.FromResult(GetJsonValue("fetchOrders"));
        }

        /// <summary>
        /// There is currently no storage of the actual picked quantity.
        /// </summary>
        /// <param name="pickIdentifier">The product identifier</param>
        /// <param name="quantity">The amount picked</param>
        /// <returns>A task to indicate when the operation is complete</returns>
        public Task StorePickedQuantityAsync(string pickIdentifier, int quantity)
        {
            return Task.CompletedTask;
        }

        public Task StoreStagingLocationAsync(long orderId, string stagingLocation)
        {
            return Task.CompletedTask;
        }
    }
}

