//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System.Threading.Tasks;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;

    /// <summary>
    /// This class contains methods to make GET and POST requests to the server server. These
    /// methods can throw exceptions when network connectivity is poor that must be caught
    /// and handled at a higher level.
    /// </summary>
    public class LAppRESTDataTransport : WorkflowRESTDataTransport, ILAppDataTransport
    {
        private readonly ILAppRESTServiceProvider _RestServiceProvider;
        private readonly ITimeoutHandler _RESTTimeoutHandler;

        public LAppRESTDataTransport(ILAppRESTServiceProvider restServiceProvider, ITimeoutHandler restTimeoutHandler)
        {
            _RestServiceProvider = restServiceProvider;
            _RESTTimeoutHandler = restTimeoutHandler;
        }

        public void Initialize()
        {
        }

        public Task<string> GetCompanies()
        {
            return _RestServiceProvider.GetCompanies(_RESTTimeoutHandler.GetTimeoutToken());
        }

        public void ResetCredentials()
        {
            _RestServiceProvider.ResetCredentials();
        }

        public Task<bool> ValidateCredentialsAsync(string userName, string password)
        {
            return _RestServiceProvider.ValidateCredentialsAsync(userName, password);
        }

        public Task<string> GetWarehouses()
        {
            return _RestServiceProvider.GetWarehouses(_RESTTimeoutHandler.GetTimeoutToken());
        }

        public Task<string> GetZones()
        {
            return _RestServiceProvider.GetZones(_RESTTimeoutHandler.GetTimeoutToken());
        }

        public Task<string> GetLicensePlateId(string licensePlateName)
        {
            return _RestServiceProvider.GetLicensePlateId(licensePlateName, _RESTTimeoutHandler.GetTimeoutToken());
        }

        public Task<string> OpenTransactionAsync()
        {
            return _RestServiceProvider.OpenTransactionAsync(_RESTTimeoutHandler.GetTimeoutToken());
        }

        public Task CloseTransactionAsync(int transactionId)
        {
            return _RestServiceProvider.CloseTransactionAsync(transactionId, _RESTTimeoutHandler.GetTimeoutToken());
        }

        public Task<string> GetOrdersAsync(int warehouseId, int zoneId)
        {
            return _RestServiceProvider.GetOrdersAsync(warehouseId, zoneId, _RESTTimeoutHandler.GetTimeoutToken());
        }

        public Task<string> GetPickTasksAsync(int warehouseId, int zoneId, int salesOrderId)
        {
            return _RestServiceProvider.GetPickTasksAsync(warehouseId, zoneId, salesOrderId, _RESTTimeoutHandler.GetTimeoutToken());
        }

        public Task<string> GetBatchNumbersAsync(int locationId, int productId)
        {
            return _RestServiceProvider.GetBatchNumbersAsync(locationId, productId, _RESTTimeoutHandler.GetTimeoutToken());
        }

        public Task<string> GetSerialNumbersAsync(int locationId, int productId)
        {
            return _RestServiceProvider.GetSerialNumbersAsync(locationId, productId, _RESTTimeoutHandler.GetTimeoutToken());
        }

        public Task ConfirmPickTasksSerialAsync(int licensePlateId, int transactionId, int salesOrderId, int lineId, string serialNumber)
        {
            return _RestServiceProvider.ConfirmPickTasksSerialAsync(licensePlateId, transactionId, salesOrderId, lineId, serialNumber, _RESTTimeoutHandler.GetTimeoutToken());
        }

        public Task ConfirmPickTasksQuantityAsync(int licensePlateId, string batchNumber, int transactionId, int salesOrderId, int lineId, int quantityPicked)
        {
            return _RestServiceProvider.ConfirmPickTasksQuantityAsync(licensePlateId, batchNumber, transactionId, salesOrderId, lineId, quantityPicked, _RESTTimeoutHandler.GetTimeoutToken());
        }

        public Task SendPhotoAsync(int transactionId, string filePath)
        {
            return _RestServiceProvider.SendPhotoAsync(transactionId, filePath, _RESTTimeoutHandler.GetTimeoutToken());
        }
    }
}
