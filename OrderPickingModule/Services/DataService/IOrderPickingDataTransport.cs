//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Threading.Tasks;

    // Extend the IWorkflowDataTransport to include the opertions required
    // of the order picking module.
    public interface IOrderPickingDataTransport
    {
        string Name { get; }

        /// <summary>
        /// Fetches a JSON-encoded string that corresponds to an Order Picking assignment list for the
        /// currently selected worker.
        /// </summary>
        /// <returns>A Task to indicate the availabily of the JSON-encoded OrdersDTO instance.</returns>
        Task<string> FetchOrderPickingDTOAsync();

        /// <summary>
        /// Store the actual quantity picked of the product.
        /// </summary>
        /// <param name="pickIdentifier">The product identifier</param>
        /// <param name="quantity">The amount picked</param>
        /// <returns>A task to indicate when the operation is complete</returns>
        Task StorePickedQuantityAsync(string pickIdentifier, int quantity);

        /// <summary>
        /// Send the staging location for an order ID.
        /// </summary>
        /// <param name="orderId">The ID of the Order</param>
        /// <param name="stagingLocation">The staging location</param>
        /// <returns>A Task to indicate when the operation is complete</returns>
        Task StoreStagingLocationAsync(long orderId, string stagingLocation);
    }
}
