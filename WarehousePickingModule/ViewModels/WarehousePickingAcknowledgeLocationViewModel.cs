//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Collections.Generic;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;

    /// <summary>
    /// Warehouse picking acknowledge location view model.
    /// </summary>
    public class WarehousePickingAcknowledgeLocationViewModel : SingleResponseViewModel, IWarehousePickingViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WarehousePickingAcknowledgeLocationViewModel"/> class.
        /// </summary>
        /// <param name="dependencies">The dependencies.</param>
        public WarehousePickingAcknowledgeLocationViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {
        }

        public IReadOnlyList<string> OverflowMenuItems { get; set; }

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
        /// Gets or sets the ready vocab word.
        /// </summary>
        private string _ReadyVocabWord;
        public string ReadyVocabWord
        {
            get { return _ReadyVocabWord; }
            set
            {
                _ReadyVocabWord = value;
                NotifyPropertyChanged();
            }
        }

        private string _NextVocabWord;
        public string NextVocabWord
        {
            get { return _NextVocabWord; }
            set
            {
                _NextVocabWord = value;
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
        /// Gets or sets the cancel vocab word.
        /// </summary>
        private string _CancelVocabWord;
        public string CancelVocabWord
        {
            get { return _CancelVocabWord; }
            set
            {
                _CancelVocabWord = value;
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
