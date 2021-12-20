//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWorkRunner;

    /// <summary>
    /// Controller for a WFA that confirms the quantity picked for a receiving
    /// work item and updates the model as appropriate.
    /// </summary>
    public class ReceivingConfirmQuantityController : ReceivingBooleanConfirmationController
    {
        public ReceivingConfirmQuantityController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore)
            : base(dependencies, guidedWorkRunner, guidedWorkStore)
        {
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var viewModel = base.CreateViewModel(viewModelName) as ReceivingBooleanConfirmationViewModel;

            var dataStore = DataStore;

            viewModel.InitialPrompt = GetLocalizedText("InitialPrompt", dataStore.QuantityLastReceived);
            viewModel.HiLabel = GetLocalizedText("HiLabel");
            viewModel.TiLabel = GetLocalizedText("TiLabel");
            viewModel.TotalLabel = GetLocalizedText("TotalLabel");
            viewModel.HiQuantityPicked = dataStore.HiQuantityLastReceived;
            viewModel.TiQuantityPicked = dataStore.TiQuantityLastReceived;
            viewModel.TotalQuantityPicked = dataStore.QuantityLastReceived;

            return viewModel;
        }
    }
}
