//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Threading.Tasks;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;
    using Retail;

    /// <summary>
    /// This class contains methods to make GET and POST requests to the middleware server. These
    /// methods can throw exceptions when network connectivity is poor that must be caught
    /// and handled at a higher level.
    /// </summary>
    public class OrderPickingRESTDataTransport : WorkflowRESTDataTransport, IOrderPickingDataTransport
    {
        private readonly IOrderPickingRESTServiceProvider _RestServiceProvider;
        private readonly IRetailConfigRepository _RetailConfigRepository;

        public OrderPickingRESTDataTransport(IOrderPickingRESTServiceProvider restServiceProvider,
            IRetailConfigRepository retailConfigRepository)
        {
            _RestServiceProvider = restServiceProvider;
            _RetailConfigRepository = retailConfigRepository;
        }

        /// <summary>
        /// Fetches a JSON-encoded string from the REST service that corresponds to an
        /// Order Picking assignment list for the currently selected worker.
        /// </summary>
        /// <returns>A Task to indicate the availabily of the JSON-encoded OrdersDTO instance.</returns>
        public Task<string> FetchOrderPickingDTOAsync()
        {
            return _RestServiceProvider.FetchOrderPickingDTOAsync(_RetailConfigRepository.GetConfig("SiteId").Value,
                                                                  _RetailConfigRepository.GetConfig("WorkerID").Value);
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

        public Task StoreStagingLocationAsync(long orderId, string stagingLocation)
        {
            return _RestServiceProvider.StoreStagingLocationAsync(orderId, stagingLocation);
        }
    }
}
