//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Threading.Tasks;

    // Extend the IWorkflowDataTransport to include the opertions required
    // of the warehouse picking module.
    public interface IWarehousePickingDataTransport 
    {
        string Name { get; }

        /// <summary>
        /// Fetches a JSON-encoded string that corresponds to an Warehouse Picking assignment list for the
        /// currently selected worker.
        /// </summary>
        /// <returns>A Task to indicate the availabily of the JSON-encoded container instance.</returns>
        Task<string> FetchWarehousePickingDTOAsync();

        /// <summary>
        /// Store the actual quantity picked of the product.
        /// </summary>
        /// <param name="pickIdentifier">The product identifier</param>
        /// <param name="quantity">The amount picked</param>
        /// <returns>A task to indicate when the operation is complete</returns>
        Task StorePickedQuantityAsync(string pickIdentifier, int quantity);
    }
}
