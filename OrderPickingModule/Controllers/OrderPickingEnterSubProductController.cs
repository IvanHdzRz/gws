//////////////////////////////////////////////////////////////////////////////
//     Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWork;
    using GuidedWorkRunner;

    /// <summary>
    /// Controller for a WFA where the user enters the Identifier for the substitute product to pick.
    /// </summary>
    public class OrderPickingEnterSubProductController : OrderPickingEnterProductController
    {
        public OrderPickingEnterSubProductController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) : 
        base(dependencies, guidedWorkRunner, guidedWorkStore)
        {
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            _ViewModel = (OrderPickingEnterDigitsViewModel)base.CreateViewModel(viewModelName);
            _ViewModel.UserVocab.Remove(GetLocalizedText("VocabWord_SkipProduct"));
            string location = LocationParser.ParseServerFormatLocation(_DataStore.LocationText, ", ");
            InfoGlobalWordPrompt = GetLocalizedText("SubstitutionInfoPrompt", _DataStore.ProductName, location, _DataStore.OriginalProductName);
            _ViewModel.SkipProductVisible = false;

            //change the initial prompt based on whether the current product being processed is the first substitution
            if (_DataStore.IsFirstSubstitution)
            {
                _ViewModel.InitialPrompt = GetLocalizedText("SubstitutionsAvailablePrompt", _DataStore.ProductName);
            }
            else
            {
                _ViewModel.InitialPrompt = _DataStore.ProductName;
            }

            return _ViewModel;
        }
    }
}
