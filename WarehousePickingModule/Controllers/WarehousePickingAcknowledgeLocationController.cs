//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System;
    using System.Collections.Generic;
    using Honeywell.DialogueRunner;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWorkRunner;

    /// <summary>
    /// Controller for WFA that allows a user to acknowledge that the correct location has been reached.
    /// </summary>
    public class WarehousePickingAcknowledgeLocationController : SingleResponseController
    {
        private readonly IGuidedWorkRunner _GuidedWorkRunner;
        private readonly IGuidedWorkStore _GuidedWorkStore;

        private WarehousePickingDataStore _DataStore => WarehousePickingDataStore.DeserializeObject(_GuidedWorkStore.GetActiveWorkflowObject().SerializedData);

        /// <summary>
        /// Initializes a new instance of the <see cref="WarehousePickingAcknowledgeLocationController"/> class.
        /// </summary>
        public WarehousePickingAcknowledgeLocationController(CoreViewControllerDependencies dependencies, IGuidedWorkRunner guidedWorkRunner, IGuidedWorkStore guidedWorkStore) : base(dependencies)
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
            var viewModel = (WarehousePickingAcknowledgeLocationViewModel)base.CreateViewModel(viewModelName);

            viewModel.InitialPrompt = GetInitialPrompt(_DataStore.Aisle);
            viewModel.TripIdentifier = _DataStore.TripIdentifier;
            viewModel.ProductIdentifier = _DataStore.ProductIdentifier;
            viewModel.ProductImage = _DataStore.ProductImage;
            viewModel.ProductName = _DataStore.ProductName;
            viewModel.RemainingQuantity = _DataStore.RemainingQuantity.ToString();
            InfoGlobalWordPrompt = _DataStore.ProductDescription ?? _DataStore.ProductName;
            viewModel.ReadyVocabWord = GetLocalizedText("accept_entry_word");
            viewModel.NextVocabWord = GetLocalizedText("next_entry_word");
            viewModel.SkipProductVocabWord = GetLocalizedText("VocabWord_SkipProduct");
            viewModel.CancelVocabWord = GetLocalizedText("VocabWord_EndOrder");
            viewModel.LocationDescriptors = _DataStore.LocationDescriptors;
            viewModel.Instructions = TranslateExtension.GetLocalizedTextForBaseKey("Instructions");
            viewModel.CurrentProductIndex = _DataStore.CurrentProductIndex;
            viewModel.TotalProducts = _DataStore.TotalProducts;

            viewModel.OverflowMenuItems = OverflowMenuItems;
            viewModel.PossibleResponses.Add(OverflowMenuItems);

            return viewModel;
        }

        /// <summary>
        /// Since we are adding our leaf WFA's into the BaldEagleWorkflowActivity composite,
        /// we cannot use the normal global word / overflow mechanism. So we have added a new
        /// OverflowMenuItems WFA param that sets these (which is nice now that we add them to
        /// the WFA that needs them, and don't have to mark them as ignored everywhere else).
        /// The controller now decides what to do with the menu item, whether it is spoken or
        /// selected in overflow menu.In most cases we send the menu item as a "button" in the
        /// ExtraData of the slot.In the case of "info" we had to treat it as an invalid entry,
        /// so that we could use the ValidatingWorkflowViewController's SignalInvalidResponseAsync
        /// command to cause the InfoGlobalWordPrompt to be spoken as the error message. This
        /// allows us to stay on the same WFA (screen) and restart the dialogue.
        /// </summary>
        protected override bool ValidateResponse(string response)
        {
            bool validResponse = base.ValidateResponse(response);

            if (validResponse)
            {
                if (Translate.GetLocalizedTextForKey("VocabWord_Info") == response)
                {
                    var viewModel = (WarehousePickingAcknowledgeLocationViewModel)ViewModel;
                    viewModel.ValidationModel.DefaultInvalidResponseMessage = InfoGlobalWordPrompt;
                    return false;
                }

                return true;
            }

            return false;
        }

        protected override void OnSuccess(string response)
        {
            var viewModel = (WarehousePickingAcknowledgeLocationViewModel)ViewModel;

            if (viewModel.SkipProductVocabWord == response ||
                viewModel.ReadyVocabWord == response ||
                viewModel.NextVocabWord == response ||
                viewModel.CancelVocabWord == response ||
                Translate.GetLocalizedTextForKey("VocabWord_NoMore") == response ||
                Translate.GetLocalizedTextForKey("VocabWord_LastPick") == response ||
                Translate.GetLocalizedTextForKey("VocabWord_OrderStatus") == response)
            {
                _GuidedWorkStore.UpdateActiveObjectExtraData("Button", response);
                return;
            }

            string msg = GetLocalizedText("Error_UnexpectedResponse", response);
            throw new Exception(msg);
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

        private readonly Dictionary<string, string> _PhoneticAlphabetMap = new Dictionary<string, string>()
        {
            {  "A", "Alpha" },
            {  "B", "Bravo" },
            {  "C", "Charlie" },
            {  "D", "Delta" },
            {  "E", "Echo" },
            {  "F", "Foxtrot" },
            {  "G", "Golf" },
            {  "H", "Hotel" },
            {  "I", "India" },
            {  "J", "Juliet" },
            {  "K", "Kilo" },
            {  "L", "Lima" },
            {  "M", "Mike" },
            {  "N", "November" },
            {  "O", "Oscar" },
            {  "P", "Papa" },
            {  "Q", "Quebec" },
            {  "R", "Romeo" },
            {  "S", "Sierra" },
            {  "T", "Tango" },
            {  "U", "Uniform" },
            {  "V", "Victor" },
            {  "W", "Whiskey" },
            {  "X", "X-ray" },
            {  "Y", "Yankee" },
            {  "Z", "Zulu" }
        };

        private string GetInitialPrompt(string aisle)
        {
            var words = new List<string>();
            CommonDialogueUtils.SplitNumberStringIntoWords(aisle, ref words);

            string locationString = string.Empty;
            foreach (var word in words)
            {
                string wordToAdd = word;
                if (_PhoneticAlphabetMap.ContainsKey(word))
                {
                    wordToAdd = _PhoneticAlphabetMap[word];
                }

                locationString += " " + wordToAdd;
            }

            return GetLocalizedText("InitialPrompt", locationString);
        }

        private async void OnStoreUpdated()
        {
            await _GuidedWorkRunner.RespondAsync();
            await _GuidedWorkRunner.RequestAsync();
            PublishWorkflowActivityEvent(_GuidedWorkRunner.WorkflowEventName);
        }
    }
}
