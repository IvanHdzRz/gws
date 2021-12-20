//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System;
    using System.Threading.Tasks;
    using GuidedWorkRunner;

    /// <summary>
    /// The marker interface to the Warehouse Picking Mobile data exchange.
    /// </summary>
    public interface IWarehousePickingMobileDataExchange : IMobileDataExchange
    {
    }

    /// <summary>
    /// The marker interface to the Warehouse Picking Mobile data exchange.
    /// </summary>
    public class WarehousePickingMobileDataExchange : IWarehousePickingMobileDataExchange
    {
        private readonly IWarehousePickingModel _Model;
        private readonly IWarehousePickingIntentBuilder _IntentBuilder;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:WarehousePicking.WarehousePickingMobileDataExchange"/> class.
        /// </summary>
        /// <param name="model">Model.</param>
        /// <param name="intentBuilder">Intent builder.</param>
        public WarehousePickingMobileDataExchange(IWarehousePickingModel model, IWarehousePickingIntentBuilder intentBuilder)
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
                case State.DisplaySubcenters:
                    intentToReturn = _IntentBuilder.EncodeSubcenters(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayLabelPrinter:
                    intentToReturn = _IntentBuilder.EncodeLabelPrinter(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayPickTripInfo:
                    intentToReturn = _IntentBuilder.EncodePickTripInfo(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayAcknowledgeLocation:
                    intentToReturn = _IntentBuilder.EncodeAcknowledgeLocation(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayEnterProduct:
                    intentToReturn = _IntentBuilder.EncodeEnterProduct(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayEnterQuantity:
                    intentToReturn = _IntentBuilder.EncodeEnterQuantity(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayConfirmQuantity:
                    intentToReturn = _IntentBuilder.EncodeConfirmQuantity(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayConfirmNoMore:
                    intentToReturn = _IntentBuilder.EncodeConfirmNoMore(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayConfirmSkipProduct:
                    intentToReturn = _IntentBuilder.EncodeConfirmSkipProduct(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayPickOrderStatus:
                    intentToReturn = _IntentBuilder.EncodeOrderStatus(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayPickOrderSummary:
                    intentToReturn = _IntentBuilder.EncodeOrderSummary(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayPickPerformance:
                    intentToReturn = _IntentBuilder.EncodePickPerformance(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayLastPick:
                    intentToReturn = _IntentBuilder.EncodeLastPick(_Model.DataStore.SerializeObject());
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
                case State.DisplaySubcenters:
                    _Model.SelectedSubcenter = _IntentBuilder.DecodeSubcenters(slots);
                    break;
                case State.DisplayLabelPrinter:
                    _Model.LabelPrinter = _IntentBuilder.DecodeLabelPrinter(slots);
                    break;
                case State.DisplayPickTripInfo:
                    _Model.AcknowledgePickTripInfo = _IntentBuilder.DecodePickTripInfo(slots);
                    break;
                case State.DisplayAcknowledgeLocation:
                    _Model.AcknowledgedLocation = _IntentBuilder.DecodeAcknowledgeLocation(slots);
                    break;
                case State.DisplayEnterProduct:
                    _Model.EnteredProduct = _IntentBuilder.DecodeEnterProduct(slots);
                    break;
                case State.DisplayEnterQuantity:
                    _Model.EnteredQuantityString = _IntentBuilder.DecodeEnterQuantity(slots);
                    break;
                case State.DisplayConfirmQuantity:
                    _Model.ShortProductConfirmation = _IntentBuilder.DecodeConfirmQuantity(slots);
                    break;
                case State.DisplayConfirmNoMore:
                    _Model.NoMoreConfirmation = _IntentBuilder.DecodeConfirmNoMore(slots);
                    break;
                case State.DisplayConfirmSkipProduct:
                    _Model.SkipProductConfirmation = _IntentBuilder.DecodeConfirmSkipProduct(slots);
                    break;
                case State.DisplayPickOrderStatus:
                    _Model.AcknowledgePickOrderStatus = _IntentBuilder.DecodePickOrderStatus(slots);
                    break;
                case State.DisplayPickOrderSummary:
                    _Model.AcknowledgePickOrderSummary = _IntentBuilder.DecodePickOrderSummary(slots);
                    break;
                case State.DisplayPickPerformance:
                    _Model.AcknowledgePickPerformance = _IntentBuilder.DecodePickPerformance(slots);
                    break;
                case State.DisplayLastPick:
                    _Model.AcknowledgeLastPick = _IntentBuilder.DecodeLastPick(slots);
                    break;
            }

            return _Model.ProcessUserInputAsync();
        }
    }
}
