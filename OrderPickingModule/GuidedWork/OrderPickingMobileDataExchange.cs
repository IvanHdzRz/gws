//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Threading.Tasks;
    using GuidedWorkRunner;

    /// <summary>
    /// The marker interface to the Order Picking Mobile data exchange.
    /// </summary>
    public interface IOrderPickingMobileDataExchange : IMobileDataExchange
    {
    }

    /// <summary>
    /// The Order Picking Mobile data exchange.
    /// </summary>
    public class OrderPickingMobileDataExchange : IOrderPickingMobileDataExchange
    {
        private readonly IOrderPickingModel _Model;
        private readonly IOrderPickingIntentBuilder _IntentBuilder;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:OrderPicking.OrderPickingMobileDataExchange"/> class.
        /// </summary>
        /// <param name="model">Model.</param>
        /// <param name="intentBuilder">Intent builder.</param>
        public OrderPickingMobileDataExchange(IOrderPickingModel model, IOrderPickingIntentBuilder intentBuilder)
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
                case State.DisplayGetContainers:
                    intentToReturn = _IntentBuilder.EncodeGetContainers(_Model.DataStore.SerializeObject());
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
                case State.DisplayEnterSubProduct:
                    intentToReturn = _IntentBuilder.EncodeEnterSubProduct(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayConfirmOverflow:
                    intentToReturn = _IntentBuilder.EncodeConfirmOverflow(_Model.DataStore.SerializeObject());
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
                case State.DisplayGoToStagingLocation:
                    intentToReturn = _IntentBuilder.EncodeGoToStaging(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayEnterStagingLocation:
                    intentToReturn = _IntentBuilder.EncodeEnterStagingLocation(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayConfirmStagingLocation:
                    intentToReturn = _IntentBuilder.EncodeConfirmStagingLocation(_Model.DataStore.SerializeObject());
                    break;
                case State.DisplayAllDone:
                    intentToReturn = _IntentBuilder.EncodeAllDone(_Model.DataStore.SerializeObject());
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
                case State.DisplayGetContainers:
                    _Model.GetContainersResponse = _IntentBuilder.DecodeGetContainers(slots);
                    break;
                case State.DisplayAcknowledgeLocation:
                    _Model.AcknowledgeLocationResponse = _IntentBuilder.DecodeAcknowledgeLocation(slots);
                    break;
                case State.DisplayEnterProduct:
                    _Model.EnteredProduct = _IntentBuilder.DecodeEnterProduct(slots);
                    break;
                case State.DisplayEnterQuantity:
                    _Model.EnteredQuantityString = _IntentBuilder.DecodeEnterQuantity(slots);
                    break;
                case State.DisplayConfirmQuantity:
                    _Model.ProductQuantityConfirmation = _IntentBuilder.DecodeConfirmQuantity(slots);
                    break;
                case State.DisplayEnterSubProduct:
                    _Model.EnteredProduct = _IntentBuilder.DecodeEnterSubProduct(slots);
                    break;
                case State.DisplayConfirmOverflow:
                    _Model.OverflowConfirmation = _IntentBuilder.DecodeConfirmOverflow(slots);
                    break;
                case State.DisplayConfirmNoMore:
                    _Model.NoMoreConfirmation = _IntentBuilder.DecodeConfirmNoMore(slots);
                    break;
                case State.DisplayConfirmSkipProduct:
                    _Model.SkipProductConfirmation = _IntentBuilder.DecodeConfirmSkipProduct(slots);
                    break;
                case State.DisplayGoToStagingLocation:
                    _Model.GoToStagingResponse = _IntentBuilder.DecodeGoToStaging(slots);
                    break;
                case State.DisplayEnterStagingLocation:
                    _Model.StagingLocationResponse = _IntentBuilder.DecodeEnterStagingLocation(slots);
                    break;
                case State.DisplayConfirmStagingLocation:
                    _Model.StagingLocationConfirmation = _IntentBuilder.DecodeConfirmStagingLocation(slots);
                    break;
            }

            return _Model.ProcessUserInputAsync();
        }
    }
}
