//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Logging;
    using GuidedWork;

    public class ReceivingModel : IReceivingModel
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(ReceivingModel));
        private readonly IReceivingDataService _DataService;
        private readonly IImageService _ImageService;

        public ReceivingModel(IReceivingDataService dataService, IImageService imageService)
        {
            _DataService = dataService;
            _ImageService = imageService;
            StateMachine = new ReceivingStateMachine(this);
        }

        public IReceivingStateMachine StateMachine { get; }

        public ReceivingUser User { get; private set; } = new ReceivingUser();
        public string CurrentUserMessage { get; set; }
        public string SelectedOrder { get; set; }
        public string EnteredHiQuantity { get; set; }
        public string EnteredTiQuantity { get; set; }
        public bool QuantityConfirmation { get; set; }
        public string AcknowledgePrintingLabel { get; set; }
        public string AcknowledgeApplyLabel { get; set; }
        public bool PalletCondition { get; set; }
        public string DamageReason { get; set; }
        public string AcknowledgeInvoiceSummary { get; set; }
        public bool NoMoreConfirmation { get; set; }

        public ReceivingDataStore DataStore
        {
            get
            {
                var dataStore = new ReceivingDataStore()
                {
                    OrderIdentifier = "123",
                    ProductIdentifier = LastValidWorkItem?.ProductIdentifier,
                    ProductName = LastValidWorkItem?.ProductName,
                    ProductImage = CurrentImagePath,
                    RequestedQuantity = LastValidWorkItem?.RequestedQuantity.ToString()
                };

                if (StateMachine.CurrentState == State.DisplayHiQuantity || StateMachine.CurrentState == State.DisplayTiQuantity ||
                    StateMachine.CurrentState == State.DisplayConfirmQuantity || StateMachine.CurrentState == State.DisplayPalletCondition)
                {
                    dataStore.ProductDescription = CurrentProduct?.Description;
                    dataStore.RemainingQuantity = (LastValidWorkItem?.RequestedQuantity - LastValidWorkItem?.ReceivedQuantity).ToString();
                    dataStore.CurrentProductIndex = CurrentWorkItemIndex.ToString();
                    dataStore.TotalProducts = TotalNumberOfWorkItems.ToString();
                    dataStore.HiQuantityLastReceived = HiQuantityLastReceived.ToString();
                    dataStore.TiQuantityLastReceived = TiQuantityLastReceived.ToString();
                    dataStore.QuantityLastReceived = QuantityLastReceived.ToString();
                }

                if (StateMachine.CurrentState == State.DisplayDamagedReason)
                {
                    dataStore.SelectionOptions = new List<string> { "Crushed", "Leaking", "Missing" };
                }

                if (StateMachine.CurrentState == State.DisplayOrders || StateMachine.CurrentState == State.DisplayInvoiceSummary)
                {
                    dataStore.ReceivingSummaryItems = new List<ReceivingSummaryItem>();
                    foreach (var item in AllReceivingWorkItems)
                    {
                        var productInfo = GetProductInfoForProductID(item.ProductIdentifier);

                        bool showQuantity = !item.Damaged && item.RequestedQuantity != item.ReceivedQuantity;
                        bool showComplete = !item.Damaged && item.RequestedQuantity == item.ReceivedQuantity;
                        bool showDamaged = item.Damaged;

                        var rsi = new ReceivingSummaryItem
                        {
                            ProductImage = productInfo.Count > 0 ? productInfo[0] : string.Empty,
                            ProductName = productInfo.Count > 0 ? productInfo[1] : string.Empty,
                            ProductIdentifier = item.ProductIdentifier,
                            RequestedQuantity = item.RequestedQuantity.ToString(),
                            RemainingQuantity = showQuantity ? (item.RequestedQuantity - item.ReceivedQuantity).ToString() : string.Empty,
                            IsComplete = showComplete,
                            IsDamaged = showDamaged
                        };

                        dataStore.ReceivingSummaryItems.Add(rsi);
                    }
                }

                return dataStore;
            }

        }

#if false
        new ReceivingDataStore
        {
            OrderIdentifier = "123",
            ProductIdentifier = LastValidWorkItem?.ProductIdentifier ?? string.Empty,
            ProductName = CurrentProduct?.Name ?? string.Empty,
            ProductImage = CurrentImagePath,
            RemainingQuantity = (LastValidWorkItem.RequestedQuantity - LastValidWorkItem.ReceivedQuantity).ToString(),
            CurrentProductIndex = CurrentWorkItemIndex.ToString(),
            TotalProducts = TotalNumberOfWorkItems.ToString(),
            HiQuantityLastReceived = HiQuantityLastReceived.ToString(),
            TiQuantityLastReceived = TiQuantityLastReceived.ToString(),
            QuantityLastReceived = QuantityLastReceived.ToString(),
            AllReceivingWorkItems = AllReceivingWorkItems,
            CurrentAndUpcomingReceivingWorkItems = CurrentAndUpcomingReceivingWorkItems
        };
