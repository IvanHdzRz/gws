//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2015 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using Common.Logging;
    using GuidedWork;
    using Retail;
    using RetailEvents;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// The model of the Order Picking workflow.
    /// </summary>
    public class OrderPickingModel : IOrderPickingModel
    {
        private readonly IRetailConfigRepository _RetailConfigRepository;
        private readonly ILog _Log = LogManager.GetLogger(nameof(OrderPickingModel));
        private readonly IOrderPickingDataService _DataService;
        IEventsServiceModel _EventsServiceModel;
        private readonly IImageService _ImageService;
        private readonly IRetailAppEventsService _RetailAppEventsService;
        private long _RequestedProductID;
        private long _CurrentProductID;
        private ProductSubstitutionMap _CurrentSubstitution;
        //once a quantity is confirmed by the user it is added to this stack so we can recall past quantities picked
        //on back button presses
        private readonly Stack<int> _ConfirmedPickedQuantities;
        // when a substitution is being processed, the stock code response for the previous product is added
        // to this stack so that it can be recalled and displayed on back button presses
        private readonly Stack<string> _StockCodeResponses;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:OrderPicking.OrderPickingModel"/> class.
        /// </summary>
        /// <param name="dataService">Data service.</param>
        /// <param name="imageService">Image service.</param>
        /// <param name="applicationConfigRepository">Application config repository.</param>
        /// <param name="retailAppEventsService">Retail app events service.</param>
        /// <param name="retailAcuityEventService">Retail acuity event service.</param>
        /// <param name="eventsServiceModel">Events service model.</param>
        /// <param name="productRequestModel">Product request model.</param>
        public OrderPickingModel(IOrderPickingDataService dataService,
            IImageService imageService,
            IRetailConfigRepository applicationConfigRepository,
            IRetailAppEventsService retailAppEventsService,
            IRetailAcuityEventService retailAcuityEventService,
            IEventsServiceModel eventsServiceModel,
            IProductRequestModel productRequestModel)
        {
            _RetailConfigRepository = applicationConfigRepository;
            _DataService = dataService;
            _EventsServiceModel = eventsServiceModel;
            _ImageService = imageService;
            _RetailAppEventsService = retailAppEventsService;
            _ConfirmedPickedQuantities = new Stack<int>();
            _StockCodeResponses = new Stack<string>();
            StateMachine = new OrderPickingStateMachine(this, retailAcuityEventService, productRequestModel);
        }

        public IOrderPickingStateMachine StateMachine { get; }
        public string GetContainersResponse { get; set; }
        public string AcknowledgeLocationResponse { get; set; }
        public string EnteredProduct { get; set; }
        public string EnteredQuantityString { get; set; }
        public int EnteredQuantity { get; set; }
        public bool NoMoreConfirmation { get; set; }
        public bool SkipProductConfirmation { get; set; }
        public bool ProductQuantityConfirmation { get; set; }
        public bool OverflowConfirmation { get; set; }
        public string GoToStagingResponse { get; set; }
        public string StagingLocationResponse { get; set; }
        public bool StagingLocationConfirmation { get; set; }

        public List<OrderPickingContainer> Containers { get; private set; } = new List<OrderPickingContainer>();

        /// <summary>
        /// Gets the product associated with the default product ID
        /// </summary>
        private Product _RequestedProduct;
        public Product RequestedProduct
        {
            get
            {
                if (_RequestedProduct == null)
                {
                    _RequestedProduct = _DataService.GetProduct(_RequestedProductID);
                }
                return _RequestedProduct;
            }
        }

        /// <summary>
        /// Gets the product associated with the current product ID.
        /// </summary>
        /// <value>The product.</value>
        private Product _CurrentProduct;
        public Product CurrentProduct
        {
            get
            {
                if (_CurrentProduct == null)
                {
                    _CurrentProduct = _DataService.GetProduct(_CurrentProductID);
                }
                return _CurrentProduct;
            }
        }

        /// <summary>
        /// Gets the accepted identifiers associated with the current product ID
        /// (Identifiers that are to be accepted for validation of the current product).
        /// </summary>
        /// <value>The product.</value>
        public IReadOnlyList<string> AcceptedIdentifiers
        {
            get
            {
                return new List<string>(CurrentProduct.AcceptedIdentifiersString.Split(new[] { " " },
                    StringSplitOptions.RemoveEmptyEntries));
            }
        }

        /// <summary>
        /// Gets the order picking work item.
        /// </summary>
        /// <value>The order picking work item.</value>
        private OrderPickingWorkItem _CurrentOrderPickingWorkItem;
        public OrderPickingWorkItem CurrentOrderPickingWorkItem
        {
            get
            {
                if (_CurrentOrderPickingWorkItem == null)
                {
                    _CurrentOrderPickingWorkItem = _DataService.GetNextWorkItem();
                }
                return _CurrentOrderPickingWorkItem;
            }
        }

        public List<OrderPickingWorkItem> CurrentAndUpcomingOrderPickingWorkItems
        {
            get
            {
                return _DataService.GetAllCurrentAndUpcomingWorkItems();
            }
        }

        public uint ExpectedStockCodeResponseLength { get; private set; }

        /// <summary>
        /// Gets the current order picking order information.
        /// </summary>
        /// <value>The order picking order information.</value>
        private OrderPickingOrderInfo _OrderPickingOrderInfo;
        public OrderPickingOrderInfo OrderPickingOrderInfo
        {
            get
            {
                if (_OrderPickingOrderInfo == null)
                {
                    _OrderPickingOrderInfo = (null != CurrentOrderPickingWorkItem) ? _DataService.GetInformation(CurrentOrderPickingWorkItem) : null;
                }
                return _OrderPickingOrderInfo;
            }
        }

        public OrderPickingContainer CurrentPickingContainer
        {
            get
            {
                return Containers.FirstOrDefault(c => c.OrderId == CurrentOrderPickingWorkItem.OrderInfoID && !c.Full);
            }                
        }

        public OrderPickingContainer CurrentStagingContainer
        {
            get
            {
                return Containers.FirstOrDefault(c => c.StagingLocation == null && c.ContainsProduct);
            }
        }

        public bool RequestFillOnShort { get; private set; }

        /// <summary>
        /// Gets the order identifier
        /// </summary>
        /// <value>The order identifier.</value>
        public string OrderIdentifier { get { return OrderPickingOrderInfo.OrderIdentifier.ToString(); } }

        /// <summary>
        /// Gets and sets the quantity of product last picked
        /// </summary>
        /// <value>The quantity last picked.</value>
        /// <remarks>Should only be used as a mechanism to store the quantity picked between the Enter Quantity and Confirm Quantity screens</remarks>
        public int QuantityLastPicked { get; set; }

        /// <summary>
        /// Gets and sets the remaining quantity to be picked
        /// </summary>
        /// <value>The remaining quantity.</value>
        public int RemainingQuantity { get; private set; }

        public string StagingLocation { get; set; }

        public uint StagingMaxSpokenLength { get; set; }

        public string StockCodeResponse { get; set; }

        /// <summary>
        /// Indicates whether or not substitutions are available
        /// </summary>
        /// <value>The substitution state.</value>
        public bool SubstitutionsAvailable
        {
            get
            {
                return (null != CurrentOrderPickingWorkItem) ? _DataService.GetSubstitutionsAvailable(CurrentOrderPickingWorkItem) : false;
            }
        }

        /// <summary>
        /// Indicates whether or not the current product being picked is a substitution.
        /// </summary>
        /// <value>bool indicating if the current product is a substitution.</value>
        public bool ProcessingSubstitution { get; set; }

        /// <summary>
        /// Gets the image path for the product associated with the current order picking work item.
        /// </summary>
        public string CurrentImagePath { get { return _ImageService.GetProductImagePath(CurrentProduct.ProductIdentifier); } }

        /// <summary>
        /// Returns whether or not there are more order picking work items to handle.
        /// </summary>
        public bool MoreWorkItems { get { return null != CurrentOrderPickingWorkItem; } }

        /// <summary>
        /// Gets the index of the current order picking work item.
        /// </summary>
        public int CurrentWorkItemIndex
        {
            get
            {
                return _DataService.GetNumCompletedWorkItems() + 1;
            }
        }

        /// <summary>
        /// Gets the total number of order picking work items.
        /// </summary>
        public int TotalNumberOfWorkItems
        {
            get
            {
                return _DataService.GetNumWorkItems();
            }
        }

        public OrderPickingDataStore DataStore
        {
            get
            {
                var dataStore = new OrderPickingDataStore
                {
                    Containers = Containers,
                    CurrentProductIndex = CurrentWorkItemIndex.ToString(),
                    CurrentStagingContainer = CurrentStagingContainer,
                    ExpectedStockCodeResponseLength = ExpectedStockCodeResponseLength,
                    IsFirstSubstitution = IsFirstSubstitution(),
                    StagingLocation = StagingLocation,
                    StagingMaxSpokenLength = StagingMaxSpokenLength,
                    TotalProducts = TotalNumberOfWorkItems.ToString(),
                    QuantityLastPicked = EnteredQuantity,
                    RemainingQuantity = RemainingQuantity,
                    StockCodeResponse = StockCodeResponse,
                    WorkItems = CurrentAndUpcomingOrderPickingWorkItems
                };

                if (CurrentProduct != null)
                {
                    dataStore.AcceptedIdentifiers = AcceptedIdentifiers.ToList();
                    dataStore.OriginalProductName = RequestedProduct?.Name;
                    dataStore.ProductImage = CurrentImagePath;
                    dataStore.ProductName = CurrentProduct.Name;
                    dataStore.ProductDescription = CurrentProduct.Description;
                    dataStore.ProductIdentifier = CurrentProduct.ProductIdentifier;
                    dataStore.Price = CurrentProduct.DisplayPrice;
                    dataStore.Size = CurrentProduct.Size;
                    dataStore.LocationDescriptors = GetLocationDescriptorsForProduct();
                    dataStore.LocationText = CurrentProduct.LocationText;
                }

                if (CurrentOrderPickingWorkItem != null)
                {
                    dataStore.CurrentPickingContainer = CurrentPickingContainer;
                }

                if (OrderPickingOrderInfo != null)
                {
                    dataStore.OrderIdentifier = OrderIdentifier;
                }

                if (StateMachine.CurrentState == State.DisplayPickOrderStatus)
                {
                    dataStore.OrderPickingSummaryItems = new List<OrderPickingSummaryItem>();
                    foreach (var item in CurrentAndUpcomingOrderPickingWorkItems)
                    {
                        Product targetProduct = _DataService.GetProduct(item.ProductID);

                        var wpsi = new OrderPickingSummaryItem
                        {
                            ProductImage = _ImageService.GetProductImagePath(targetProduct.ProductIdentifier),
                            ProductName = targetProduct.Name,
                            Quantity = item.Quantity.ToString()
                        };

                        dataStore.OrderPickingSummaryItems.Add(wpsi);
                    }
                }

                return dataStore;
            }
        }

        /// <summary>
        /// Initializes the workflow.
        /// </summary>
        public Task InitializeWorkflowAsync()
        {
            StateMachine.InitializeStateMachine();
            ApplyConfigurations();
            _CurrentOrderPickingWorkItem = null;
            _OrderPickingOrderInfo = null;
            _RequestedProduct = null;
            _CurrentProduct = null;
            _CurrentSubstitution = null;

            _EventsServiceModel.WorkflowRunning = true;
            _EventsServiceModel.RunningWorkflowType = WorkflowType.OrderPick;
            _EventsServiceModel.AssignmentId = null;
            _EventsServiceModel.ClearAdditionalProperties();
            _RetailAppEventsService.StartWorkflowEvent(WorkflowType.OrderPick);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the workflow's background activity.  Called when switching to
        /// to another workflow.  Not called when restarting the current
        /// workflow.
        /// </summary>
        /// <returns>A task representing the asynchronous stop operation.</returns>
        public Task StopWorkflowAsync()
        {
            return Task.CompletedTask;
        }

        public Task ProcessUserInputAsync()
        {
            return StateMachine.ExecuteStateAsync();
        }

        public async Task GetPicksAndAssociateContainersAsync()
        {
            await _DataService.GetPicksAsync();
            Containers = _DataService.GetContainersForOrders();
        }

        private void ApplyConfigurations()
        {
            SetExpectedStockCodeResponseLength();
            SetRequestFillOnShort();
            SetStagingMaxSpokenLength();
        }

        private void SetExpectedStockCodeResponseLength()
        {
            string workflowCheckDigitsName = "OrderPickingCheckDigits";
            try
            {
                ExpectedStockCodeResponseLength = _RetailConfigRepository.GetConfig(workflowCheckDigitsName).Value.ToNonZeroUint();

            }
            catch
            {
                string globalCheckDigitsName = "GlobalCheckDigits";
                _Log.Debug(m => m($"{workflowCheckDigitsName} configuration not found. Trying {globalCheckDigitsName} configuration."));
                try
                {

                    ExpectedStockCodeResponseLength = _RetailConfigRepository.GetConfig(globalCheckDigitsName).Value.ToNonZeroUint();
                }
                catch
                {
                    uint defaultValue = 3;
                    _Log.Debug(m => m($"{globalCheckDigitsName} configuration not found. Using default value of {defaultValue}."));
                    ExpectedStockCodeResponseLength = defaultValue;
                }
            }
        }

        private void SetRequestFillOnShort()
        {
            RequestFillOnShort = _RetailConfigRepository.GetConfig("OrderPickingRequestFillOnShort")?.Value == "true";
        }

        private void SetStagingMaxSpokenLength()
        {
            string configurationName = "OrderPickingStagingMaxSpokenLength";
            try
            {
                StagingMaxSpokenLength = _RetailConfigRepository.GetConfig(configurationName).Value.ToNonZeroUint();
            }
            catch
            {
                uint defaultValue = 3;
                _Log.Debug(m => m($"{configurationName} configuration not found. Using default value of {defaultValue}."));
                StagingMaxSpokenLength = defaultValue;
            }
        }

        /// <summary>
        /// Gets the total quantity remaining to be picked.
        /// </summary>
        /// <returns>Total quantity remaining</returns>
        private int GetTotalQuantityRemaining()
        {
            //return _DataService.GetTotalQuantityRemaining();
            return 0;
        }

        /// <summary>
        /// Gets the total quantity that has been picked.
        /// </summary>
        /// <returns></returns>
        private int GetTotalQuantityPicked()
        {
            //return _DataService.GetTotalQuantityPicked();
            return 0;
        }

        public Task StoreStagingLocationAsync(long orderId, string stagingLocation)
        {
            return _DataService.StoreStagingLocationAsync(orderId, stagingLocation);
        }

        public void HandleOverflow()
        {
            var newContainer = new OrderPickingContainer
            {
                Identifier = (Containers.Max(c => int.Parse(c.Identifier)) + 1).ToString(),
                OrderId = CurrentPickingContainer.OrderId,
                OrderIdentifier = CurrentPickingContainer.OrderIdentifier
            };
            Containers.Add(newContainer);
            CurrentPickingContainer.ContainsProduct = true;
            CurrentPickingContainer.Full = true;
        }

        /// <summary>
        /// Determines if the current product is the first substitution for the original (requested) product for the order
        /// </summary>
        /// <returns>true or false indiciating if the current product is the first substitution</returns>
        public bool IsFirstSubstitution()
        {
            if(_CurrentSubstitution != null)
            {
                return _CurrentSubstitution.SubstitutionPriority == 1;
            }

            return false;
        }

        /// <summary>
        /// Sets the Requested and Current Product ID to the OrderPickingWorkItem id (ie. the requested product for the portion of the order)
        /// </summary>
        public void SetRequestedProduct()
        {
            if (null != CurrentOrderPickingWorkItem)
            {
                ProcessingSubstitution = false;
                _CurrentSubstitution = null;
                _RequestedProductID = CurrentOrderPickingWorkItem.ProductID;
                _RequestedProduct = null;
                _CurrentProductID = _RequestedProductID;
                _CurrentProduct = null;
                RemainingQuantity = CurrentOrderPickingWorkItem.Quantity;
            }
        }

        /// <summary>
        /// Sets the Current Product ID property to the next available substitute product and indicates we are processing a substitution.
        /// </summary>
        public void SetSubstitutedProduct()
        {
            if (SubstitutionsAvailable)
            {
                ProcessingSubstitution = true;
                // Once we get this from the IDataService it is marked as "Used," so we must store 
                // it in order to continue accessing properties on the same substitution.
                _CurrentSubstitution = _DataService.GetSubstitution(CurrentOrderPickingWorkItem);
                _CurrentProductID = _CurrentSubstitution.ProductID;
                _CurrentProduct = null;
                // add the current stock code response to the stack so that it can be recalled if
                // this product is returned to using the back button
                _StockCodeResponses.Push(StockCodeResponse);
            }
        }

        public bool RevertPickingActions()
        {
            //This check to ProcessingSubstitution is necessary since this method is only intended to be called when the back button is pressed
            //on a certain OrderPicking controller (OrderPickingEnterSubItemController) however the wrong method is used to capture the back button
            //press in that controller (OnStopAsync()) - therefore, this check prevents us from executing the revert actions in situations when 
            //Controller.OnStopAsync() is called and we don't want to revert picking actions
            if (ProcessingSubstitution)
            {
                // restore the stock code response for the previous product
                StockCodeResponse = _StockCodeResponses.Pop();

                //determine which substitution we are currently on
                if (_CurrentSubstitution.SubstitutionPriority == 1)
                {
                    //update the current substitution's used status
                    _CurrentSubstitution.Used = false;
                    _DataService.UpdateSubInSubstitutionMap(_CurrentSubstitution);

                    //set the current product to the requested product
                    //resetting RemainingQuantity is taken care of by SetRequestedProduct()
                    SetRequestedProduct();

                    //clear the confirmed picked quantities and response stacks since we will be returning
                    // to the original requested item (ie. no confirmed quantities have been picked)
                    _ConfirmedPickedQuantities.Clear();
                    _StockCodeResponses.Clear();
                } else
                {
                    //update the current substitution's used status
                    _CurrentSubstitution.Used = false;
                    _DataService.UpdateSubInSubstitutionMap(_CurrentSubstitution);

                    //get the previous substitution based on the current substitution's priority number
                    //and set as the current product
                    _CurrentSubstitution = _DataService.GetPreviousSubstitution(_CurrentSubstitution, CurrentOrderPickingWorkItem);
                    _CurrentProductID = _CurrentSubstitution.ProductID;
                    _CurrentProduct = null;

                    //restore the remaining quantity
                    RemainingQuantity += _ConfirmedPickedQuantities.Pop();
                }
            }
            return _ConfirmedPickedQuantities.Count > 1;
        }

        /// <summary>
        /// Sets the current order picking work item to the in-progress state in the data service and
        /// updates the server.
        /// </summary>
        public void SetWorkItemInProgress()
        {
            _DataService.SetWorkItemInProgressAsync(CurrentOrderPickingWorkItem);
            _ConfirmedPickedQuantities.Clear();
            _StockCodeResponses.Clear();
        }

        /// <summary>
        /// Sets the current order picking work item to the complete state in the data service and
        /// updates the server.
        /// </summary>
        public Task SetWorkItemCompleteAsync()
        {
            ProcessingSubstitution = false;
            _CurrentSubstitution = null;
            _ConfirmedPickedQuantities.Clear();
            _StockCodeResponses.Clear();
            var completedWorkItem = CurrentOrderPickingWorkItem;

            // clear cached work item
            _CurrentOrderPickingWorkItem = null;
            _OrderPickingOrderInfo = null;

            // Don't catch exceptions here (they are caught in the DataService)
            return _DataService.SetWorkItemCompleteAsync(completedWorkItem);
        }

        /// <summary>
        /// Skips the current order picking work item.
        /// </summary>
        public void SetWorkItemSkipped()
        {
            _DataService.SetWorkItemSkipped(CurrentOrderPickingWorkItem);
            _ConfirmedPickedQuantities.Clear();
            _StockCodeResponses.Clear();
            _CurrentSubstitution = null;

            // clear cached work item
            _CurrentOrderPickingWorkItem = null;
            _OrderPickingOrderInfo = null;
        }

        /// <summary>
        /// Determine if the productIdentifier parameter matches any part
        /// of an Accepted Identifier
        /// </summary>
        /// <param name="productIdentifier"></param>
        /// <returns>true if there is a match.</returns>
        public bool IsValidIdentifier(string productIdentifier)
        {
            foreach (string identifier in AcceptedIdentifiers)
            {
                if (identifier.Substring(Math.Max(0, identifier.Length - productIdentifier.Length)) == productIdentifier)
                {
                    return true;
                }
            }

            return false;
        }

        public HashSet<string> GetResponseExpressions(int hintLength)
        {
            var responseExpressions = new HashSet<string>();

            var productIdAndNumbers = new List<string>();
            if (null != CurrentProduct.ProductIdentifier)
            {
                productIdAndNumbers.Add(CurrentProduct.ProductIdentifier);
                productIdAndNumbers.AddRange(AcceptedIdentifiers);
            }

            foreach (var item in productIdAndNumbers)
            {
                string validNumber = item.Substring(Math.Max(0, item.Length - hintLength));
                responseExpressions.Add(validNumber);
            }

            return responseExpressions;
        }

        /// <summary>
        /// Updates the quantity remaining to be picked.
        /// </summary>
        /// <param name="quantityPicked">The quantity picked.</param>
        public void UpdateRemainingQuantity(int quantityPicked)
        {
            RemainingQuantity -= quantityPicked;
            //push the quantity picked onto the stack since the user has confirmed their picked quantity
            _ConfirmedPickedQuantities.Push(quantityPicked);
        }

        /// <summary>
        /// Generates a record for the product with the quantity picked for that item
        /// </summary>
        public Task GeneratePickedQuantityRecordAsync(int quantityPicked)
        {
            var identifier = ProcessingSubstitution ? _CurrentSubstitution.ID : CurrentOrderPickingWorkItem.ID;
            return _DataService.StorePickedQuantityAsync(identifier.ToString(), quantityPicked);
        }

        public List<LocationDescriptor> GetLocationDescriptorsForProduct()
        {
            if (null != CurrentProduct)
            {
                var locationDescriptorsList = _DataService.GetLocationDescriptorsListForProductId(CurrentProduct.ID);

                return locationDescriptorsList[0];
            }

            return new List<LocationDescriptor>();
        }
    }
}
