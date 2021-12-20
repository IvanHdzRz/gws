//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Logging;
    using GuidedWork;
    using Newtonsoft.Json;

    public class BasePickingFileDataTransport : WorkflowFileDataTransport, IBasePickingDataTransport
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(BasePickingFileDataTransport));
        public Dictionary<string, List<string>> Responses;
        private readonly IConfigurationDataService _ConfigurationDataService;
        private readonly IBasePickingConfigRepository _BasePickingConfigRepository;

        public BasePickingFileDataTransport(IWorkflowParameterService workflowParameterService,
            IWorkflowResourceRegistry workflowResourceRegistry,
            IConfigurationDataService configurationDataService,
            IBasePickingConfigRepository basePickingConfigRepository) : base(workflowParameterService, workflowResourceRegistry)
        {
            _ConfigurationDataService = configurationDataService;
            _BasePickingConfigRepository = basePickingConfigRepository;
        }

        public void Initialize()
        {
        }

        public Task<string> SignOnAsync(string operatorId, string password)
        {
            return Task.FromResult(string.Empty);
        }

        public Task<string> SignOffAsync()
        {
            return Task.FromResult(string.Empty);
        }

        public Task<string> GetPicksAsync()
        {
            var picks = new List<Pick>
            {
                new Pick
                {
                    PickId = 0,
                    ProductSpokenVerification = "960",
                    ProductScannedVerification = "960",
                    ProductName = "Item 1",
                    Aisle = "1",
                    CheckDigits = "98",
                    ContainerPosition = 1,
                    ContainerScannedVerification = "867",
                    ContainerSpokenVerification = "867",
                    QuantityToPick = 8,
                    Slot = "16"
                },
                new Pick
                {
                    PickId = 1,
                    ProductSpokenVerification = "298",
                    ProductScannedVerification = "298",
                    ProductName = "Item 2",
                    Aisle = "1",
                    CheckDigits = "56",
                    ContainerPosition = 2,
                    ContainerScannedVerification = "666",
                    ContainerSpokenVerification = "666",
                    QuantityToPick = 3,
                    Slot = "17"
                },
                new Pick
                {
                    PickId = 2,
                    ProductSpokenVerification = "207",
                    ProductScannedVerification = "207",
                    ProductName = "Item 3",
                    Aisle = "1",
                    CheckDigits = "06",
                    ContainerPosition = 3,
                    ContainerScannedVerification = "080",
                    ContainerSpokenVerification = "080",
                    QuantityToPick = 6,
                    Slot = "8"
                },
                new Pick
                {
                    PickId = 3,
                    ProductSpokenVerification = "668",
                    ProductScannedVerification = "668",
                    ProductName = "Item 4",
                    Aisle = "1",
                    CheckDigits = "77",
                    ContainerPosition = 4,
                    ContainerScannedVerification = "049",
                    ContainerSpokenVerification = "049",
                    QuantityToPick = 11,
                    Slot = "15"
                },
                new Pick
                {
                    PickId = 4,
                    ProductSpokenVerification = "792",
                    ProductScannedVerification = "792",
                    ProductName = "Item 5",
                    Aisle = "1",
                    CheckDigits = "02",
                    ContainerPosition = 5,
                    ContainerScannedVerification = "974",
                    ContainerSpokenVerification = "974",
                    QuantityToPick = 4,
                    Slot = "7"
                },
                new Pick
                {
                    PickId = 5,
                    ProductSpokenVerification = "200",
                    ProductScannedVerification = "200",
                    ProductName = "Item 6",
                    Aisle = "2",
                    CheckDigits = "56",
                    ContainerPosition = 1,
                    ContainerScannedVerification = "441",
                    ContainerSpokenVerification = "441",
                    QuantityToPick = 13,
                    Slot = "16"
                },
                new Pick
                {
                    PickId = 6,
                    ProductSpokenVerification = "858",
                    ProductScannedVerification = "858",
                    ProductName = "Item 7",
                    Aisle = "2",
                    CheckDigits = "98",
                    ContainerPosition = 2,
                    ContainerScannedVerification = "235",
                    ContainerSpokenVerification = "235",
                    QuantityToPick = 8,
                    Slot = "14"
                },
                new Pick
                {
                    PickId = 7,
                    ProductSpokenVerification = "836",
                    ProductScannedVerification = "836",
                    ProductName = "Item 8",
                    Aisle = "2",
                    CheckDigits = "36",
                    ContainerPosition = 3,
                    ContainerScannedVerification = "724",
                    ContainerSpokenVerification = "724",
                    QuantityToPick = 4,
                    Slot = "12"
                },
                new Pick
                {
                    PickId = 8,
                    ProductSpokenVerification = "465",
                    ProductScannedVerification = "465",
                    ProductName = "Item 9",
                    Aisle = "2",
                    CheckDigits = "76",
                    ContainerPosition = 4,
                    ContainerScannedVerification = "771",
                    ContainerSpokenVerification = "771",
                    QuantityToPick = 11,
                    Slot = "13"
                },
                new Pick
                {
                    PickId = 9,
                    ProductSpokenVerification = "322",
                    ProductScannedVerification = "322",
                    ProductName = "Item 10",
                    Aisle = "2",
                    CheckDigits = "63",
                    ContainerPosition = 5,
                    ContainerScannedVerification = "346",
                    ContainerSpokenVerification = "346",
                    QuantityToPick = 1,
                    Slot = "6"
                },
                new Pick
                {
                    PickId = 10,
                    ProductSpokenVerification = "895",
                    ProductScannedVerification = "895",
                    ProductName = "Item 11",
                    Aisle = "3",
                    CheckDigits = "03",
                    ContainerPosition = 1,
                    ContainerScannedVerification = "708",
                    ContainerSpokenVerification = "708",
                    QuantityToPick = 11,
                    Slot = "4"
                },
                new Pick
                {
                    PickId = 11,
                    ProductSpokenVerification = "871",
                    ProductScannedVerification = "871",
                    ProductName = "Item 12",
                    Aisle = "3",
                    CheckDigits = "44",
                    ContainerPosition = 2,
                    ContainerScannedVerification = "850",
                    ContainerSpokenVerification = "850",
                    QuantityToPick = 8,
                    Slot = "11"
                },
                new Pick
                {
                    PickId = 12,
                    ProductSpokenVerification = "845",
                    ProductScannedVerification = "845",
                    ProductName = "Item 13",
                    Aisle = "3",
                    CheckDigits = "86",
                    ContainerPosition = 3,
                    ContainerScannedVerification = "794",
                    ContainerSpokenVerification = "794",
                    QuantityToPick = 5,
                    Slot = "3"
                },
                new Pick
                {
                    PickId = 13,
                    ProductSpokenVerification = "108",
                    ProductScannedVerification = "108",
                    ProductName = "Item 14",
                    Aisle = "3",
                    CheckDigits = "19",
                    ContainerPosition = 4,
                    ContainerScannedVerification = "534",
                    ContainerSpokenVerification = "534",
                    QuantityToPick = 10,
                    Slot = "15"
                },
                new Pick
                {
                    PickId = 14,
                    ProductSpokenVerification = "044",
                    ProductScannedVerification = "044",
                    ProductName = "Item 15",
                    Aisle = "3",
                    CheckDigits = "56",
                    ContainerPosition = 5,
                    ContainerScannedVerification = "139",
                    ContainerSpokenVerification = "139",
                    QuantityToPick = 10,
                    Slot = "2"
                },
                new Pick
                {
                    PickId = 15,
                    ProductSpokenVerification = "456",
                    ProductScannedVerification = "456",
                    ProductName = "Item 16",
                    Aisle = "4",
                    CheckDigits = "09",
                    ContainerPosition = 1,
                    ContainerScannedVerification = "459",
                    ContainerSpokenVerification = "459",
                    QuantityToPick = 11,
                    Slot = "13"
                },
                new Pick
                {
                    PickId = 16,
                    ProductSpokenVerification = "911",
                    ProductScannedVerification = "911",
                    ProductName = "Item 17",
                    Aisle = "4",
                    CheckDigits = "77",
                    ContainerPosition = 2,
                    ContainerScannedVerification = "916",
                    ContainerSpokenVerification = "916",
                    QuantityToPick = 6,
                    Slot = "20"
                },
                new Pick
                {
                    PickId = 17,
                    ProductSpokenVerification = "008",
                    ProductScannedVerification = "008",
                    ProductName = "Item 18",
                    Aisle = "4",
                    CheckDigits = "77",
                    ContainerPosition = 3,
                    ContainerScannedVerification = "003",
                    ContainerSpokenVerification = "003",
                    QuantityToPick = 8,
                    Slot = "16"
                },
                new Pick
                {
                    PickId = 18,
                    ProductSpokenVerification = "157",
                    ProductScannedVerification = "157",
                    ProductName = "Item 19",
                    Aisle = "4",
                    CheckDigits = "72",
                    ContainerPosition = 4,
                    ContainerScannedVerification = "071",
                    ContainerSpokenVerification = "071",
                    QuantityToPick = 5,
                    Slot = "18"
                },
                new Pick
                {
                    PickId = 19,
                    ProductSpokenVerification = "254",
                    ProductScannedVerification = "254",
                    ProductName = "Item 20",
                    Aisle = "4",
                    CheckDigits = "50",
                    ContainerPosition = 5,
                    ContainerScannedVerification = "947",
                    ContainerSpokenVerification = "947",
                    QuantityToPick = 2,
                    Slot = "7"
                },
                new Pick
                {
                    PickId = 20,
                    ProductSpokenVerification = "554",
                    ProductScannedVerification = "554",
                    ProductName = "Item 21",
                    Aisle = "5",
                    CheckDigits = "63",
                    ContainerPosition = 1,
                    ContainerScannedVerification = "343",
                    ContainerSpokenVerification = "343",
                    QuantityToPick = 3,
                    Slot = "16"
                },
                new Pick
                {
                    PickId = 21,
                    ProductSpokenVerification = "761",
                    ProductScannedVerification = "761",
                    ProductName = "Item 22",
                    Aisle = "5",
                    CheckDigits = "85",
                    ContainerPosition = 2,
                    ContainerScannedVerification = "029",
                    ContainerSpokenVerification = "029",
                    QuantityToPick = 10,
                    Slot = "14"
                },
                new Pick
                {
                    PickId = 22,
                    ProductSpokenVerification = "587",
                    ProductScannedVerification = "587",
                    ProductName = "Item 23",
                    Aisle = "5",
                    CheckDigits = "57",
                    ContainerPosition = 3,
                    ContainerScannedVerification = "236",
                    ContainerSpokenVerification = "236",
                    QuantityToPick = 4,
                    Slot = "6"
                },
                new Pick
                {
                    PickId = 23,
                    ProductSpokenVerification = "189",
                    ProductScannedVerification = "189",
                    ProductName = "Item 24",
                    Aisle = "5",
                    CheckDigits = "66",
                    ContainerPosition = 4,
                    ContainerScannedVerification = "253",
                    ContainerSpokenVerification = "253",
                    QuantityToPick = 7,
                    Slot = "1"
                },
                new Pick
                {
                    PickId = 24,
                    ProductSpokenVerification = "469",
                    ProductScannedVerification = "469",
                    ProductName = "Item 25",
                    Aisle = "5",
                    CheckDigits = "32",
                    ContainerPosition = 5,
                    ContainerScannedVerification = "057",
                    ContainerSpokenVerification = "057",
                    QuantityToPick = 11,
                    Slot = "13"
                }
            };
            return Task.FromResult(JsonConvert.SerializeObject(picks));
        }

        public Task<string> UpdatePickAsync(long pickId, int quantityPicked)
        {
            return Task.FromResult(string.Empty);
        }
    }
}
