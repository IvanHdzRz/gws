//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ILAppDataService
    {
        void Initialize();

        Task<List<Company>> GetCompanies();

        void ResetCredentials();

        Task<bool> ValidateCredentialsAsync(string userName, string password);

        Task<List<Warehouse>> GetWarehouses();

        Task<List<Zone>> GetZones();

        Task<int> GetLicensePlateId(string licensePlateName);

        Task<int> OpenTransactionAsync();

        Task CloseTransactionAsync(int transactionId);

        Task<List<int>> GetOrdersAsync(int warehouseId, int zoneId);

        Task<PickRoute> GetPickTasksAsync(int warehouseId, int zoneId, int salesOrderId);

        Task<List<string>> GetBatchNumbersAsync(int locationId, int productId);

        Task<List<string>> GetSerialNumbersAsync(int locationId, int productId);

        //JeLo ConfirmPickTasksSerial(int transactionId, int orderId, int licensePlateId, int lineId, string serialNumber);
        Task ConfirmPickTasksSerialAsync(int licensePlateId, int transactionId, int salesOrderId, int lineId, string serialNumber);

        //JeLo ConfirmPickTasksQuantityAsync(int transactionId, int orderId, int licensePlateId, string batchNumber, int lineId, int quantityPicked);
        Task ConfirmPickTasksQuantityAsync(int licensePlateId, string batchNumber, int transactionId, int salesOrderId, int lineId, int quantityPicked);

        Task SendPhotoAsync(int transactionId, string filePath);
    }
}
