//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using Common.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using GuidedWork;
    using Retail;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Honeywell.Firebird.CoreLibrary;

    public class OrderPickingDataService : IOrderPickingDataService
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(OrderPickingDataService));

        private readonly IRetailDataService _DataService;
        private readonly IOrderPickingDataProxy _DataProxy;
        private readonly IDatabaseContainer _DbContainer;
        private readonly IUIDelegate _UiDelegate;
        private readonly IModal _Modal;


        private readonly List<Type> _TableTypes = new List<Type>
        {
            typeof(OrderPickingWorkItem),
            typeof(OrderPickingOrderInfo),
            typeof(ProductSubstitutionMap)
        };

        public OrderPickingDataService(IRetailDataService dataService, IOrderPickingDataProxy dataProxy, IDatabaseContainer databaseContainer, IAppStateService appStateService, IUIDelegate uiDelegate, IModal modal)
        {
            _DataService = dataService;
            _DataProxy = dataProxy;
            _DbContainer = databaseContainer;
            appStateService.FirstPriorityResetEventAsync += ResetAsync;
            _UiDelegate = uiDelegate;
            _Modal = modal;
        }

        #region IOrderPickingDataService Implemenation

        public List<OrderPickingWorkItem> GetAllCurrentAndUpcomingWorkItems()
        {
            lock(_DbContainer.DatabaseLock)
            {
                //TODO: there may be a better (more efficient) way to do this in LINQ

                //disclaimer - the OrderPickingWorkItem objects returned from this query will only have
                //the ProductID and Quantity fields populated - others will be null due to the select statement
                return _DbContainer.Database.Query<OrderPickingWorkItem>(
                    "select ProductID, Quantity, OrderInfoID " +
                    "from OrderPickingWorkItem " +
                    "where Completed = 0 " +
                    "order by Sequence").ToList();
            }
        }

        public List<OrderPickingContainer> GetContainersForOrders()
        {
            lock (_DbContainer.DatabaseLock)
            {
                var containers = new List<OrderPickingContainer>();
                var orderInfos = _DbContainer.Database.Table<OrderPickingOrderInfo>().GroupBy(oi => oi.ID).Select(g => g.First()).ToList();

                int containerId = 1;
                foreach (var orderInfo in orderInfos)
                {
                    containers.Add(new OrderPickingContainer
                    {
                        Identifier = containerId++.ToString(),
                        OrderId = orderInfo.ID,
                        OrderIdentifier = orderInfo.OrderIdentifier,
                    });
                }
                return containers;
            }
        }

        public OrderPickingOrderInfo GetInformation(OrderPickingWorkItem workItem)
        {
            lock(_DbContainer.DatabaseLock)
            {
                return _DbContainer.Database.Table<OrderPickingOrderInfo>().First(opoi => opoi.ID == workItem.OrderInfoID);
            }
        }

        public List<List<LocationDescriptor>> GetLocationDescriptorsListForProductId(long productID)
        {
            return _DataService.GetLocationDescriptorsListForProductId(productID);
        }

        public OrderPickingWorkItem GetNextWorkItem()
        {
            return _DataService.GetNextWorkItem<OrderPickingWorkItem>();
        }

        public int GetNumCompletedWorkItems()
        {
            return _DataService.GetNumCompletedWorkItems<OrderPickingWorkItem>();
        }

        public int GetNumWorkItems()
        {
            return _DataService.GetNumWorkItems<OrderPickingWorkItem>();
        }

        public async Task GetPicksAsync()
        {
            string orderPickingDTOJson = await _DataProxy.DataTransport.FetchOrderPickingDTOAsync().ConfigureAwait(false);
            var orderPickingDTO = TracingJsonConvert.DeserializeRequiringAllMembers<OrderPickingDTO>(orderPickingDTOJson, _Log);
            var orderPickingWorkItemContainers = GetOrderPickingWorkItemContainersFromOrderPickingDTO(orderPickingDTO);

            var productHashSet = new HashSet<string>();
            foreach (var container in orderPickingWorkItemContainers.Where(opwic => !opwic.WorkItem.Completed))
            {
                productHashSet.UnionWith(container.Products.Select(product => product.ProductIdentifier));
            }
            _DataService.RetrieveFilesForProductIdentifiersFromProductImagesFireAndForget(productHashSet.ToList());

            UpdateDataForOrderPicking(orderPickingWorkItemContainers);
        }

        public ProductSubstitutionMap GetPreviousSubstitution(ProductSubstitutionMap currentSub, OrderPickingWorkItem workItem)
        {
            int previousSubPriority = currentSub.SubstitutionPriority - 1;
            lock(_DbContainer.DatabaseLock)
            {
                return _DbContainer.Database.Table<ProductSubstitutionMap>().First(prevSub =>
                    prevSub.OrderPickingWorkItemID == workItem.ID && prevSub.Used && prevSub.SubstitutionPriority == previousSubPriority);
            }
        }

        public Product GetProduct(long id)
        {
            return _DataService.GetProduct(id);
        }

        public ProductSubstitutionMap GetSubstitution(OrderPickingWorkItem workItem)
        {
            lock(_DbContainer.DatabaseLock)
            {
                var substitutionMappings = _DbContainer.Database.Table<ProductSubstitutionMap>()
                                                   .Where(mapping => mapping.OrderPickingWorkItemID == workItem.ID &&
                                                                     !mapping.Used)
                                                   .OrderBy(mapping => mapping.SubstitutionPriority)
                                                   .ToList();

                if(substitutionMappings.Any())
                {
                    // Use the highest priority item and mark it as used
                    substitutionMappings[0].Used = true;
                    _DbContainer.Database.Update(substitutionMappings[0]);
                    return substitutionMappings[0];
                }
                else
                {
                    return null;
                }
            }
        }

        public bool GetSubstitutionsAvailable(OrderPickingWorkItem workItem)
        {
            lock(_DbContainer.DatabaseLock)
            {
                return _DbContainer.Database.Table<ProductSubstitutionMap>().Any(mapping =>
                    mapping.OrderPickingWorkItemID == workItem.ID && !mapping.Used);
            }
        }

        public Task SetWorkItemCompleteAsync(OrderPickingWorkItem workItem)
        {
            return AlertOnDataLossAsync(_DataService.SetWorkItemCompleteAsync(workItem));
        }

        public Task SetWorkItemInProgressAsync(OrderPickingWorkItem workItem)
        {
            return AlertOnDataLossAsync(_DataService.SetWorkItemInProgressAsync(workItem));
        }


        public void SetWorkItemSkipped(OrderPickingWorkItem workItem)
        {
            _DataService.SetWorkItemSkipped(workItem);
        }

        public Task StorePickedQuantityAsync(string workItemId, int quantity)
        {
            return AlertOnDataLossAsync(_DataProxy.DataTransport.StorePickedQuantityAsync(workItemId, quantity));
        }

        public Task StoreStagingLocationAsync(long orderId, string stagingLocation)
        {
            return AlertOnDataLossAsync(_DataProxy.DataTransport.StoreStagingLocationAsync(orderId, stagingLocation));
        }

        public void UpdateSubInSubstitutionMap(ProductSubstitutionMap sub)
        {
            lock(_DbContainer.DatabaseLock)
            {
                _DbContainer.Database.Update(sub);
            }
        }

        #endregion

        #region OrderPickingDataService Database Helpers

        //TODO: Move away from using these private class "containers" for inserting things
        //into the database
        class OrderPickingWorkItemContainer
        {
            public List<Product> Products;
            public List<ProductLocation> ProductLocations;
            public List<LocationDescriptor> LocationDescriptors;
            public OrderPickingOrderInfo OrderPickingOrderInfo;
            public List<ProductSubstitutionMap> ProductSubstitutionMappings;
            public OrderPickingWorkItem WorkItem;
        }

        IReadOnlyList<OrderPickingWorkItemContainer> GetOrderPickingWorkItemContainersFromOrderPickingDTO(
            OrderPickingDTO orderPickingDTO)
        {
            var containerList = new List<OrderPickingWorkItemContainer>();

            if(orderPickingDTO?.OrderPickingAssignments == null)
            {
                return containerList;
            }

            foreach(var orderPickingAssignmentDTO in orderPickingDTO.OrderPickingAssignments.Assignments)
            {
                var productList = new List<Product>();
                productList.Add(new Product
                {
                    ID = orderPickingAssignmentDTO.Product.Id,
                    Description = orderPickingAssignmentDTO.Product.Description,
                    DisplayPrice = orderPickingAssignmentDTO.Product.DisplayPrice,
                    Name = orderPickingAssignmentDTO.Product.Name,
                    ProductIdentifier = orderPickingAssignmentDTO.Product.ProductIdentifier,
                    AcceptedIdentifiersString = orderPickingAssignmentDTO.Product.AcceptedIdentifiers != null ? string.Join(" ", orderPickingAssignmentDTO.Product.AcceptedIdentifiers.ToArray()) : string.Empty,
                    Size = orderPickingAssignmentDTO.Product.Size,
                    LocationText = orderPickingAssignmentDTO.Location.DescriptiveText
                });

                var productLocationList = new List<ProductLocation>();
                productLocationList.Add(new ProductLocation
                {
                    // ID is auto incremented
                    ProductID = orderPickingAssignmentDTO.Product.Id,
                    LocationID = orderPickingAssignmentDTO.Location.Id
                });

                var locationDescriptorList = new List<LocationDescriptor>();
                foreach (var descriptor in orderPickingAssignmentDTO.Location.Descriptors)
                {
                    locationDescriptorList.Add(new LocationDescriptor
                    {
                        ID = descriptor.Id,
                        LocationID = orderPickingAssignmentDTO.Location.Id,
                        Name = descriptor.Name,
                        Value = descriptor.Value,
                        DescOrder = descriptor.DescOrder,
                    });
                }

                var orderPickingOrderInfo = new OrderPickingOrderInfo
                {
                    ID = orderPickingAssignmentDTO.OrderInfo.Id,
                    OrderIdentifier = orderPickingAssignmentDTO.OrderInfo.OrderIdentifier,
                    RequestedCompletionDate = orderPickingAssignmentDTO.OrderInfo.RequestedCompletionDate
                };

                var orderPickingWorkItem = new OrderPickingWorkItem
                {
                    ID = orderPickingAssignmentDTO.Id,
                    WorkerID = orderPickingAssignmentDTO.WorkerId,
                    ProductID = orderPickingAssignmentDTO.Product.Id,
                    OrderInfoID = orderPickingAssignmentDTO.OrderInfo.Id,
                    Quantity = orderPickingAssignmentDTO.Quantity,
                    Sequence = orderPickingAssignmentDTO.CompletionOrder,
                    InProgress = orderPickingAssignmentDTO.Status == (int)AssignmentStatus.InProgress,
                    Completed = orderPickingAssignmentDTO.Status == (int)AssignmentStatus.Completed,
                    Notes = orderPickingAssignmentDTO.Notes
                };

                var substitutionMappingList = new List<ProductSubstitutionMap>();
                if(orderPickingAssignmentDTO.Substitutions != null)
                {
                    foreach (var substitution in orderPickingAssignmentDTO.Substitutions)
                    {
                        substitutionMappingList.Add(new ProductSubstitutionMap
                        {
                            ID = substitution.Id,
                            OrderPickingWorkItemID = orderPickingAssignmentDTO.Id,
                            ProductID = substitution.Product.Id,
                            SubstitutionPriority = substitution.SubstitutionPriority,
                            Used = false
                        });

                        productList.Add(new Product
                        {
                            ID = substitution.Product.Id,
                            Description = substitution.Product.Description,
                            DisplayPrice = substitution.Product.DisplayPrice,
                            Name = substitution.Product.Name,
                            ProductIdentifier = substitution.Product.ProductIdentifier,
                            AcceptedIdentifiersString = substitution.Product.AcceptedIdentifiers != null ? string.Join(" ", substitution.Product.AcceptedIdentifiers.ToArray()) : string.Empty,
                            Size = substitution.Product.Size,
                            LocationText = substitution.Location.DescriptiveText
                        });

                        productLocationList.Add(new ProductLocation
                        {
                            // ID is auto incremented
                            ProductID = substitution.Product.Id,
                            LocationID = substitution.Location.Id
                        });

                        foreach (var descriptor in substitution.Location.Descriptors)
                        {
                            locationDescriptorList.Add(new LocationDescriptor
                            {
                                ID = descriptor.Id,
                                LocationID = substitution.Location.Id,
                                Name = descriptor.Name,
                                Value = descriptor.Value,
                                DescOrder = descriptor.DescOrder,
                            });
                        }
                    }
                }

                containerList.Add(new OrderPickingWorkItemContainer
                {
                    Products = productList,
                    ProductLocations = productLocationList,
                    LocationDescriptors = locationDescriptorList,
                    OrderPickingOrderInfo = orderPickingOrderInfo,
                    ProductSubstitutionMappings = substitutionMappingList,
                    WorkItem = orderPickingWorkItem
                });
            }

            return containerList;
        }

        void UpdateDataForOrderPicking(IReadOnlyList<OrderPickingWorkItemContainer> orderPickingWorkItemContainers)
        {
            lock(_DbContainer.DatabaseLock)
            {
                foreach (var container in orderPickingWorkItemContainers.Where(
                    container => !container.WorkItem.Completed &&
                        !_DbContainer.Database.Table<OrderPickingWorkItem>().Where(opwi => opwi.ID == container.WorkItem.ID).Any()))
                {
                    foreach (var product in container.Products)
                    {
                        _DbContainer.Database.InsertOrReplace(product);
                    }

                    foreach (var productLocation in container.ProductLocations)
                    {
                        _DbContainer.Database.Insert(productLocation);
                    }

                    foreach (var locationDescriptor in container.LocationDescriptors)
                    {
                        _DbContainer.Database.InsertOrReplace(locationDescriptor);
                    }

                    _DbContainer.Database.InsertOrReplace(container.OrderPickingOrderInfo);
                    _DbContainer.Database.InsertOrReplace(container.WorkItem);

                    foreach(var substitutionMapping in container.ProductSubstitutionMappings)
                    {
                        _DbContainer.Database.Insert(substitutionMapping);
                    }
                }
            }
        }

        /// <summary>
        /// Resets the data service tables.
        /// </summary>
        /// <returns>The async.</returns>
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
            lock(_DbContainer.DatabaseLock)
            {
                foreach (var type in _TableTypes)
                {
                    _DbContainer.Database.DropTable(new SQLite.TableMapping(type));
                }
            }
        }
        #endregion

        /// <summary>
        /// The data loss alert title key.
        /// </summary>
        public static readonly string DataLossAlertTitleKey = "Text_DataLossAlertTitle";

        /// <summary>
        /// The data loss alert body key.
        /// </summary>
        public static readonly string DataLossAlertBodyKey = "Text_DataLossAlertBody";

        /// <summary>
        /// Displays an alert to the user if the given Task throws an exception. This method is useful
        /// for critical asynchronous calls that we don't await.
        ///
        /// For example, several outgoing network requests are not awaited, as we already have the data
        /// necessary for the user to move on to the next work item. However, if an error occurs while
        /// uploading data which causes that data to be lost, we want to alert the user.
        /// </summary>
        /// <param name="task">The task to handle exceptions thrown by.</param>
        /// <param name="exceptionToAlert">A function that returns the title and body text, respectively,
        /// to be displayed. This function can be used to customize the alert depending on the exception.
        /// If this function returns null, or any of the text in the returned Tuple is empty, the alert
        /// will not be displayed, allowing specific exceptions to be ignored.</param>
        /// <remarks>This method will suppress ANY exception thrown by the task.</remarks>
        protected async Task AlertOnFailureAsync(Task task, Func<Exception, Tuple<string, string>> exceptionToAlert)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var alert = exceptionToAlert(e);
                bool suppressAlert = (alert == null ||
                                      string.IsNullOrWhiteSpace(alert.Item1) ||
                                      string.IsNullOrWhiteSpace(alert.Item2));
                _Log.Error($"AlertOnFailureAsync caught exception, {(suppressAlert ? "suppressing" : "alerting")}:");
                _Log.Error(e);
                if (!suppressAlert)
                {
                    await _UiDelegate.BeginInvokeOnMainThread(() => _Modal.ShowAlertDialogAsync(alert.Item1, alert.Item2)).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Displays a generic data loss warning alert to the user if the given Task throws an exception.
        /// Intended to be used for asynchronous WorkflowModel methods which are not awaited by the controller.
        /// </summary>
        /// <param name="task">The task to handle exceptions thrown by.</param>
        /// <remarks>If custom exception handling is desired, <see cref="AlertOnFailureAsync(Task, Func{Exception, Tuple{string, string}})"/>
        /// should be used, with a custom exception handler. In the case of </remarks>
        protected Task AlertOnDataLossAsync(Task task)
        {
            return AlertOnFailureAsync(task, DataLossAlert);
        }

        /// <summary>
        /// A generic implementation for the exception-to-alert method required by <see cref="AlertOnFailureAsync(Task, Func{Exception, Tuple{string, string}})"/>.
        /// </summary>
        /// <param name="e">The exception thrown by the background task.</param>
        /// <returns>A tuple containing the title and body text, respectively, for the alert to be
        /// displayed.</returns>
        protected virtual Tuple<string, string> DataLossAlert(Exception e)
        {
            return new Tuple<string, string>(Translate.GetLocalizedTextForKey(DataLossAlertTitleKey), Translate.GetLocalizedTextForKey(DataLossAlertBodyKey));
        }
    }
}
