//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;

    /// <summary>
    /// Order picking enter staging location view model.
    /// </summary>
    public class OrderPickingEnterStagingLocationViewModel : DigitEntryViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderPickingEnterStagingLocationViewModel"/> class.
        /// </summary>
        /// <param name="dependencies">The depenendencies.</param>
        public OrderPickingEnterStagingLocationViewModel(WorkflowViewModelDependencies dependencies)
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
        /// Gets or sets the header
        /// </summary>
        private string _Header;
        public string Header
        {
            get { return _Header; }
            set
            {
                _Header = value;
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
    }
}

