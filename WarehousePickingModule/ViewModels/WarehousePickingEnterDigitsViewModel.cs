//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Collections.Generic;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;

    /// <summary>
    /// Warehouse picking enter digits view model.
    /// </summary>
    public class WarehousePickingEnterDigitsViewModel : DigitEntryViewModel, IWarehousePickingViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:GuidedWork.WarehousePickingEnterDigitsViewModel"/> class.
        /// </summary>
        /// <param name="dependencies">The depenendencies.</param>
        public WarehousePickingEnterDigitsViewModel(WorkflowViewModelDependencies dependencies)
                : base(dependencies)
        {
        }

        /// <summary>
        /// Gets or sets the words in the overflow menu items.
        /// </summary>
        public IReadOnlyList<string> OverflowMenuItems { get; set; }

        /// <summary>
        /// Gets or sets the assignment number.
        /// </summary>
        private string _AssignmentNumber;
        public string AssignmentNumber
        {
            get { return _AssignmentNumber; }
            set
            {
                _AssignmentNumber = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the stockcode entry placeholder.
        /// </summary>
        private string _StockCodeEntryPlaceholder;
        public string StockCodeEntryPlaceholder
        {
            get { return _StockCodeEntryPlaceholder; }
            set
            {
                _StockCodeEntryPlaceholder = value;
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

        /// <summary>
        /// Gets or sets the trip identifier
        /// </summary>
        private string _TripIdentifier;
        public string TripIdentifier
        {
            get { return _TripIdentifier; }
            set
            {
                _TripIdentifier = value;
                NotifyPropertyChanged();
            }
        }

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

        /// <summary>
        /// Gets or sets the product image path.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the users response for stock code.
        /// </summary>
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
        /// Gets or sets the product remaining quantity.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the skip item vocab word.
        /// </summary>
        private string _SkipProductVocabWord;
        public string SkipProductVocabWord
        {
            get { return _SkipProductVocabWord; }
            set
            {
                _SkipProductVocabWord = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a bool representing whether the skip item button should be displayed.
        /// </summary>
        private bool _SkipProductVisible;
        public bool SkipProductVisible
        {
            get { return _SkipProductVisible; }
            set
            {
                _SkipProductVisible = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a bool representing whether the quantity entry should be displayed.
        /// </summary>
        private bool _QuantityEntryVisible;
        public bool QuantityEntryVisible
        {
            get { return _QuantityEntryVisible; }
            set
            {
                _QuantityEntryVisible = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a bool representing whether the ordered quantity should be displayed.
        /// </summary>
        private bool _OrderedQuantityVisible = true;
        public bool OrderedQuantityVisible
        {
            get { return _OrderedQuantityVisible; }
            set
            {
                _OrderedQuantityVisible = value;
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

        /// <summary>
        /// Gets or sets the list of LocationDescriptors.
        /// </summary>
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

