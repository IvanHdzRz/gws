//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWork;

    /// <summary>
    /// Controller that allows a user to acknoledge that a substitution has been made.
    /// </summary>
    public class OrderPickingAcknowledgeSubstitutionController : SingleResponseController
    {
        private readonly IOrderPickingModel _Model;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderPickingAcknowledgeLocationController"/> class.
        /// </summary>
        public OrderPickingAcknowledgeSubstitutionController(CoreViewControllerDependencies dependencies, IOrderPickingModel orderPickingModel) : base(dependencies)
        {
            _Model = orderPickingModel;
            PublishesAsOptionEvent = true;

            _Model.SetSubstitutedProduct();
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var viewModel = (OrderPickingAcknowledgeSubstitutionViewModel)base.CreateViewModel(viewModelName);
            viewModel.ExpectedStockCodeResponseLength = _Model.ExpectedStockCodeResponseLength;
            viewModel.LastDigitsLabel = GetLocalizedText("LastDigits", _Model.ExpectedStockCodeResponseLength.ToString());
            viewModel.ReadyVocabWord = GetLocalizedText("accept_entry_word");
            viewModel.NextVocabWord = GetLocalizedText("next_entry_word");

            //TODO: Order Picking
            var prompt = GetLocalizedText("InitialPrompt", _Model.RemainingQuantity.ToString(), viewModel.NextVocabWord);
            viewModel.InitialPrompt = prompt;

            string location = LocationParser.ParseServerFormatLocation(_Model.CurrentProduct.LocationText, ", ");

            viewModel.ProductImageGridSubviewModel = new ProductImageGridSubviewModel
            {
                Header = prompt,
                ProductImage = _Model.CurrentImagePath,
                ProductName = _Model.CurrentProduct.Name,
                ProductDescription = _Model.CurrentProduct.Description,
                ProductIdentifier = _Model.CurrentProduct.ProductIdentifier,
                Location = location
            };

            viewModel.OrderNumber = _Model.OrderIdentifier;
            return viewModel;
        }

        protected override void OnSuccess(string response)
        {
            var viewModel = (OrderPickingAcknowledgeSubstitutionViewModel)ViewModel;

            if (viewModel.ReadyVocabWord == response || viewModel.NextVocabWord == response)
            {
                base.OnSuccess(response);
                return;
            }
            string msg = GetLocalizedText("Error_UnexpectedResponse", response);
            throw new Exception(msg);
        }
    }
}
