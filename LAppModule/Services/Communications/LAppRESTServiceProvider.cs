//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class LAppRESTServiceProvider : ILAppRESTServiceProvider
    {
        private readonly ILAppRESTService _RESTService;
        private readonly ILAppRESTHeaderUtilities _RESTHeaderUtilities;

        public LAppRESTServiceProvider(ILAppRESTService RESTService,
            ILAppRESTHeaderUtilities RESTHeaderUtilities)
        {
            _RESTService = RESTService;
            _RESTHeaderUtilities = RESTHeaderUtilities;
        }

        public Task<string> GetCompanies(CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTGETAsync("Companies", false, cancellationToken);
        }

        public void ResetCredentials()
        {
            _RESTHeaderUtilities.ResetCredentials();
        }

        public Task<bool> ValidateCredentialsAsync(string userName, string password)
        {
            return _RESTHeaderUtilities.ValidateCredentialsAsync(userName, password);
        }

        public Task<string> GetWarehouses(CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTGETAsync("Warehouses", false, cancellationToken);
        }

        public Task<string> GetZones(CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTGETAsync("Zones", false, cancellationToken);
        }

        public Task<string> GetLicensePlateId(string licensePlateName, CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTPOSTAsync($"Licenseplate", false, cancellationToken);
        }

        public Task<string> OpenTransactionAsync(CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTGETAsync("SalesPickingTransfer", false, cancellationToken);
        }

        public Task CloseTransactionAsync(int transactionId, CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTPOSTAsync("CloseTransaction", false, cancellationToken);
        }

        public Task<string> GetOrdersAsync(int warehouseId, int zoneId, CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTGETAsync($"Orders", false, cancellationToken);
        }

        public Task<string> GetPickTasksAsync(int warehouseId, int zoneId, int salesOrderId, CancellationToken cancellationToken = default)
        {

            return _RESTService.ExecuteRESTPOSTAsync($"PickRoutes", false, cancellationToken);
        }

        public Task<string> GetBatchNumbersAsync(int locationId, int productId, CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTGETAsync($"BatchNumbers", false, cancellationToken);
        }

        public Task<string> GetSerialNumbersAsync(int locationId, int productId, CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTGETAsync($"SerialNumbers", false, cancellationToken);
        }

        public Task ConfirmPickTasksSerialAsync(int licensePlateId, int transactionId, int salesOrderId, int lineId, string serialNumber, CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTPOSTAsync($"ConfirmSerial({serialNumber})", false, cancellationToken);
        }

        public Task ConfirmPickTasksQuantityAsync(int licensePlateId, string batchNumber, int transactionId, int salesOrderId, int lineId, int quantityPicked, CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTPOSTAsync($"Confirm({quantityPicked})", false, cancellationToken);
        }

        public Task SendPhotoAsync(int transactionId, string filePath, CancellationToken cancellationToken = default)
        {
            var contentHeaders = new Dictionary<string, string>()
            {
                {"Content-Disposition", Path.GetFileName(filePath)}
            };

            return _RESTService.ExecuteRESTPOSTFileAsync($"Photos({transactionId})", filePath, false, cancellationToken, contentHeaders);
        }
    }
}
