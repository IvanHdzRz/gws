//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird;
    using System.Collections.Generic;

    /// <summary>
    /// Controller for WFA that allows a user to acknowledge that the correct location has been reached.
    /// </summary>
    public class OrderPickingAcknowledgeLocationController : SingleResponseController
    {
        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        private OrderPickingDataStore _DataStore => OrderPickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);


        /// <summary>
        /// Initializes a new instance of the <see cref="OrderPickingAcknowledgeLocationController"/> class.
        /// </summary>
        public OrderPickingAcknowledgeLocationController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) : base(dependencies)
        {
            _GuidedWorkRunner = guidedWorkRunner;
            _GuidedWorkStore = guidedWorkStore;
            PublishesAsOptionEvent = true;
        }

        public string InfoGlobalWordPrompt { get; set; }

        public override bool ShouldAllowBackNavigation()
        {
            _GuidedWorkStore.UpdateActiveObjectExtraData("Button", "NavigateBack");
            return false;
        }

        protected override void OnStart(NavigationReason reason)
        {
            base.OnStart(reason);
            _GuidedWorkStore.StoreUpdated += OnStoreUpdated;
        }

        protected override void OnStop()
        {
            base.OnStop();
            _GuidedWorkStore.StoreUpdated -= OnStoreUpdated;
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var viewModel = (OrderPickingAcknowledgeLocationViewModel)base.CreateViewModel(viewModelName);

            string location = LocationParser.ParseServerFormatLocation(_DataStore.LocationText, ", ");
            string initialPrompt = GetLocalizedText("InitialPrompt", location);

            viewModel.ExpectedStockCodeResponseLength = _DataStore.ExpectedStockCodeResponseLength;
            viewModel.InitialPrompt = initialPrompt;
            viewModel.LastDigitsLabel = GetLocalizedText("LastDigits", _DataStore.ExpectedStockCodeResponseLength.ToString());
            viewModel.OrderIdentifier = _DataStore.OrderIdentifier;
            viewModel.Price = _DataStore.Price;
            viewModel.ProductIdentifier = _DataStore.ProductIdentifier;
            viewModel.ProductImage = _DataStore.ProductImage;
            viewModel.ProductName = _DataStore.ProductName;
            viewModel.RemainingQuantity = _DataStore.RemainingQuantity.ToString();
            viewModel.Size = _DataStore.Size;
            viewModel.LocationDescriptors = _DataStore.LocationDescriptors;
            viewModel.Instructions = TranslateExtension.GetLocalizedTextForBaseKey("Instructions");
            viewModel.CurrentProductIndex = _DataStore.CurrentProductIndex.ToString();
            viewModel.TotalProducts = _DataStore.TotalProducts.ToString();
            viewModel.OverflowMenuItems = new List<string>
            {
                GetLocalizedText("VocabWord_OrderStatus")
            };
            viewModel.PossibleResponses.Add(GetLocalizedText("accept_entry_word"));
            viewModel.PossibleResponses.Add(GetLocalizedText("next_entry_word"));
            viewModel.PossibleResponses.Add(GetLocalizedText("VocabWord_SkipProduct"));
            viewModel.PossibleResponses.Add(GetLocalizedText("VocabWord_EndOrder"));
            viewModel.PossibleResponses.Add(GetLocalizedText("VocabWord_OrderStatus"));

            InfoGlobalWordPrompt = (_DataStore.ProductDescription != null) ? _DataStore.ProductDescription : _DataStore.ProductName;

            return viewModel;
        }

        protected override void OnSuccess(string response)
        {
            var viewModel = (OrderPickingAcknowledgeLocationViewModel)ViewModel;


            if (viewModel.PossibleResponses.Contains(response))
            {
                _GuidedWorkStore.UpdateActiveObjectExtraData("Button", response);
                return;
            }

            string msg = GetLocalizedText("Error_UnexpectedResponse", response);
            throw new ArgumentException(msg);
        }

        private async void OnStoreUpdated()
        {
            await _GuidedWorkRunner.RespondAsync();
            await _GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(_GuidedWorkRunner.WorkflowEventName);
        }
    }
}
