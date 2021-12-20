//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2015 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWork;

    /// <summary>
    /// Order picking acknowledge substitution view model.
    /// </summary>
    public class OrderPickingAcknowledgeSubstitutionViewModel : SingleResponseViewModel
    {
        private ProductImageGridSubviewModel _ProductImageGridSubviewModel;

        /// <summary>
        /// Gets or sets the product image grid subview model.
        /// </summary>
        /// <value>The product image grid subview model.</value>
        public ProductImageGridSubviewModel ProductImageGridSubviewModel
        {
            get { return _ProductImageGridSubviewModel; }
            set
            {
                _ProductImageGridSubviewModel = value;
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

        /// <summary>
        /// Gets or sets the next vocab word.
        /// </summary>
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

        private string _OrderNumber;

        /// <summary>
        /// Gets or sets the zone.
        /// </summary>
        /// <value>The zone.</value>
        public string OrderNumber
        {
            get { return _OrderNumber; }
            set
            {
                _OrderNumber = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Retail.OrderPickingAcknowledgeSubstitutionViewModel"/> class.
        /// </summary>
        /// <param name="dependencies">The depenendencies.</param>
        public OrderPickingAcknowledgeSubstitutionViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {
        }
    }
}
