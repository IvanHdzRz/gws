//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Threading.Tasks;
    using GuidedWork;

    /// <summary>
    /// The data transports for Warehouse Picking.
    /// </summary>
    public class WarehousePickingFileDataTransport : WorkflowFileDataTransport, IWarehousePickingDataTransport
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:WarehousePicking.WarehousePickingFileDataTransport"/> class.
        /// </summary>
        /// <param name="workflowParameterService">Workflow parameter service.</param>
        /// <param name="workflowResourceRegistry">Workflow resource registry.</param>
        public WarehousePickingFileDataTransport(IWorkflowParameterService workflowParameterService,
            IWorkflowResourceRegistry workflowResourceRegistry) : base(workflowParameterService, workflowResourceRegistry)
        {
        }

        /// <summary>
        /// Fetches a JSON-encoded string from the workflow specific
        /// data JSON files.
        /// </summary>
        /// <returns>A Task to indicate the availabily of the JSON-encoded container instance.</returns>
        public Task<string> FetchWarehousePickingDTOAsync()
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
    }
}

