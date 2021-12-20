//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using GuidedWork;

    /// <summary>
    /// The model of the warehouse picking workflow.
    /// </summary>
    public class WarehousePickingModel : IWarehousePickingModel
    {
        private readonly IWarehousePickingDataService _DataService;
        private readonly IImageService _ImageService;
        private long _CurrentProductID;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:GuidedWork.WarehousePickingModel"/> class.
        /// </summary>
        /// <param name="dataService">Data service.</param>
        /// <param name="warehousePickingActivityTracker">Activity tracker</param>
        /// <param name="imageService">Image service.</param>
        public WarehousePickingModel(IWarehousePickingDataService dataService, IWarehousePickingActivityTracker warehousePickingActivityTracker, IImageService imageService)
        {
            _DataService = dataService;
            _ImageService = imageService;
            StateMachine = new WarehousePickingStateMachine(this, warehousePickingActivityTracker);
        }

        public IWarehousePickingStateMachine StateMachine { get; }
        public string CurrentUserMessage { get; set; }

        public string SelectedSubcenter { get; set; }
        public string LabelPrinter { get; set; }
        public string AcknowledgePickTripInfo { get; set; }
        public string AcknowledgedLocation { get; set; }
        public string EnteredProduct { get; set; }
        public string EnteredQuantityString { get; set; }
        public int EnteredQuantity { get; set; }
        public bool ShortProductConfirmation { get; set; }
        public bool NoMoreConfirmation { get; set; }
        public bool SkipProductConfirmation { get; set; }
        public string AcknowledgePickOrderStatus { get; set; }
        public string AcknowledgePickOrderSummary { get; set; }
        public string AcknowledgePickPerformance { get; set; }
        public string AcknowledgeLastPick { get; set; }

        public WarehousePickingUser User { get; } = new WarehousePickingUser();

        public WarehousePickingDataStore DataStore
        {
            get
            {
                var dataStore = new WarehousePickingDataStore
                {
                    LabelPrinter = LabelPrinter ?? string.Empty,
                    CurrentProductIndex = CurrentWorkItemIndex.ToString(),
                    TotalProducts = TotalNumberOfWorkItems.ToString(),
                    QuantityLastPicked = EnteredQuantity,
                    RemainingQuantity = RemainingQuantity,
                    PickedCases = GetTotalQuantityPicked().ToString(),
                    RemainingCases = GetTotalQuantityRemaining().ToString(),
                    SelectionOptions = new List<string> { "Dry", "Freezer", "Produce" },

                    // TODO: move the translations back to controller
                    ReadyVocabWord = Translate.GetLocalizedTextForKey("accept_entry_word"),
                    NextVocabWord = Translate.GetLocalizedTextForKey("next_entry_word"),
                    CancelVocabWord = Translate.GetLocalizedTextForKey("VocabWord_EndOrder"),
                    SkipProductVocabWord = Translate.GetLocalizedTextForKey("VocabWord_SkipProduct")
                };

                if (CurrentProduct != null)
                {
                    dataStore.ProductImage = CurrentImagePath;
                    dataStore.ProductName = CurrentProduct.Name;
                    dataStore.ProductDescription = CurrentProduct.Description;
                    dataStore.ProductIdentifier = CurrentProduct.ProductIdentifier;
                    dataStore.DetailedLocationText = CurrentProduct.LocationText;
                    dataStore.Price = CurrentProduct.DisplayPrice;
                    dataStore.Size = CurrentProduct.Size;
                    dataStore.LocationDescriptors = GetLocationDescriptorsForProduct();
                }

                if (CurrentWarehousePickingWorkItem != null)
                {
                    dataStore.TripIdentifier = CurrentWarehousePickingWorkItem.TripID;
                    dataStore.PickedQuantity = CurrentWarehousePickingWorkItem.PickedQuantity.ToString();
                    dataStore.Aisle = CurrentWarehousePickingWorkItem.Aisle;
                    dataStore.SlotID = CurrentWarehousePickingWorkItem.SlotID;
                    dataStore.CheckDigit = CurrentWarehousePickingWorkItem.CheckDigit;
                }

                if (StateMachine.CurrentState == State.DisplayPickOrderStatus || StateMachine.CurrentState == State.DisplayPickOrderSummary)
                {
                    dataStore.WarehousePickingSummaryItems = new List<WarehousePickingSummaryItem>();
                    foreach (var item in AllWarehousePickingWorkItems)
                    {
                        var productInfo = GetProductInfoForProductID(item.ProductID);

                        var wpsi = new WarehousePickingSummaryItem
                        {
                            ProductImage = productInfo.Count > 0 ? productInfo[0] : string.Empty,
                            ProductName = productInfo.Count > 0 ? productInfo[1] : string.Empty,
                            PickQuantity = item.PickQuantity.ToString(),
                            Aisle = item.Aisle,
                            SlotID = item.SlotID,
                            ShortedQuantity = item.ShortedIndicator ? (item.PickedQuantity - item.PickQuantity).ToString() : string.Empty,
                            IsComplete = item.PickedQuantity == item.PickQuantity
                        };

                        dataStore.WarehousePickingSummaryItems.Add(wpsi);
                    }
                }

                if (StateMachine.CurrentState == State.DisplayPickPerformance)
                {
                    dataStore.WarehousePickingWorkItems = CurrentAndUpcomingWarehousePickingWorkItems;
                }

                if (StateMachine.CurrentState == State.DisplayLastPick)
                {
                    dataStore.PreviousWarehousePickingWorkItem = PreviousWarehousePickingWorkItem;
                }

                return dataStore;
            }
        }

        public void UpdateUser(string userId, string password)
        {
            User.Id = userId;
            User.Password = password;
        }

        public Task ProcessUserInputAsync()
        {
            return StateMachine.ExecuteStateAsync();
        }

        public string GetAisleForProductID(long productID)
        {
            return CurrentWarehousePickingWorkItem?.Aisle;
        }

        /// <summary>
        /// Returns whether or not there are more warehouse picking work items to handle.
        /// </summary>
        public bool MoreWorkItems => null != CurrentWarehousePickingWorkItem;

        /// <summary>
        /// Initializes the workflow.
        /// </summary>
        public Task InitializeWorkflowAsync()
        {
            PreviousWarehousePickingWorkItem = null;
            _WarehousePickingWorkItem = null;
            _CurrentProduct = null;

            StateMachine.InitializeStateMachine();

            return Task.FromResult(false);
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

        /// <summary>
        /// Sets the Requested and Current Product ID to the WarehousePickingWorkItem id (ie. the requested product for the portion of the pick)
        /// </summary>
        public void SetRequestedProduct()
        {
            if (null != CurrentWarehousePickingWorkItem)
            {
                _CurrentProductID = CurrentWarehousePickingWorkItem.ProductID;
                _CurrentProduct = null;
            }
        }

        /// <summary>
        /// Sets the current warehouse picking work item to the in-progress state in the data service and
        /// updates the server.
        /// </summary>
        public void SetWorkItemInProgress()
        {
            _DataService.SetWorkItemInProgressAsync(CurrentWarehousePickingWorkItem);
        }

        /// <summary>
        /// Sets the current warehouse picking work item to the complete state in the data service and
        /// updates the server.
        /// </summary>
        public Task SetWorkItemCompleteAsync()
        {
            var completedWorkItem = CurrentWarehousePickingWorkItem;

            // clear cached work item
            PreviousWarehousePickingWorkItem = completedWorkItem;
            _WarehousePickingWorkItem = null;

            // Don't catch exceptions here (they are caught in the DataService)
            return _DataService.SetWorkItemCompleteAsync(completedWorkItem);
        }

        /// <summary>
        /// Skips the current warehouse picking work item.
        /// </summary>
        public void SetWorkItemSkipped()
        {
            _DataService.SetWorkItemSkipped(CurrentWarehousePickingWorkItem);

            // clear cached work item
            _WarehousePickingWorkItem = null;
        }

        /// <summary>
        /// Updates the quantity remaining to be picked.
        /// </summary>
        /// <param name="quantityPicked">The quantity picked.</param>
        public void UpdateRemainingQuantity(int quantityPicked)
        {
            CurrentWarehousePickingWorkItem.PickedQuantity += quantityPicked;
        }

        /// <summary>
        /// Sets the shorted indicator on the current work item.
        /// </summary>
        public void SetShortedIndicator()
        {
            CurrentWarehousePickingWorkItem.ShortedIndicator = true;
        }

        /// <summary>
        /// Generates a record for the product with the quantity picked for that item
        /// </summary>
        public Task GeneratePickedQuantityRecordAsync(int quantityPicked)
        {
            var identifier = CurrentWarehousePickingWorkItem.ID;
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

        public Task RetrievePicksAsync()
        {
            if (_DataService.GetNumWorkItems() > 0)
            {
                // Don't reload data unless the DataService has been reset
                return Task.FromResult(false);
            }

            return _DataService.GetPicksAsync();
        }

        public void ResetPicks()
        {
            PreviousWarehousePickingWorkItem = null;
            _WarehousePickingWorkItem = null;
            _CurrentProduct = null;

            _DataService.ResetPicks();
        }

        public Task<bool> ValidateUserCredentialsAsync()
        {
            if (string.IsNullOrWhiteSpace(User.Id) ||
                string.IsNullOrWhiteSpace(User.Password))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task RetrieveSubcentersAsync()
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Gets the warehouse picking work item.
        /// </summary>
        /// <value>The warehouse picking work item.</value>
        private WarehousePickingWorkItem _WarehousePickingWorkItem;
        public WarehousePickingWorkItem CurrentWarehousePickingWorkItem => _WarehousePickingWorkItem ?? (_WarehousePickingWorkItem = _DataService.GetNextWorkItem());

        private List<string> GetProductInfoForProductID(long productID)
        {
            Product targetProduct = _DataService.GetProduct(productID);
            List<string> productInfo = new List<string>();
            if (targetProduct != null)
            {
                productInfo.Add(_ImageService.GetProductImagePath(targetProduct.ProductIdentifier));
                productInfo.Add(targetProduct.Name);
            }

            return productInfo;
        }

        /// <summary>
        /// Gets the product associated with the current product ID.
        /// </summary>
        /// <value>The product.</value>
        private Product _CurrentProduct;
        private Product CurrentProduct => _CurrentProduct ?? (_CurrentProduct = _DataService.GetProduct(_CurrentProductID));

        /// <summary>
        /// Gets the previous warehouse picking item.
        /// </summary>
        private WarehousePickingWorkItem PreviousWarehousePickingWorkItem { get; set; }

        /// <summary>
        /// Gets all of the non-completed WarehousePickingWorkItems for the current order
        /// </summary>
        /// <returns>List of non-completed WarehousePickingWorkItem objects</returns>
        private List<WarehousePickingWorkItem> CurrentAndUpcomingWarehousePickingWorkItems => _DataService.GetAllCurrentAndUpcomingWorkItems();

        /// <summary>
        /// Gets all of the WarehousePickingWorkItems for the current order
        /// </summary>
        private List<WarehousePickingWorkItem> AllWarehousePickingWorkItems => _DataService.GetAllWorkItems();

        /// <summary>
        /// Gets the total quantity remaining to be picked.
        /// </summary>
        /// <returns>Total quantity remaining</returns>
        private int GetTotalQuantityRemaining()
        {
            return _DataService.GetTotalQuantityRemaining();
        }

        /// <summary>
        /// Gets the total quantity that has been picked.
        /// </summary>
        /// <returns></returns>
        private int GetTotalQuantityPicked()
        {
            return _DataService.GetTotalQuantityPicked();
        }

        /// <summary>
        /// Gets and sets the remaining quantity to be picked
        /// </summary>
        /// <value>The remaining quantity.</value>
        private int RemainingQuantity
        {
            get
            {
                if (CurrentWarehousePickingWorkItem != null)
                {
                    return CurrentWarehousePickingWorkItem.PickQuantity - CurrentWarehousePickingWorkItem.PickedQuantity;
                }

                return 0;

            }
        }

        /// <summary>
        /// Gets the image path for the product associated with the current warehouse picking work item.
        /// </summary>
        private string CurrentImagePath
        {
            get
            {
                string imagePath = _ImageService.GetProductImagePath(CurrentProduct?.ProductIdentifier);
                if (String.IsNullOrEmpty(imagePath))
                {
                    imagePath = "missing_image.png";
                }

                return imagePath;
            }
        }

        /// <summary>
        /// Gets the index of the current warehouse picking work item.
        /// </summary>
        private int CurrentWorkItemIndex => _DataService.GetNumCompletedWorkItems() + 1;

        /// <summary>
        /// Gets the total number of warehouse picking work items.
        /// </summary>
        private int TotalNumberOfWorkItems => _DataService.GetNumWorkItems();
    }
}
