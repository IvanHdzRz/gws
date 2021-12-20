//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System.Threading.Tasks;

    // Extend the IWorkflowDataTransport to include the opertions required of the Business One module.
    public interface ILAppDataTransport 
    {
        string Name { get; }

        void Initialize();

        Task<string> GetCompanies();

        void ResetCredentials();

        Task<bool> ValidateCredentialsAsync(string userName, string password);

        Task<string> GetWarehouses();

        Task<string> GetZones();

        Task<string> GetLicensePlateId(string licensePlateName);

        Task<string> OpenTransactionAsync();

        Task CloseTransactionAsync(int transactionId);

        Task<string> GetOrdersAsync(int warehouseId, int zoneId);

        Task<string> GetPickTasksAsync(int warehouseId, int zoneId, int salesOrderId);

        Task<string> GetBatchNumbersAsync(int locationId, int productId);

        Task<string> GetSerialNumbersAsync(int locationId, int productId);

        Task ConfirmPickTasksSerialAsync(int licensePlateId, int transactionId, int salesOrderId, int lineId, string serialNumber);

        Task ConfirmPickTasksQuantityAsync(int licensePlateId, string batchNumber, int transactionId, int salesOrderId, int lineId, int quantityPicked);

        Task SendPhotoAsync(int transactionId, string filePath);
    }
}
