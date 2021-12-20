//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface defining the required REST methods for sending Warhouse Picking
    /// events.
    /// </summary>
    public interface IWarehousePickingRESTServiceProvider
    {
        /// <summary>
        /// Fetches a JSON-encoded string that corresponds to a Warehouse Picking assignment list for a
        /// specified worker Id.
        /// </summary>
        /// <param name="siteId">The site Id.</param>
        /// <param name="workerId">A worker Id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task to indicate the availabily of the JSON-encoded container instance.</returns>
        Task<string> FetchWarehousePickingDTOAsync(string siteId, string workerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send the quantity picked for the associated pick identifer.
        /// </summary>
        /// <param name="pickIdentifier">The Id of the product</param>
        /// <param name="quantity">The actual quantity</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A Task to indicate when the operation is complete</returns>
        Task StorePickedQuantityAsync(string pickIdentifier, int quantity, CancellationToken cancellationToken = default);
    }
}
