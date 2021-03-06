//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Collections.Generic;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using Retail;

    public class OrderPickingBooleanConfirmationViewModel : BooleanConfirmationViewModel, IOrderPickingViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderPickingBooleanConfirmationViewModel"/> class.
        /// </summary>
        /// <param name="dependencies">The depenendencies.</param>
        public OrderPickingBooleanConfirmationViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {

        }

        /// <summary>
        /// Gets or sets the container identifier
        /// </summary>
        private string _Container;
        public string Container
        {
            get { return _Container; }
            set
            {
                _Container = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the current product index.
        /// </summary>
        private string _CurrentProductIndex;
        public string CurrentProductIndex
        {
            get { return _CurrentProductIndex; }
            set
            {
                _CurrentProductIndex = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the expected stock code response length.
        /// </summary>
        private uint _ExpectedStockCodeResponseLength;
        public uint ExpectedStockCodeResponseLength
        {
            get { return _ExpectedStockCodeResponseLength; }
            set
            {
                _ExpectedStockCodeResponseLength = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the instruction text.
        /// </summary>
        private string _Instructions;
        public string Instructions
        {
            get { return _Instructions; }
            set
            {
                _Instructions = value;
                NotifyPropertyChanged();
            }
        }

        private string _LastDigitsLabel;
        public string LastDigitsLabel
        {
            get { return _LastDigitsLabel; }
            set
            {
                _LastDigitsLabel = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        private string _OrderIdentifier;
        public string OrderIdentifier
        {
            get { return _OrderIdentifier; }
            set
            {
                _OrderIdentifier = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the overflow menu items
        /// </summary>
        public IReadOnlyList<string> OverflowMenuItems { get; set; }

        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        private string _Price;
        public string Price
        {
            get { return _Price; }
            set
            {
                _Price = value;
                NotifyPropertyChanged();
            }
        }

        private string _ProductName;
        public string ProductName
        {
            get { return _ProductName; }
            set
            {
                _ProductName = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        private string _ProductIdentifier;
        public string ProductIdentifier
        {
            get { return _ProductIdentifier; }
            set
            {
                _ProductIdentifier = value;
                NotifyPropertyChanged();
            }
        }

        private string _ProductImage;
        public string ProductImage
        {
            get { return _ProductImage; }
            set
            {
                _ProductImage = value;
                NotifyPropertyChanged();
            }
        }

        private string _RemainingQuantity;
        public string RemainingQuantity
        {
            get { return _RemainingQuantity; }
            set
            {
                _RemainingQuantity = value;
                NotifyPropertyChanged();
            }
        }

        private string _QuantityPicked;
        public string QuantityPicked
        {
            get { return _QuantityPicked; }
            set
            {
                _QuantityPicked = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        private string _Size;
        public string Size
        {
            get { return _Size; }
            set
            {
                _Size = value;
                NotifyPropertyChanged();
            }
        }

        private string _StockCodeResponse;
        public string StockCodeResponse
        {
            get { return _StockCodeResponse; }
            set
            {
                _StockCodeResponse = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the total products.
        /// </summary>
        private string _TotalProducts;
        public string TotalProducts
        {
            get { return _TotalProducts; }
            set
            {
                _TotalProducts = value;
                NotifyPropertyChanged();
            }
        }

        private List<LocationDescriptor> _LocationDescriptors;
        public List<LocationDescriptor> LocationDescriptors
        {
            get { return _LocationDescriptors; }
            set
            {
                _LocationDescriptors = value;
                NotifyPropertyChanged();
            }
        }
    }
}
