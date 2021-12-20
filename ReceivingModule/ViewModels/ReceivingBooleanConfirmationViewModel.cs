//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWork;

    public class ReceivingBooleanConfirmationViewModel : BooleanConfirmationViewModel, IReceivingViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivingBooleanConfirmationViewModel"/> class.
        /// </summary>
        /// <param name="dependencies">The depenendencies.</param>
        public ReceivingBooleanConfirmationViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {
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

        private string _HiQuantityPicked;
        public string HiQuantityPicked
        {
            get { return _HiQuantityPicked; }
            set
            {
                _HiQuantityPicked = value;
                NotifyPropertyChanged();
            }
        }

        private string _TiQuantityPicked;
        public string TiQuantityPicked
        {
            get { return _TiQuantityPicked; }
            set
            {
                _TiQuantityPicked = value;
                NotifyPropertyChanged();
            }
        }

        private string _TotalQuantityPicked;
        public string TotalQuantityPicked
        {
            get { return _TotalQuantityPicked; }
            set
            {
                _TotalQuantityPicked = value;
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

        /// <summary>
        /// Gets or sets the hi label.
        /// </summary>
        private string _HiLabel;
        public string HiLabel
        {
            get { return _HiLabel; }
            set
            {
                _HiLabel = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the ti label.
        /// </summary>
        private string _TiLabel;
        public string TiLabel
        {
            get { return _TiLabel; }
            set
            {
                _TiLabel = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the total label.
        /// </summary>
        private string _TotalLabel;
        public string TotalLabel
        {
            get { return _TotalLabel; }
            set
            {
                _TotalLabel = value;
                NotifyPropertyChanged();
            }
        }
    }
}
