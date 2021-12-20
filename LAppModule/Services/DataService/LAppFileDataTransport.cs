//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GuidedWork;
    using Newtonsoft.Json;

    public class LAppFileDataTransport : WorkflowFileDataTransport, ILAppDataTransport
    {
        private int pickRouteCount = 0;

        public LAppFileDataTransport(IWorkflowParameterService workflowParameterService,
            IWorkflowResourceRegistry workflowResourceRegistry) : base(workflowParameterService, workflowResourceRegistry)
        {
        }

        public void Initialize()
        {
        }

        public Task<string> GetCompanies()
        {
            var company = new List<Company>
            {
                new Company
                {
                    Id = 1,
                    DatabaseName = "Honeywell"
                }
            };

            return Task.FromResult(JsonConvert.SerializeObject(company));
        }

        public void ResetCredentials()
        {
        }

        public Task<bool> ValidateCredentialsAsync(string userName, string password)
        {
            return Task.FromResult(true);
        }

        public Task<string> GetWarehouses()
        {
            var warehouseList = new List<Warehouse>
            {
                new Warehouse
                {
                    Id = 1,
                    Code = "05",
                    Name = "Bin Warehouse"
                },

                new Warehouse
                {
                    Id = 5,
                    Code = "04",
                    Name = "Consignment Warehouse"
                },

                new Warehouse
                {
                    Id = 4,
                    Code = "03",
                    Name = "Dropship Warehouse"
                },

                new Warehouse
                {
                    Id = 2,
                    Code = "01",
                    Name = "General Warehouse"
                },

                new Warehouse
                {
                    Id = 3,
                    Code = "02",
                    Name = "West Coast Warehouse"
                }
            };

            return Task.FromResult(JsonConvert.SerializeObject(warehouseList));
        }

        public Task<string> GetZones()
        {
            var zoneList = new List<Zone>
            {
                new Zone
                {
                    Id = 2,
                    Name = "Dry"
                },

                new Zone
                {
                    Id = 1,
                    Name = "Freezer"
                }
            };

            return Task.FromResult(JsonConvert.SerializeObject(zoneList));
        }

        public Task<string> GetLicensePlateId(string licensePlateName)
        {
            return Task.FromResult("45");
        }

        public Task<string> OpenTransactionAsync()
        {
            return Task.FromResult("6");
        }

        public Task CloseTransactionAsync(int transactionId)
        {
            return Task.CompletedTask;
        }

        public Task<string> GetOrdersAsync(int warehouseId, int zoneId)
        {
            // reset the pick route index
            pickRouteCount = 0;

            var orders = new List<int>
            {
                451,
                452,
                453,
                454,
                455,
                456,
                457,
                458,
                459,
                460,
                461,
                462,
                463,
                464,
                465
            };

            return Task.FromResult(JsonConvert.SerializeObject(orders));
        }

        public Task<string> GetPickTasksAsync(int warehouseId, int zoneId, int salesOrderId)
        {
            PickRoute pickRoute = null;

            switch (pickRouteCount)
            {
                case 0:
                    pickRoute = new PickRoute
                    {
                        ProductId = 2,
                        LineId = 0,
                        LocationId = 2132,
                        ProductCode = "A00002",
                        ProductName = "J.B. Officeprint 1420",
                        QtyToPick = 4,
                        LocationName = "A012",
                        BatchNumbers = new List<string>(),
                        SerialNumbers = new List<string>(),
                        OrderId = 460,
                        UOM = "",
                        CustomerName = "Maxi-Teq",
                        LocationCheckDigit = "55",
                        ProductCheckDigit = "20",
                    };
                    break;
                case 1:
                    pickRoute = new PickRoute
                    {
                        ProductId = 2077,
                        LineId = 1,
                        LocationId = 2132,
                        ProductCode = "A00005",
                        ProductName = "Rainbow Color Printer 7.5",
                        QtyToPick = 1,
                        LocationName = "A012",
                        BatchNumbers = new List<string>
                        {
                            "BATCH1",
                            "BATCH2",
                            "BATCH3",
                            "B1234"
                        },
                        SerialNumbers = new List<string>(),
                        OrderId = 460,
                        UOM = "",
                        CustomerName = "Maxi-Teq",
                        LocationCheckDigit = "55",
                        ProductCheckDigit = "40"
                    };
                    break;
                case 2:
                    pickRoute = new PickRoute
                    {
                        ProductId = 2078,
                        LineId = 2,
                        LocationId = 2133,
                        ProductCode = "A00003",
                        ProductName = "J.B. Officeprint 1186",
                        QtyToPick = 1,
                        LocationName = "A013",
                        BatchNumbers = new List<string>(),
                        SerialNumbers = new List<string>
                        {
                            "SERIAL",
                            "SERIAL1",
                            "SERIAL2",
                            "SERIAL3",
                            "SERIAL4",
                            "SERIAL5",
                            "SERIAL6",
                            "SERIAL7",
                            "SERIAL8",
                            "SERIAL9",
                            "SERIAL10",
                            "SERIAL11",
                            "SERIAL12",
                            "SERIAL13",
                            "SERIAL14",
                            "SERIAL15",
                            "SERIAL16",
                            "SERIAL17",
                            "SERIAL18",
                            "SERIAL19",
                            "SERIAL20",
                            "SERIAL21",
                            "SERIAL22",
                            "SERIAL23",
                            "SERIAL24",
                            "SERIAL25",
                            "SERIAL26",
                            "SERIAL27",
                            "SERIAL28",
                            "SERIAL29",
                            "S1234"
                        },
                        OrderId = 460,
                        UOM = "",
                        CustomerName = "Maxi-Teq",
                        LocationCheckDigit = "05",
                        ProductCheckDigit = "80"
                    };
                    break;
                case 3:
                    pickRouteCount = 0;
                    return Task.FromResult<string>(null);
            }

            pickRouteCount++;

            return Task.FromResult(JsonConvert.SerializeObject(pickRoute));
        }

        public Task<string> GetBatchNumbersAsync(int locationId, int productId)
        {
            var batchNumbers = new List<string>
            {
                "BATCH1",
                "BATCH2",
                "BATCH3",
                "B1234"
            };

            return Task.FromResult(JsonConvert.SerializeObject(batchNumbers));
        }

        public Task<string> GetSerialNumbersAsync(int locationId, int productId)
        {
            var serialNumbers = new List<string>
            {
                "SERIAL",
                "SERIAL1",
                "SERIAL2",
                "SERIAL3",
                "SERIAL4",
                "SERIAL5",
                "SERIAL6",
                "SERIAL7",
                "SERIAL8",
                "SERIAL9",
                "SERIAL10",
                "SERIAL11",
                "SERIAL12",
                "SERIAL13",
                "SERIAL14",
                "SERIAL15",
                "SERIAL16",
                "SERIAL17",
                "SERIAL18",
                "SERIAL19",
                "SERIAL20",
                "SERIAL21",
                "SERIAL22",
                "SERIAL23",
                "SERIAL24",
                "SERIAL25",
                "SERIAL26",
                "SERIAL27",
                "SERIAL28",
                "SERIAL29",
                "S1234"
            };

            return Task.FromResult(JsonConvert.SerializeObject(serialNumbers));
        }

        public Task ConfirmPickTasksSerialAsync(int licensePlateId, int transactionId, int salesOrderId, int lineId, string serialNumber)
        {
            return Task.CompletedTask;
        }

        public Task ConfirmPickTasksQuantityAsync(int licensePlateId, string batchNumber, int transactionId, int salesOrderId, int lineId, int quantityPicked)
        {
            return Task.CompletedTask;
        }

        public Task SendPhotoAsync(int transactionId, string filePath)
        {
            return Task.CompletedTask;
        }
    }
}
