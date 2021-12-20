//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWorkRunner;

    /// <summary>
    /// Controller for a WFA where the user enters the quantity picked.
    /// </summary>
    public class ReceivingEnterTiQuantityController : ReceivingEnterValueController
    {
        public const string ConfirmQuantityEventName = "ConfirmQuantity";

        public ReceivingEnterTiQuantityController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) :
        base(dependencies, guidedWorkRunner, guidedWorkStore)
        {
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var viewModel = (ReceivingEnterDigitsViewModel)base.CreateViewModel(viewModelName);

            viewModel.HiQuantityPicked = DataStore.HiQuantityLastReceived;

            return viewModel;
        }



        private void ResetUiToInitialState()
        {
            var viewModel = (ReceivingEnterDigitsViewModel)ViewModel;
            viewModel.Response = string.Empty;
            viewModel.InitialPrompt = GetLocalizedText("InitialPrompt");
            viewModel.ErrorMessage = string.Empty;
        }
    }
}