#endif

        public void UpdateUser(string userId, string password)
        {
            User.Id = userId;
            User.Password = password;
        }

        public Task ProcessUserInputAsync()
        {
            return StateMachine.ExecuteStateAsync();
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

        public Task GetReceivingWorkItemsAsync()
        {
            return _DataService.GetReceivingWorkItemsAsync();
        }

        public ReceivingWorkItem ReceivingWorkItem
        {
            get
            {
                return _DataService.GetNextWorkItem();
            }
        }

        public ReceivingWorkItem LastValidWorkItem { get; set; }

        public bool MoreWorkItems
        {
            get
            {
                return _DataService.GetNextWorkItem() != null;
            }
        }

        public string ProductLocationText
        {
            get
            {
                return LastValidWorkItem?.ProductDestinationText;
            }
        }

        public string CurrentBarcode { get; set; }
        public string LastInvalidBarcode { get; set; }

        /// <summary>
        /// Gets the product associated with the current product ID.
        /// </summary>
        /// <value>The product.</value>
        public Product CurrentProduct
        {
            get
            {
                // Cache this in the future
                return _DataService.GetProduct(LastValidWorkItem.ProductIdentifier);
            }
        }

        /// <summary>
        /// Gets and sets the quantity of product last received
        /// </summary>
        /// <value>The quantity last received.</value>
        /// <remarks>Should only be used as a mechanism to store the quantity picked between the Enter Quantity and Confirm Quantity screens</remarks>
        public int QuantityLastReceived { get; set; }

        /// <summary>
        /// Gets and sets the hi quantity of product last received
        /// </summary>
        /// <value>The quantity last received.</value>
        /// <remarks>Should only be used as a mechanism to store the quantity picked between the Enter Quantity and Confirm Quantity screens</remarks>
        public int HiQuantityLastReceived { get; set; }

        /// <summary>
        /// Gets and sets the ti quantity of product last received
        /// </summary>
        /// <value>The quantity last received.</value>
        /// <remarks>Should only be used as a mechanism to store the quantity picked between the Enter Quantity and Confirm Quantity screens</remarks>
        public int TiQuantityLastReceived { get; set; }


        public int CurrentWorkItemIndex
        {
            get
            {
                return _DataService.GetNumCompletedWorkItems() + 1;
            }
        }

        public int TotalNumberOfWorkItems
        {
            get
            {
                return _DataService.GetNumWorkItems();
            }
        }

        public List<ReceivingWorkItem> CurrentAndUpcomingReceivingWorkItems
        {
            get
            {
                return _DataService.GetAllCurrentAndUpcomingWorkItems();
            }
        }

        /// <summary>
        /// Gets all of the ReceivingWorkItems for the current order
        /// </summary>
        public List<ReceivingWorkItem> AllReceivingWorkItems
        {
            get
            {
                return _DataService.GetAllWorkItems();
            }
        }

        public List<string> GetProductInfoForProductID(string productID)
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

        public List<ReceivingWorkItem> RetrieveMatchingWorkItems(string productIdentifierOrNumber)
        {
            return _DataService.GetMatchingReceivingWorkItems(productIdentifierOrNumber);
        }

        /// <summary>
        /// Gets the image path for the product associated with the last valid work item.
        /// </summary>
        public string CurrentImagePath
        {
            get
            {
                string imagePath = _ImageService.GetProductImagePath(LastValidWorkItem?.ProductIdentifier);
                if (String.IsNullOrEmpty(imagePath))
                {
                    imagePath = "missing_image.png";
                }

                return imagePath;
            }
        }

        /// <summary>
        /// Gets the product numbers for the receiving work item
        /// </summary>
        /// <value>The product numbers.</value>
        public IReadOnlyList<string> ProductNumbers
        {
            get
            {
                return new List<string>(ReceivingWorkItem.ProductNumbersString.Split(new[] { " " },
                    StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public HashSet<string> GetResponseExpressions(int hintLength)
        {
            var responseExpressions = new HashSet<string>();

            var productIdAndNumbers = new List<string>();
            if (null != ReceivingWorkItem.ProductIdentifier)
            {
                productIdAndNumbers.Add(ReceivingWorkItem.ProductIdentifier);
                productIdAndNumbers.AddRange(ProductNumbers);
            }

            foreach (var item in productIdAndNumbers)
            {
                string validNumber = item.Substring(Math.Max(0, item.Length - hintLength));
                responseExpressions.Add(validNumber);
            }

            return responseExpressions;
        }

        public void SetWorkItemSkipped()
        {
            _DataService.SetWorkItemSkipped(ReceivingWorkItem);
        }

        public void UpdateRemainingWorkItemQuantity(int quantityPicked)
        {
            LastValidWorkItem.LastReceivedQuantity = quantityPicked;
            LastValidWorkItem.ReceivedQuantity += quantityPicked;
            _DataService.UpdateWorkItem(LastValidWorkItem);
        }

        public void UpdateDamaged(bool damaged)
        {
            LastValidWorkItem.Damaged = damaged;
            _DataService.UpdateWorkItem(LastValidWorkItem);
        }

        public async Task SetWorkItemCompleteAsync()
        {
            // Make a deep copy of the ReceivingWorkItem since the referenced
            // object could change during the DataService operation
            var cachedReceivingWorkItem = new ReceivingWorkItem
            {
                ID = LastValidWorkItem.ID,
                ProductIdentifier = LastValidWorkItem.ProductIdentifier,
                ProductName = LastValidWorkItem.ProductName,
                ProductDestinationText = LastValidWorkItem.ProductDestinationText,
                ProductImageName = LastValidWorkItem.ProductImageName,
                ProductImagePath = CurrentImagePath,
                InProgress = LastValidWorkItem.InProgress,
                RequestedQuantity = LastValidWorkItem.RequestedQuantity,
                ReceivedQuantity = LastValidWorkItem.ReceivedQuantity,
                LastReceivedQuantity = LastValidWorkItem.LastReceivedQuantity,
                Completed = LastValidWorkItem.Completed
            };

            try
            {
                await _DataService.SetWorkItemCompleteAsync(cachedReceivingWorkItem).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _Log.Warn(m => m("ReceivingModel: SetWorkItemCompleteAsync for {0} failed: {1}",
                    cachedReceivingWorkItem.ProductIdentifier, e));
                throw;
            }
        }

        public Task InitializeWorkflowAsync()
        {
            CurrentBarcode = string.Empty;
            LastInvalidBarcode = string.Empty;
            StateMachine.InitializeStateMachine();

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
    }
}
