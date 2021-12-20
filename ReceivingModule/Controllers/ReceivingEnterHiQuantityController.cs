//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using Honeywell.Firebird.CoreLibrary;
    using GuidedWorkRunner;

    /// <summary>
    /// Controller for a WFA where the user enters the quantity picked.
    /// </summary>
    public class ReceivingEnterHiQuantityController : ReceivingEnterValueController
    {
        public ReceivingEnterHiQuantityController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) :
        base(dependencies, guidedWorkRunner, guidedWorkStore)
        {
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
