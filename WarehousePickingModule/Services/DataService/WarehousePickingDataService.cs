//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using Common.Logging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GuidedWork;

    /// <summary>
    /// A class that operatess on the data store of work items, whether remote
    /// or local.
    /// </summary>
    public class WarehousePickingDataService : IWarehousePickingDataService
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(WarehousePickingDataService));

        private readonly IDataService _DataService;
        private readonly IWarehousePickingDataProxy _DataProxy;

        private readonly List<WarehousePickingWorkItem> _WorkItems = new List<WarehousePickingWorkItem>();
        private readonly List<Product> _Products = new List<Product>();
        private readonly List<ProductLocation> _ProductLocations = new List<ProductLocation>();
        private readonly List<LocationDescriptor> _LocationDescriptors = new List<LocationDescriptor>();

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:WarehousePicking.WarehousePickingDataService"/> class.
        /// </summary>
        /// <param name="dataService">Data service.</param>
        /// <param name="dataProxy">Data proxy.</param>
        /// <param name="appStateService">App state service.</param>
        public WarehousePickingDataService(IDataService dataService, IWarehousePickingDataProxy dataProxy, IAppStateService appStateService)
        {
            _DataService = dataService;
            _DataProxy = dataProxy;
            appStateService.FirstPriorityResetEventAsync += ResetAsync;
        }

        /// <summary>
        /// Retrieves a list of all the WarehousePickingWorkItem objects in this trip
        /// </summary>
        /// <returns> List of WarehousePickingWorkItem objects that fit the criteria in the database query</returns>
        #region IWarehousePickingDataService Implemenation

        public List<WarehousePickingWorkItem> GetAllWorkItems()
        {
            return _WorkItems.OrderBy(wi => wi.Sequence).ToList();
        }

        /// <summary>
        /// Retrieves a list of all the WarehousePickingWorkItem objects that have not been completed
        /// </summary>
        /// <returns> List of WarehousePickingWorkItem objects that fit the criteria in the database query</returns>
        public List<WarehousePickingWorkItem> GetAllCurrentAndUpcomingWorkItems()
        {
            return _WorkItems.Where(wi => wi.Completed == false).OrderBy(wi => wi.Sequence).ToList();
        }

        /// <summary>
        /// Gets the total quantity remaining to be picked.
        /// </summary>
        /// <returns>Total quantity remaining</returns>
        public int GetTotalQuantityRemaining()
        {
            return _WorkItems.Where(wi => wi.Completed == false).Sum(wi => wi.PickQuantity);
        }

        /// <summary>
        /// Gets the total quantity that has been picked.
        /// </summary>
        /// <returns></returns>
        public int GetTotalQuantityPicked()
        {
            return _WorkItems.Where(wi => wi.Completed).Sum(wi => wi.PickedQuantity);
        }

        private List<LocationDescriptor> GetLocationDescriptorsForLocationId(long locationID)
        {
            return _LocationDescriptors.Where(locationDescriptor => locationDescriptor.LocationID == locationID).
                OrderBy(locationDescriptor => locationDescriptor.DescOrder).ToList();
        }

        /// <summary>
        /// Gets the list of (ordered) location descriptors for a Product ID.
        /// </summary>
        /// <returns>The location descriptors.</returns>
        /// <param name="productID">Product ID.</param>
        public List<List<LocationDescriptor>> GetLocationDescriptorsListForProductId(long productID)
        {
            var listOfLocationDescriptorLists = new List<List<LocationDescriptor>>();
            var productLocationsQuery = _ProductLocations.Where(productLocation => productLocation.ProductID == productID);

            foreach (var productLocation in productLocationsQuery)
            {
                listOfLocationDescriptorLists.Add(GetLocationDescriptorsForLocationId(productLocation.LocationID));
            }
            return listOfLocationDescriptorLists;
        }

        /// <summary>
        /// Gets the next work item.
        /// </summary>
        /// <returns>The next work item.</returns>
        public WarehousePickingWorkItem GetNextWorkItem()
        {
            var sequenceQuery = _WorkItems.Where(wi => wi.Completed == false).OrderBy(wi => wi.Sequence);
            return sequenceQuery.Any() ? sequenceQuery.FirstOrDefault() : null;
        }

        /// <summary>
        /// Returns the number of completed work items specific to this workflow (WarehousePickingWorkItems)
        /// </summary>
        /// <returns>The number completed work items.</returns>
        public int GetNumCompletedWorkItems()
        {
            return _WorkItems.Count(wi => wi.Completed);
        }

        /// <summary>
        /// Returns the number of work items specific to this workflow (WarehousePickingWorkItem)
        /// </summary>
        /// <returns>The number work items.</returns>
        public int GetNumWorkItems()
        {
            return _WorkItems.Count;
        }

        /// <summary>
        /// Retrieves Picks for the current worker and stores them locally.
        /// </summary>
        /// <returns>
        /// A task to represent the asynchronous operation.
        /// </returns>
        public async Task GetPicksAsync()
        {
            string WarehousePickingDTOJson = await _DataProxy.DataTransport.FetchWarehousePickingDTOAsync().ConfigureAwait(false);
            var WarehousePickingDTO = TracingJsonConvert.DeserializeRequiringAllMembers<WarehousePickingDTO>(WarehousePickingDTOJson, _Log);
            var WarehousePickingWorkItemContainers = GetWarehousePickingWorkItemContainersFromWarehousePickingDTO(WarehousePickingDTO);

            var productHashSet = new HashSet<string>();
            foreach (var container in WarehousePickingWorkItemContainers.Where(opwic => !opwic.WorkItem.Completed))
            {
                productHashSet.UnionWith(container.Products.Select(product => product.ProductIdentifier));
            }
            _DataService.RetrieveFilesForProductIdentifiersFromProductImagesFireAndForget(productHashSet.ToList());

            UpdateDataForWarehousePicking(WarehousePickingWorkItemContainers);
        }

        /// <summary>
        /// Reset the embedded data so that the assignment can be executed again.
        /// </summary>
        public void ResetPicks()
        {
            _WorkItems.Clear();
            _Products.Clear();
            _ProductLocations.Clear();
            _LocationDescriptors.Clear();
        }

        /// <summary>
        /// Gets the product for a given ID.
        /// </summary>
        /// <returns>The product.</returns>
        /// <param name="id">The Id of the database row.</param>
        public Product GetProduct(long id)
        {
            return _Products.FirstOrDefault(product => product.ID == id);
        }

        /// <summary>
        /// Set the specific work item to completed.
        /// </summary>
        /// <returns>
        /// A task to represent the asynchronous operation.
        /// </returns>
        /// <param name="workItem">The specific work item.</param>
        public Task SetWorkItemCompleteAsync(WarehousePickingWorkItem workItem)
        {
            workItem.Completed = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Set the specific work item to in progress.
        /// </summary>
        /// <returns>
        /// A task to represent the asynchronous operation.
        /// </returns>
        /// <param name="workItem">The specific work item.</param>
        public Task SetWorkItemInProgressAsync(WarehousePickingWorkItem workItem)
        {
            workItem.InProgress = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Set the specific work item to skipped.  This in
        /// effect modifies its sequence order to be last in the assignment.
        /// </summary>
        /// <param name="workItem">The specific work item.</param>
        public void SetWorkItemSkipped(WarehousePickingWorkItem workItem)
        {
            int maxSequence = _WorkItems.Max(wi => wi.Sequence);
            workItem.Sequence = maxSequence + 1;
        }

        /// <summary>
        /// Store the picked quantity in the data service and notify the proxy.
        /// </summary>
        /// <param name="pickIdentifier">The id of the pick whose quantity you want to store</param>
        /// <param name="quantity">The quantity picked.</param>
        /// <returns></returns>
        public Task StorePickedQuantityAsync(string pickIdentifier, int quantity)
        {
            return _DataProxy.DataTransport.StorePickedQuantityAsync(pickIdentifier, quantity);
        }

        #endregion

        #region WarehousePickingDataService Database Helpers

        //TODO: Move away from using these private class "containers" for inserting things
        //into the database
        class WarehousePickingWorkItemContainer
        {
            public List<Product> Products;
            public List<ProductLocation> ProductLocations;
            public List<LocationDescriptor> LocationDescriptors;
            public WarehousePickingWorkItem WorkItem;
        }

        IReadOnlyList<WarehousePickingWorkItemContainer> GetWarehousePickingWorkItemContainersFromWarehousePickingDTO(
            WarehousePickingDTO warehousePickingDTO)
        {
            var containerList = new List<WarehousePickingWorkItemContainer>();

            if (warehousePickingDTO?.WarehousePickingAssignments == null)
            {
                return containerList;
            }

            foreach (var warehousePickingAssignmentDTO in warehousePickingDTO.WarehousePickingAssignments.Assignments)
            {
                var productList = new List<Product>();
                productList.Add(new Product
                {
                    ID = warehousePickingAssignmentDTO.Product.Id,
                    Name = warehousePickingAssignmentDTO.Product.Name,
                    Description = warehousePickingAssignmentDTO.Product.Description,
                    ProductIdentifier = warehousePickingAssignmentDTO.Product.ProductIdentifier,
                    LocationText = warehousePickingAssignmentDTO.Location.DescriptiveText
                });

                var productLocationList = new List<ProductLocation>();
                productLocationList.Add(new ProductLocation
                {
                    // ID is auto incremented
                    ProductID = warehousePickingAssignmentDTO.Product.Id,
                    LocationID = warehousePickingAssignmentDTO.Location.Id
                });

                var locationDescriptorList = new List<LocationDescriptor>();
                foreach (var descriptor in warehousePickingAssignmentDTO.Location.Descriptors)
                {
                    locationDescriptorList.Add(new LocationDescriptor
                    {
                        ID = descriptor.Id,
                        LocationID = warehousePickingAssignmentDTO.Location.Id,
                        Name = descriptor.Name,
                        Value = descriptor.Value,
                        DescOrder = descriptor.DescOrder
                    });
                }

                var warehousePickingWorkItem = new WarehousePickingWorkItem
                {
                    ID = warehousePickingAssignmentDTO.Id,
                    WorkerID = warehousePickingAssignmentDTO.WorkerId,
                    Sequence = warehousePickingAssignmentDTO.Sequence,
                    InProgress = warehousePickingAssignmentDTO.Status == (int)AssignmentStatus.InProgress,
                    Completed = warehousePickingAssignmentDTO.Status == (int)AssignmentStatus.Completed,

                    ProductID = warehousePickingAssignmentDTO.Product.Id,
                    Aisle = warehousePickingAssignmentDTO.Aisle,
                    SlotID = warehousePickingAssignmentDTO.SlotId,
                    SubCenterID = warehousePickingAssignmentDTO.SubCenterId,
                    CheckDigit = warehousePickingAssignmentDTO.CheckDigit,
                    PickQuantity = warehousePickingAssignmentDTO.PickQuantity,
                    PickedQuantity = warehousePickingAssignmentDTO.PickedQuantity,
                    ShortedIndicator = warehousePickingAssignmentDTO.ShortedIndicator,
                    TripID = warehousePickingAssignmentDTO.TripId,
                    StoreNumber = warehousePickingAssignmentDTO.StoreNumber,
                    DoorNumber = warehousePickingAssignmentDTO.DoorNumber,
                    CheckPattern = warehousePickingAssignmentDTO.CheckPattern,
                    RouteNumber = warehousePickingAssignmentDTO.RouteNumber
                };

                containerList.Add(new WarehousePickingWorkItemContainer
                {
                    Products = productList,
                    ProductLocations = productLocationList,
                    LocationDescriptors = locationDescriptorList,
                    WorkItem = warehousePickingWorkItem
                });
            }

            return containerList;
        }

        private void UpdateDataForWarehousePicking(IReadOnlyList<WarehousePickingWorkItemContainer> warehousePickingWorkItemContainer)
        {
            foreach (var container in warehousePickingWorkItemContainer)
            {
                foreach (var product in container.Products)
                {
                    _Products.Add(product);
                }

                foreach (var productLocation in container.ProductLocations)
                {
                    _ProductLocations.Add(productLocation);
                }

                foreach (var locationDescriptor in container.LocationDescriptors)
                {
                    _LocationDescriptors.Add(locationDescriptor);
                }

                _WorkItems.Add(container.WorkItem);
            }
        }

        /// <summary> 
        /// Resets the data service tables. 
        /// </summary> 
        /// <returns>The async.</returns> 
        Task ResetAsync()
        {
            _Log.Debug(m => m("Resetting data service"));
            ResetPicks();
            _Log.Debug(m => m("Data service reset complete"));
            return Task.CompletedTask;
        }

        #endregion
    }
}
