//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using RESTCommunication;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Class providing the required REST methods for sending Warhouse Picking
    /// events.
    /// </summary>
    public class WarehousePickingRESTServiceProvider : IWarehousePickingRESTServiceProvider
    {
        private readonly IRESTService _RESTService;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:WarehousePicking.WarehousePickingRESTServiceProvider"/> class.
        /// </summary>
        /// <param name="RESTService">RESTS ervice.</param>
        public WarehousePickingRESTServiceProvider(IRESTService RESTService)
        {
            _RESTService = RESTService;
        }

        /// <summary>
        /// Fetches a JSON-encoded string that corresponds to a Warehouse Picking assignment list for a
        /// specified worker Id.
        /// </summary>
        /// <param name="siteId">The site Id.</param>
        /// <param name="workerId">A worker Id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task to indicate the availabily of the JSON-encoded container instance.</returns>
        public Task<string> FetchWarehousePickingDTOAsync(string siteId, string workerId, CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTGETAsync($"devicecomm/assignments/picking/{siteId}/{workerId}", false, cancellationToken);
        }

        /// <summary>
        /// Send the quantity picked for the associated pick identifer.
        /// </summary>
        /// <param name="pickIdentifier">The Id of the product</param>
        /// <param name="quantity">The actual quantity</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A Task to indicate when the operation is complete</returns>
        public Task StorePickedQuantityAsync(string pickIdentifier, int quantity, CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTGETAsync($"devicecomm/assignments/picking/result/{pickIdentifier}/{quantity}", true, cancellationToken);
        }
    }
}
