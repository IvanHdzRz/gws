//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;

    public class OrderPickingConfirmStagingLocationViewModel : BooleanConfirmationViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderPickingConfirmStagingLocationViewModel"/> class.
        /// </summary>
        /// <param name="dependencies">The depenendencies.</param>
        public OrderPickingConfirmStagingLocationViewModel(WorkflowViewModelDependencies dependencies)
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

        /// <summary>
        /// Gets or sets the staging location
        /// </summary>
        private string _StagingLocation;
        public string StagingLocation
        {
            get { return _StagingLocation; }
            set
            {
                _StagingLocation = value;
                NotifyPropertyChanged();
            }
        }
    }
}
