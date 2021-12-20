//////////////////////////////////////////////////////////////////////////////
//     Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWorkRunner;

    /// <summary>
    /// Abstract controller that encapulates the common features of the digit entry WFAs for Order Picking.
    /// </summary>
    public abstract class ReceivingEnterValueController : DigitEntryController
    {
        protected readonly IGuidedWorkRunner GuidedWorkRunner;
        protected readonly IGuidedWorkStore GuidedWorkStore;

        protected ReceivingDataStore DataStore => ReceivingDataStore.DeserializeObject(GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        protected ReceivingEnterValueController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) : 
        base(dependencies)
        {
            GuidedWorkRunner = guidedWorkRunner;
            GuidedWorkStore = guidedWorkStore;
        }

        private IReadOnlyList<string> _OverflowMenuItems;

        private IReadOnlyList<string> OverflowMenuItems
        {
            get
            {
                if (_OverflowMenuItems == null)
                {
                    _OverflowMenuItems = GetWorkflowParamAs<List<string>>("OverflowMenuItems");
                    if (null == _OverflowMenuItems)
                    {
                        _OverflowMenuItems = new List<string>();
                    }
                    else
                    {
                        var translatedOverflowMenuItems = new List<string>();
                        foreach (var overflowMenuItem in _OverflowMenuItems)
                        {
                            string translatedOverflowMenuItem = GetLocalizedText(overflowMenuItem);
                            translatedOverflowMenuItems.Add(translatedOverflowMenuItem);
                        }
                        _OverflowMenuItems = translatedOverflowMenuItems;
                    }
                }

                return _OverflowMenuItems;
            }
        }

        public string InfoGlobalWordPrompt { get; set; }

        public override bool ShouldAllowBackNavigation()
        {
            GuidedWorkStore.UpdateActiveObjectExtraData("Button", "NavigateBack");
            return false;
        }

        protected override void OnStart(NavigationReason reason)
        {
            base.OnStart(reason);
            GuidedWorkStore.StoreUpdated += OnStoreUpdated;
        }

        protected override void OnStop()
        {
            base.OnStop();
            GuidedWorkStore.StoreUpdated -= OnStoreUpdated;
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var viewModel = (ReceivingEnterDigitsViewModel)base.CreateViewModel(viewModelName);

            viewModel.OrderIdentifier = "123";
           // viewModel.Price = Model.CurrentProduct.DisplayPrice;
           // viewModel.ProductName = Model.CurrentProduct.Name;
            //viewModel.Size = Model.CurrentProduct.Size;
            //viewModel.LocationDescriptors = Model.GetLocationDescriptorsForProduct();
            viewModel.RemainingQuantity = DataStore.RemainingQuantity;
            viewModel.Instructions = TranslateExtension.GetLocalizedTextForBaseKey("Instructions");
            viewModel.ProductIdentifier = DataStore.ProductIdentifier;
            viewModel.CurrentProductIndex = DataStore.CurrentProductIndex;
            viewModel.TotalProducts = DataStore.TotalProducts;

            viewModel.InitialPrompt = GetLocalizedText("InitialPrompt");
            viewModel.ProductName = DataStore.ProductName;
            viewModel.ProductImage = DataStore.ProductImage;
            viewModel.HiLabel = GetLocalizedText("HiLabel");
            viewModel.TiLabel = GetLocalizedText("TiLabel");
            viewModel.TotalLabel = GetLocalizedText("TotalLabel");

            InfoGlobalWordPrompt = DataStore.ProductDescription ?? DataStore.ProductName;

            viewModel.OverflowMenuItems = OverflowMenuItems;
            foreach (var overflowMenuItem in OverflowMenuItems)
            {
                viewModel.UserVocab.Add(overflowMenuItem);
            }

            return viewModel;
        }

        protected override bool ValidateResponse(string response)
        {
            if (!base.ValidateResponse(response))
            {
                return false;
            }

            // Check if the response is a valid vocab word
            if (IsInUserVocab(response))
            {
                if (GetLocalizedText("VocabWord_Info") == response)
                {
                    var viewModel = (ReceivingEnterDigitsViewModel)ViewModel;
                    viewModel.ErrorMessage = string.Empty;
                    viewModel.ValidationModel.DefaultInvalidResponseMessage = InfoGlobalWordPrompt;
                    return false;
                }
            }

            return true;
        }

        protected override void OnSuccess(string response)
        {
            var activeWorkflowObject = GuidedWorkStore.GetActiveWorkflowObject();

            if (IsInUserVocab(response))
            {
                activeWorkflowObject.ExtraData["Button"] = response;
            }
            else
            {
                var viewModel = (ReceivingEnterDigitsViewModel)ViewModel;
                viewModel.Response = response;
                activeWorkflowObject.Data = response;
            }

            activeWorkflowObject.Modified = true;
            GuidedWorkStore.UpdateCompleted();
        }

        protected override Task OnFailureAsync(string response)
        {
            var viewModel = (ReceivingEnterDigitsViewModel)ViewModel;
            if (!string.IsNullOrWhiteSpace(response))
            {
                viewModel.ErrorMessage = GetLocalizedText("Error_WrongQuantity");
            }
            return base.OnFailureAsync(response);
        }

        private async void OnStoreUpdated()
        {
            await GuidedWorkRunner.RespondAsync();
            await GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(GuidedWorkRunner.WorkflowEventName);
        }
    }
}
