//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using RESTCommunication;
    using Retail;
    using System.Threading;
    using System.Threading.Tasks;

    public class OrderPickingRESTServiceProvider : IOrderPickingRESTServiceProvider
    {
        private readonly IRESTService _RESTService;

        public OrderPickingRESTServiceProvider(IRetailRESTService RESTService)
        {
            _RESTService = RESTService;
        }

        public Task<string> FetchOrderPickingDTOAsync(string siteId, string workerId, CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTGETAsync($"devicecomm/assignments/picking/{siteId}/{workerId}", false, cancellationToken);
        }

        public Task StorePickedQuantityAsync(string pickIdentifier, int quantity, CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTGETAsync($"devicecomm/assignments/picking/result/{pickIdentifier}/{quantity}", true, cancellationToken);
        }

        public Task StoreStagingLocationAsync(long orderId, string stagingLocation, CancellationToken cancellationToken = default)
        {
            var stagingInfo = "{\"stagingInfo\":{" +
                "\"orderId\":\"" + orderId.ToString() + "\"," +
                "\"stagingLocation\":\"" + stagingLocation + "\"" +
                "}}";

            return _RESTService.ExecuteRESTPOSTDataAsync("devicecomm/assignment/picking/stageOrder",
                                                         stagingInfo,
                                                         true,
                                                         cancellationToken);
        }
    }
}
