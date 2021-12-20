//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;

    public class PickPromptMultipleStateMachine : PickPromptStateMachine
    {
        public static readonly CoreAppSMState DisplayNotEnterQuantity = new CoreAppSMState(nameof(DisplayNotEnterQuantity));
        public static readonly CoreAppSMState HandleNotEnterQuantityResponse = new CoreAppSMState(nameof(HandleNotEnterQuantityResponse));

        public PickPromptMultipleStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            base.ConfigureStates();

            //--------------------------------------------------------------------------
            //Check Digit Prompt
            ConfigureLogicState(SlotVerification, () => { NextState = DisplayCheckDigitEntry; }, DisplayCheckDigitEntry);

            ConfigureDisplayState(DisplayCheckDigitEntry, HandleCheckDigitResponse, encodeAction: EncodeCheckDigitEntry, decodeAction: DecodeCheckDigitEntry);

            ConfigureLogicState(HandleCheckDigitResponse, () =>
            {
                // TODO: support skip slot
                if (_CurrentCheckDigit == VoiceLinkModuleVocab.SkipSlot.IdentificationKey)
                {
                    CurrentUserMessage = "Skip slot not supported";
                    MessageType = UserMessageType.Warning;
                    NextState = DisplayCheckDigitEntry;
                    return;
                }
                NextState = VerifyProductSlot;
            }, DisplayCheckDigitEntry, VerifyProductSlot);

            //--------------------------------------------------------------------------
            //Quantity prompts
            ConfigureLogicState(EnterQuantity, () =>
            {
                // Quantity verification on, or shorting, or partial, or qty verification failed   
                if (PickingRegionsResponse.CurrentPickingRegion.QuantityVerification || _ShortProduct || Model.Partial == true)
                {
                    NextState = DisplayEnterQuantity;
                }
                else
                {
                    NextState = DisplayNotEnterQuantity;
                }
            }, DisplayNotEnterQuantity, DisplayEnterQuantity);

            ConfigureDisplayState(DisplayNotEnterQuantity, HandleNotEnterQuantityResponse, DisplayCheckDigitEntry,
                encodeAction: EncodeNotEnterQuantity, decodeAction: DecodeNotEnterQuantity);

            ConfigureDisplayState(DisplayEnterQuantity, HandleEnterQuantityResponse, DisplayCheckDigitEntry,
                encodeAction: EncodeEnterQuantity, decodeAction: DecodeEnterQuantity);

            ConfigureLogicState(HandleEnterQuantityResponse, () =>
            {
                NextState = VerifyEnteredQuantity;
                if (!HandleOverflowVocabs())
                {
                    var parsed = int.TryParse(_CurrentQuantityPicked, out _PickedQuantity);

                    if (!parsed)
                    {
                        CurrentUserMessage = Translate.GetLocalizedTextForKey("PickPrompt_EnterQuantity_Failed");
                        NextState = DisplayEnterQuantity;
                    }
                    else if (_PickedQuantity == _ExpectedQuantity)
                    {
                        Model.Partial = false;
                        _ShortProduct = false;
                    }
                }
            }, DisplayEnterQuantity, VerifyEnteredQuantity);

            ConfigureLogicState(HandleNotEnterQuantityResponse, () =>
            {
                if (!HandleOverflowVocabs())
                {
                    _PickedQuantity = _ExpectedQuantity;
                    NextState = VerifyEnteredQuantity;
                }
            }, VerifyEnteredQuantity);

        }

        private bool HandleOverflowVocabs()
        {
            //TODO: Handle partial, skip slot, and short product responses
            if (_CurrentQuantityPicked == VoiceLinkModuleVocab.SkipSlot.IdentificationKey)
            {
                CurrentUserMessage = "Skip slot not supported";
                MessageType = UserMessageType.Warning;
                return true;
            }
            else if (_CurrentQuantityPicked == VoiceLinkModuleVocab.Partial.IdentificationKey)
            {
                CurrentUserMessage = "Partial not supported";
                MessageType = UserMessageType.Warning;
                return true;
            }
            else if (_CurrentQuantityPicked == VoiceLinkModuleVocab.ShortProduct.IdentificationKey)
            {
                CurrentUserMessage = "Short not supported";
                MessageType = UserMessageType.Warning;
                return true;
            }
            else if (_CurrentQuantityPicked == null)
            {
                CurrentUserMessage = Translate.GetLocalizedTextForKey("PickPrompt_EnterQuantity_Failed");
                return true;
            }
            return false;
        }

        #region EncodersDecoders

        private WorkflowObjectContainer EncodeNotEnterQuantity(IVoiceLinkModel model)
        {

            // Quantity verification on, or shorting, or partial, or qty verification failed   
            var prompt = Translate.GetLocalizedTextForKey("PickPrompt_Multiple_EnterQuantity_Quantity",
                _ExpectedQuantity.ToString(), _CurrentPick.UOM, _IdDescription, _Description, _CurrentPick.PromptMessage);
            if (_ShortProduct)
            {
                prompt = Translate.GetLocalizedTextForKey("PickPrompt_EnterQuantity_Short");
            }
            else if (model.Partial == true)
            {
                prompt = Translate.GetLocalizedTextForKey("PickPrompt_EnterQuantity_Partial");
            }

            var uiElements = GeneratePickDisplayElements(model.CurrentPick);

            var wfoContainer = new WorkflowObjectContainer();
            WorkflowObject wfo = null;
            wfo = WorkflowObjectFactory.CreateReadyIntent(prompt,
                                                            "quantity",
                                                            prompt,
                                                            model.CurrentUserMessage,
                                                            initialPrompt: model.CurrentUserMessage);

            wfo.MessageType = model.MessageType;

            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(VoiceLinkModuleVocab.SkipSlot));
            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(VoiceLinkModuleVocab.Partial));
            if (model.Partial == true)
            {
                wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(VoiceLinkModuleVocab.ShortProduct));
            }

            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeNotEnterQuantity(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            _CurrentQuantityPicked = GenericBaseEncoder<IVoiceLinkModel>.DecodeValueEntry(slotContainer);
        }
        #endregion

    }
}
