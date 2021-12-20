//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ILAppRESTServiceProvider
    {
        Task<string> GetCompanies(CancellationToken cancellationToken = default);

        void ResetCredentials();

        Task<bool> ValidateCredentialsAsync(string userName, string password);

        Task<string> GetWarehouses(CancellationToken cancellationToken = default);

        Task<string> GetZones(CancellationToken cancellationToken = default);

        Task<string> GetLicensePlateId(string licensePlateName, CancellationToken cancellationToken = default);

        Task<string> OpenTransactionAsync(CancellationToken cancellationToken = default);

        Task CloseTransactionAsync(int transactionId, CancellationToken cancellationToken = default);

        Task<string> GetOrdersAsync(int warehouseId, int zoneId, CancellationToken cancellationToken = default);

        Task<string> GetPickTasksAsync(int warehouseId, int zoneId, int salesOrderId, CancellationToken cancellationToken = default);

        Task<string> GetBatchNumbersAsync(int locationId, int productId, CancellationToken cancellationToken = default);

        Task<string> GetSerialNumbersAsync(int locationId, int productId, CancellationToken cancellationToken = default);

        Task ConfirmPickTasksSerialAsync(int licensePlateId, int transactionId, int salesOrderId, int lineId, string serialNumber, CancellationToken cancellationToken = default);

        Task ConfirmPickTasksQuantityAsync(int licensePlateId, string batchNumber, int transactionId, int salesOrderId, int lineId, int quantityPicked, CancellationToken cancellationToken = default);

        Task SendPhotoAsync(int transactionId, string filePath, CancellationToken cancellationToken = default);
    }
}
