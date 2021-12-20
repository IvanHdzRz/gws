//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Logging;
    using GuidedWork;
    using Newtonsoft.Json;

    public class LAppDataService : ILAppDataService
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(LAppDataService));
        private readonly ILAppDataProxy _DataProxy;
        private readonly ILAppConfigRepository _LAppConfigRepository;

        public LAppDataService(ILAppDataProxy dataProxy, ILAppConfigRepository lappConfigRepository)
        {
            _DataProxy = dataProxy;
            _LAppConfigRepository = lappConfigRepository;
        }

        public void Initialize()
        {
            if (_DataProxy.DataTransport == null)
            {
                SelectTransport();
            }
            _DataProxy.DataTransport.Initialize();
        }

        #region ILAppDataService Implemenation

        public async Task<List<Company>> GetCompanies()
        {
            string companiesString = await _DataProxy.DataTransport.GetCompanies();

            var companies = JsonConvert.DeserializeObject<List<Company>>(companiesString);

            return companies;
        }

        public void ResetCredentials()
        {
            _DataProxy.DataTransport.ResetCredentials();
        }

        public Task<bool> ValidateCredentialsAsync(string userName, string password)
        {
            return _DataProxy.DataTransport.ValidateCredentialsAsync(userName, password);
        }

        public async Task<List<Warehouse>> GetWarehouses()
        {
            string warehousesString = await _DataProxy.DataTransport.GetWarehouses();

            var warehouses = JsonConvert.DeserializeObject<List<Warehouse>>(warehousesString);

            return warehouses;
        }

        public async Task<List<Zone>> GetZones()
        {
            string zonesString = await _DataProxy.DataTransport.GetZones();

            var zones = JsonConvert.DeserializeObject<List<Zone>>(zonesString);

            return zones;
        }

        public async Task<int> GetLicensePlateId(string licensePlateName)
        {
            string licensePlateIdString = await _DataProxy.DataTransport.GetLicensePlateId(licensePlateName);

            return int.Parse(licensePlateIdString);
        }

        public async Task<int> OpenTransactionAsync()
        {
            string transactionIdString = await _DataProxy.DataTransport.OpenTransactionAsync();

            return int.Parse(transactionIdString);
        }

       public Task CloseTransactionAsync(int transactionId)
        {
            return _DataProxy.DataTransport.CloseTransactionAsync(transactionId);
        }

        public async Task<List<int>> GetOrdersAsync(int warehouseId, int zoneId)
        {
            string lappOrdersString = await _DataProxy.DataTransport.GetOrdersAsync(warehouseId, zoneId);

            var orders = JsonConvert.DeserializeObject<List<int>>(lappOrdersString);

            return orders;
        }

        public async Task<PickRoute> GetPickTasksAsync(int warehouseId, int zoneId, int salesOrderId)
        {
            string lappPickRouteString = await _DataProxy.DataTransport.GetPickTasksAsync(warehouseId, zoneId, salesOrderId);

            if (string.IsNullOrEmpty(lappPickRouteString))
            {
                return null;
            }

            _Log.Debug($"#lappPickRouteString#: {lappPickRouteString}");

            var pickRoute = JsonConvert.DeserializeObject<PickRoute>(lappPickRouteString);

            return pickRoute;
        }

        public async Task<List<string>> GetBatchNumbersAsync(int locationId, int productId)
        {
            string batchNumbersString = await _DataProxy.DataTransport.GetBatchNumbersAsync(locationId, productId);

            var batchNumbers = JsonConvert.DeserializeObject<List<string>>(batchNumbersString);

            return batchNumbers;
        }

        public async Task<List<string>> GetSerialNumbersAsync(int locationId, int productId)
        {
            string serialNumbersString = await _DataProxy.DataTransport.GetSerialNumbersAsync(locationId, productId);

            var serialNumbers = JsonConvert.DeserializeObject<List<string>>(serialNumbersString);

            return serialNumbers;
        }

        public Task ConfirmPickTasksSerialAsync(int licensePlateId, int transactionId, int salesOrderId, int lineId, string serialNumber)
        {
            return _DataProxy.DataTransport.ConfirmPickTasksSerialAsync(licensePlateId, transactionId, salesOrderId, lineId, serialNumber);
        }

        public Task ConfirmPickTasksQuantityAsync(int licensePlateId, string batchNumber, int transactionId, int salesOrderId, int lineId, int quantityPicked)
        {
            return _DataProxy.DataTransport.ConfirmPickTasksQuantityAsync(licensePlateId, batchNumber, transactionId, salesOrderId, lineId, quantityPicked);
        }

        public Task SendPhotoAsync(int transactionId, string filePath)
        {
            return _DataProxy.DataTransport.SendPhotoAsync(transactionId, filePath);
        }

        #endregion

        private void SelectTransport()
        {
            string transportName = "FileDataTransport";
            bool serverSelected = _LAppConfigRepository.GetConfig("WorkflowFilterChoice")?.Value == LocalizationHelper.ServerLocalizationKey;

            if (serverSelected)
            {
                transportName = "RESTDataTransport";
            }

            _DataProxy.SelectTransport(transportName);
        }
    }
}
