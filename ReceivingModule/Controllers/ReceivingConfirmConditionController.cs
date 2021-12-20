//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWorkRunner;

    /// <summary>
    /// Controller for a WFA that confirms the condition of a receiving
    /// work item and updates the model as appropriate.
    /// </summary>
    public class ReceivingConfirmConditionController : ReceivingBooleanConfirmationController
    {
        public ReceivingConfirmConditionController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore)
            : base(dependencies, guidedWorkRunner, guidedWorkStore)
        {
        }

        public override bool ShouldAllowBackNavigation()
        {
            return false;
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            var viewModel = (ReceivingBooleanConfirmationViewModel)base.CreateViewModel(viewModelName);

            viewModel.RemainingQuantity = DataStore.RemainingQuantity;
            viewModel.AffirmativeWord = GetLocalizedText("VocabWord_Good");
            viewModel.NegativeWord = GetLocalizedText("VocabWord_Damaged");

            return viewModel;
        }
    }
}
