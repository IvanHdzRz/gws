//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;

    public class PickPromptSingleStateMachine : PickPromptStateMachine
    {
        public PickPromptSingleStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
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
                NextState = VerifyProductSlot;
                // TODO: support short product, skip slot, and partial
                if (_CurrentCheckDigit == VoiceLinkModuleVocab.ShortProduct.IdentificationKey)
                {
                    CurrentUserMessage = "Short product not supported";
                    MessageType = UserMessageType.Warning;
                    NextState = DisplayCheckDigitEntry;
                }
                else if (_CurrentCheckDigit == VoiceLinkModuleVocab.SkipSlot.IdentificationKey)
                {
                    CurrentUserMessage = "Skip slot not supported";
                    MessageType = UserMessageType.Warning;
                    NextState = DisplayCheckDigitEntry;
                }
                else if (_CurrentCheckDigit == VoiceLinkModuleVocab.Partial.IdentificationKey)
                {
                    CurrentUserMessage = "Partial not supported";
                    MessageType = UserMessageType.Warning;
                    NextState = DisplayCheckDigitEntry;
                }
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
                    _PickedQuantity = _ExpectedQuantity;
                    NextState = LotTracking;
                }
            }, LotTracking, DisplayEnterQuantity);

            ConfigureDisplayState(DisplayEnterQuantity, HandleEnterQuantityResponse, DisplayCheckDigitEntry,
                encodeAction: EncodeEnterQuantity, decodeAction: DecodeEnterQuantity);

            ConfigureLogicState(HandleEnterQuantityResponse, () =>
            {
                //TODO: Handle partial, and skip slot responses
                if (_CurrentQuantityPicked == VoiceLinkModuleVocab.SkipSlot.IdentificationKey)
                {
                    CurrentUserMessage = "Skip slot not supported";
                    MessageType = UserMessageType.Warning;
                    NextState = DisplayEnterQuantity;
                }
                else if (_CurrentQuantityPicked == VoiceLinkModuleVocab.Partial.IdentificationKey)
                {
                    CurrentUserMessage = "Partial not supported";
                    MessageType = UserMessageType.Warning;
                    NextState = DisplayEnterQuantity;
                }
                else if (_CurrentQuantityPicked == null)
                {
                    CurrentUserMessage = Translate.GetLocalizedTextForKey("PickPrompt_EnterQuantity_Failed");
                    NextState = DisplayEnterQuantity;
                }
                else
                {
                    NextState = VerifyEnteredQuantity;
                    var parsed = int.TryParse(_CurrentQuantityPicked, out _PickedQuantity);

                    if (!parsed)
                    {
                        CurrentUserMessage = Translate.GetLocalizedTextForKey("PickPrompt_EnterQuantity_Failed");
                        NextState = DisplayEnterQuantity;
                    }
                    else if (_PickedQuantity == _ExpectedQuantity)
                    {
                        Model.Partial = false;
                    }
                }
            }, DisplayEnterQuantity, VerifyEnteredQuantity);
        }
    }
}
