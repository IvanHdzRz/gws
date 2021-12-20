//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Threading.Tasks;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;

    /// <summary>
    /// This class contains methods to make GET and POST requests to the server server. These
    /// methods can throw exceptions when network connectivity is poor that must be caught
    /// and handled at a higher level.
    /// </summary>
    public class WarehousePickingRESTDataTransport : WorkflowRESTDataTransport, IWarehousePickingDataTransport
    {
        private readonly IWarehousePickingRESTServiceProvider _RestServiceProvider;
        private readonly IServerConfigRepository _ServerConfigRepository;

        public WarehousePickingRESTDataTransport(IWarehousePickingRESTServiceProvider restServiceProvider,
            IServerConfigRepository serverConfigRepository)
        {
            _RestServiceProvider = restServiceProvider;
            _ServerConfigRepository = serverConfigRepository;
        }

        /// <summary>
        /// Fetches a JSON-encoded string from the REST service that corresponds to an
        /// Warehouse Picking assignment list for the currently selected worker.
        /// </summary>
        /// <returns>A Task to indicate the availabily of the JSON-encoded OrdersDTO instance.</returns>
        public Task<string> FetchWarehousePickingDTOAsync()
        {
            return _RestServiceProvider.FetchWarehousePickingDTOAsync(_ServerConfigRepository.GetConfig("SiteId").Value,
                                                                  _ServerConfigRepository.GetConfig("WorkerID").Value);
        }

        /// <summary>
        /// Store the actual quantity picked of the product by sending a REST
        /// service request.
        /// </summary>
        /// <param name="pickIdentifier">The product identifier</param>
        /// <param name="quantity">The amount picked</param>
        /// <returns>A task to indicate when the operation is complete</returns>
        public Task StorePickedQuantityAsync(string pickIdentifier, int quantity)
        {
            return _RestServiceProvider.StorePickedQuantityAsync(pickIdentifier, quantity);
        }
    }
}
