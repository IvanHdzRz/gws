//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using Common.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using SQLite;
    using GuidedWork;

    public class ReceivingDataService : IReceivingDataService
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(ReceivingDataService));

        private readonly IDataService _DataService;
        private readonly IReceivingDataProxy _DataProxy;
        private readonly IDatabaseContainer _DbContainer;
        readonly IImageService _ImageService;

        private readonly List<Type> _TableTypes = new List<Type>
        {
            typeof(ReceivingWorkItem)
        };

        public ReceivingDataService(IDataService dataService, IReceivingDataProxy dataProxy, IDatabaseContainer databaseContainer, IAppStateService appStateService, IImageService imageService)
        {
            _DataService = dataService;
            _DataProxy = dataProxy;
            _DbContainer = databaseContainer;
            _ImageService = imageService;
            appStateService.FirstPriorityResetEventAsync += ResetAsync;
        }

        #region IReceivingDataService Implementation

        public List<ReceivingWorkItem> GetAllWorkItems()
        {
            lock (_DbContainer.DatabaseLock)
            {
                //TODO: there may be a better (more efficient) way to do this in LINQ

                return _DbContainer.Database.Query<ReceivingWorkItem>(
                    "select * " +
                    "from ReceivingWorkItem " +
                    "order by Sequence").ToList();
            }
        }

        public List<ReceivingWorkItem> GetAllCurrentAndUpcomingWorkItems()
        {
            lock (_DbContainer.DatabaseLock)
            {
                //TODO: there may be a better (more efficient) way to do this in LINQ

                //disclaimer - the ReceivingWorkItem objects returned from this query will only have
                //the ProductID and RequestedQuantity fields populated - others will be null due to the select statement
                return _DbContainer.Database.Query<ReceivingWorkItem>(
                    "select ProductIdentifier, RequestedQuantity, ReceivedQuantity, Damaged " +
                    "from ReceivingWorkItem " +
                    "where Completed = 0 " +
                    "order by Sequence").ToList();
            }
        }

        public Product GetProduct(string id)
        {
            return _DataService.GetProduct(id);
        }

        public async Task GetReceivingWorkItemsAsync()
        {
            string receivingDTOJson = await _DataProxy.DataTransport.FetchReceivingDTOAsync();
            var receivingDTO =
                TracingJsonConvert.DeserializeRequiringAllMembers<ReceivingDTO>(receivingDTOJson, _Log);

            //check if there are no load to cart assignments
            if (receivingDTO.ReceivingAssignments != null)
            {
                //implicit conversions from List<ReceivingAssignmentDTO> to List<ReceivingWorkItem>
                List<ReceivingWorkItem> receivingWorkItems =
                    receivingDTO.ReceivingAssignments.Assignments.Select<ReceivingAssignmentDTO, ReceivingWorkItem>(x => x).ToList();
                FixupReceivingWorkItemImages(receivingWorkItems);

                List<Product> products =
                    receivingDTO.ReceivingAssignments.Assignments.Select<ReceivingAssignmentDTO, Product>(x => x).ToList();

                lock (_DbContainer.DatabaseLock)
                {
                    UpdateDataForReceiving(receivingWorkItems, products);
                }
            }
        }

        public List<ReceivingWorkItem> GetMatchingReceivingWorkItems(string productIdOrNumber)
        {
            var matchingWorkItems = new List<ReceivingWorkItem>();
            lock (_DbContainer.DatabaseLock)
            {
                var receivingWorkItems = _DbContainer.Database.Table<ReceivingWorkItem>().Where(lwi => lwi.Completed == false).ToList();
                foreach (var item in receivingWorkItems)
                {
                    //check product identifier first
                    if (IsSmallStringFoundInTailOfBigString(productIdOrNumber, item.ProductIdentifier))
                    {
                        matchingWorkItems.Add(item);
                    }
                    else
                    {
                        //iterate through product numbers now and do same comparison since product identifier didnt match
                        foreach (var productNumber in item.ProductNumbersString.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (IsSmallStringFoundInTailOfBigString(productIdOrNumber, productNumber))
                            {
                                matchingWorkItems.Add(item);
                                //assuming we can only match 1 product number per product
                                break;
                            }
                        }
                    }
                }
            }

            return matchingWorkItems;
        }

        public ReceivingWorkItem GetNextWorkItem()
        {
            return _DataService.GetNextWorkItem<ReceivingWorkItem>();
        }

        public int GetNumCompletedWorkItems()
        {
            return _DataService.GetNumCompletedWorkItems<ReceivingWorkItem>();
        }

        public int GetNumWorkItems()
        {
            return _DataService.GetNumWorkItems<ReceivingWorkItem>();
        }

        public void UpdateWorkItem(ReceivingWorkItem workItem)
        {
            lock (_DbContainer.DatabaseLock)
            {
                _DbContainer.Database.Update(workItem);
            }
        }

        public void SetWorkItemSkipped(ReceivingWorkItem workItem)
        {
            _DataService.SetWorkItemSkipped(workItem);
        }

        public Task SetWorkItemCompleteAsync(ReceivingWorkItem workItem)
        {
            return _DataService.SetWorkItemCompleteAsync(workItem);
        }

        #endregion

        #region ReceivingDataService Database Helpers

        void FixupReceivingWorkItemImages(List<ReceivingWorkItem> receivingWorkItems)
        {
            var productImageNames = new List<string>();
            foreach (var receivingWorkItem in receivingWorkItems)
            {
                if (null != receivingWorkItem && null != receivingWorkItem.ProductImageName)
                {
                    productImageNames.Add(receivingWorkItem.ProductImageName);
                }
            }

            _DataService.RetrieveFilesForImageNamesFromProductImagesFireAndForget(productImageNames);
            foreach (var receivingWorkItem in receivingWorkItems)
            {
                if (null != receivingWorkItem && null != receivingWorkItem.ProductImageName)
                {
                    receivingWorkItem.ProductImagePath = _ImageService.GetProductImagePath(receivingWorkItem.ProductIdentifier);
                }
            }
        }

        void UpdateDataForReceiving(List<ReceivingWorkItem> receivingWorkItems, List<Product> products)
        {
            var addedUniqueIds = new List<long>();

            // Add updated receiving work items
            foreach (var receivingWorkItem in receivingWorkItems)
            {
                var uniqueID = receivingWorkItem.ID;

                // Only insert the objects into the database if the receiving work item wasn't started
                // and doesn't already exist. If it already exists that means the status wasn't updated in
                // the middle ware.
                if (!receivingWorkItem.InProgress &&
                    !receivingWorkItem.Completed &&
                    !_DbContainer.Database.Table<ReceivingWorkItem>().Where(lwi => lwi.ID == uniqueID).Any())
                {
                    _DbContainer.Database.Insert(receivingWorkItem);

                    addedUniqueIds.Add(uniqueID);
                }
            }

            foreach (var product in products)
            {
                _DbContainer.Database.InsertOrReplace(product);
            }
        }

        //TODO: figure out a better way with this method since it is used
        //by other workflows in the DataService
        bool IsSmallStringFoundInTailOfBigString(string smallString, string bigString)
        {
            string substring = bigString.Substring(Math.Max(0, bigString.Length - smallString.Length));
            return smallString == substring;
        }

        Task ResetAsync()
        {
            _Log.Debug(m => m("Removing and creating database tables"));
            RemoveTables();
            CreateTables();
            _Log.Debug(m => m("Database table reset complete"));
            return Task.CompletedTask;
        }

        void CreateTables()
        {
            lock (_DbContainer.DatabaseLock)
            {
                foreach (var type in _TableTypes)
                {
                    _DbContainer.Database.CreateTable(type);
                }
            }
        }

        void RemoveTables()
        {
            lock (_DbContainer.DatabaseLock)
            {
                foreach (var type in _TableTypes)
                {
                    _DbContainer.Database.DropTable(new TableMapping(type));
                }
            }
        }
        #endregion
    }
}
