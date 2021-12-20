//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IOrderPickingRESTServiceProvider
    {
        /// <summary>
        /// Fetches a JSON-encoded string that corresponds to an Order Picking assignment list for a
        /// specified worker Id.
        /// </summary>
        /// <param name="siteId">The site Id.</param>
        /// <param name="workerId">A worker Id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task to indicate the availabily of the JSON-encoded OrdersDTO instance.</returns>
        Task<string> FetchOrderPickingDTOAsync(string siteId, string workerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send the quantity picked for the associated pick identifer.
        /// </summary>
        /// <param name="pickIdentifier">The Id of the product</param>
        /// <param name="quantity">The actual quantity</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A Task to indicate when the operation is complete</returns>
        Task StorePickedQuantityAsync(string pickIdentifier, int quantity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send the staging location for an order ID.
        /// </summary>
        /// <param name="orderId">The ID of the Order</param>
        /// <param name="stagingLocation">The staging location</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A Task to indicate when the operation is complete</returns>
        Task StoreStagingLocationAsync(long orderId, string stagingLocation, CancellationToken cancellationToken = default);
    }
}
