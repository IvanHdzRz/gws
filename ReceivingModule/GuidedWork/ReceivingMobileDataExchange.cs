//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System;
    using System.Threading.Tasks;
    using GuidedWorkRunner;

    /// <summary>
    /// The marker interface to the Receiving Mobile data exchange.
    /// </summary>
    public interface IReceivingMobileDataExchange : IMobileDataExchange
    {
    }

    /// <summary>
    /// The marker interface to the Receiving Mobile data exchange.
    /// </summary>
    public class ReceivingMobileDataExchange : IReceivingMobileDataExchange
    {
        private readonly IReceivingModel _Model;
        private readonly IReceivingIntentBuilder _IntentBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Receiving.ReceivingMobileDataExchange"/> class.
        /// </summary>
        /// <param name="model">Model.</param>
        /// <param name="intentBuilder">Intent builder.</param>
        public ReceivingMobileDataExchange(IReceivingModel model, IReceivingIntentBuilder intentBuilder)
        {
            _Model = model;
            _IntentBuilder = intentBuilder;
        }

        /// <summary>
        /// Reset this instance.
        /// </summary>
        public void Reset()
        {
        }

        /// <summary>
        /// Authenticates with the exchange asynchrously.
        /// </summary>
        /// <returns>The asynchronous Task.</returns>
        public Task AuthenticateAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Requests an encoded intent asynchronously.
        /// </summary>
        /// <returns>The asynchronous Task.</returns>
        public Task<string> RequestAsync()
        {
            string intentToReturn = string.Empty;

            switch (_Model.StateMachine.CurrentState)
            {
                case State.DisplaySignIn:
                    intentToReturn = _IntentBuilder.EncodeSignIn(_Model.CurrentUserMessage);
                    break;
                case State.StartOperPrep:
                    intentToReturn = _IntentBuilder.EncodeOperPrep();
                    break;
                case State.DisplayOrders:
                    intentToReturn = _IntentBuilder.EncodeOrders(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayHiQuantity:
                    intentToReturn = _IntentBuilder.EncodeHiQuantity(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayTiQuantity:
                    intentToReturn = _IntentBuilder.EncodeTiQuantity(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayConfirmQuantity:
                    intentToReturn = _IntentBuilder.EncodeConfirmQuantity(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayPrintingLabel:
                    intentToReturn = _IntentBuilder.EncodePrintingLabel(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayApplyLabel:
                    intentToReturn = _IntentBuilder.EncodeApplyLabel(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayPalletCondition:
                    intentToReturn = _IntentBuilder.EncodePalletCondition(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayDamagedReason:
                    intentToReturn = _IntentBuilder.EncodeDamageReason(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayInvoiceSummary:
                    intentToReturn = _IntentBuilder.EncodeInvoiceSummary(_Model.DataStore.SerializeObject());
                    break;
                default:
                    intentToReturn = _IntentBuilder.EncodeBackgroundActivity("Retrieving Data");
                    break;
            }

            return Task.FromResult(intentToReturn);
        }

        /// <summary>
        /// Updates the associated model with the slot asychronosuly.
        /// </summary>
        /// <returns>The asynchronous Task.</returns>
        /// <param name="slots">Slots.</param>
        public Task RespondAsync(string slots)
        {
            switch (_Model.StateMachine.CurrentState)
            {
                case State.DisplaySignIn:
                    Tuple<string, string> userProps = _IntentBuilder.DecodeSignIn(slots);
                    _Model.UpdateUser(userProps.Item1, userProps.Item2);
                    break;
                case State.DisplayOrders:
                    _Model.SelectedOrder = _IntentBuilder.DecodeOrder(slots);
                    break;
                case State.DisplayHiQuantity:
                    _Model.EnteredHiQuantity = _IntentBuilder.DecodeHiQuantity(slots);
                    break;
                case State.DisplayTiQuantity:
                    _Model.EnteredTiQuantity = _IntentBuilder.DecodeTiQuantity(slots);
                    break;
                case State.DisplayConfirmQuantity:
                    _Model.QuantityConfirmation = _IntentBuilder.DecodeConfirmQuantity(slots);
                    break;
                case State.DisplayPrintingLabel:
                    _Model.AcknowledgePrintingLabel = _IntentBuilder.DecodePrintingLabel(slots);
                    break;
                case State.DisplayApplyLabel:
                    _Model.AcknowledgeApplyLabel = _IntentBuilder.DecodeApplyLabel(slots);
                    break;
                case State.DisplayPalletCondition:
                    _Model.PalletCondition = _IntentBuilder.DecodePalletCondition(slots);
                    break;
                case State.DisplayDamagedReason:
                    _Model.DamageReason = _IntentBuilder.DecodeDamageReason(slots);
                    break;
                case State.DisplayInvoiceSummary:
                    _Model.AcknowledgeInvoiceSummary = _IntentBuilder.DecodeInvoiceSummary(slots);
                    break;
            }

            return _Model.ProcessUserInputAsync();
        }
    }
}
