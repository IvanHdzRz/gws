//////////////////////////////////////////////////////////////////////////////
//     Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using GuidedWorkRunner;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Honeywell.Firebird.WorkflowEngine;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Controller that instructs a worker how many orders there are, to get containers, and waits until they are ready
    /// </summary>
    public class OrderPickingGetContainersController : SingleResponseController
    {

        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        private OrderPickingDataStore _DataStore => OrderPickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        private readonly List<string> CancelVocab;
        private readonly List<string> ReadyVocab;

        public OrderPickingGetContainersController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) : 
        base(dependencies)
        {
            CancelVocab = new List<string> { GetLocalizedText("VocabWord_EndOrder") };
            ReadyVocab = new List<string> { GetLocalizedText("next_entry_word"), GetLocalizedText("accept_entry_word") };
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
            var viewModel = (SingleResponseViewModel)base.CreateViewModel(viewModelName);

            viewModel.PossibleResponses.Add(CancelVocab);
            viewModel.PossibleResponses.Add(ReadyVocab);

            int containerCount = _DataStore.Containers.Count;
            if (containerCount == 1)
            {
                viewModel.InitialPrompt = GetLocalizedText("InitialPromptSingular", containerCount.ToString());
            }
            else
            {
                viewModel.InitialPrompt = GetLocalizedText("InitialPromptPlural", containerCount.ToString());
            }

            InfoGlobalWordPrompt = GetLocalizedText("Instructions");

            return viewModel;
        }

        protected override void OnSuccess(string response)
        {
            if (CancelVocab.Contains(response) || ReadyVocab.Contains(response))
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
