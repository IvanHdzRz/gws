//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;

    public class OrderPickingConfirmOverflowViewModel : BooleanConfirmationViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderPickingConfirmOverflowViewModel"/> class.
        /// </summary>
        /// <param name="dependencies">The depenendencies.</param>
        public OrderPickingConfirmOverflowViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        /// Gets or sets the order identifier
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
