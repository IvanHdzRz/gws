//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System;
    using System.Windows.Input;
    using Common.Logging;
    using Honeywell.Firebird;
    using Honeywell.Firebird.WorkflowEngine;
    using System.Collections.Generic;
    using GuidedWork;
    using Xamarin.Forms;
    using Honeywell.Firebird.CoreLibrary;

    public class WarehousePickingBooleanConfirmationViewModel : BooleanConfirmationViewModel, IWarehousePickingViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WarehousePickingBooleanConfirmationViewModel"/> class.
        /// </summary>
        /// <param name="dependencies">The depenendencies.</param>
        public WarehousePickingBooleanConfirmationViewModel(WorkflowViewModelDependencies dependencies)
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
