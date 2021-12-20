//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Threading.Tasks;
    using LiquidState;
    using LiquidState.Awaitable.Core;
    using Common.Logging;
    using Retail;
    using Honeywell.Firebird;

    public class OrderPickingStateMachine : IOrderPickingStateMachine
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(OrderPickingStateMachine));

        private OrderPickingStateInternals _StateInternals;
        private IAwaitableStateMachine<State, Trigger> _StateMachine;
        private State _BackgroundActivityNextState;

        public OrderPickingStateMachine(IOrderPickingModel model, IRetailAcuityEventService retailAcuityEventService, IProductRequestModel productRequestModel)
        {
            _StateInternals = new OrderPickingStateInternals(model, retailAcuityEventService, productRequestModel);
        }

        public State CurrentState => _StateMachine.CurrentState;

        public void InitializeStateMachine()
        {
            var smConfig = StateMachineFactory.CreateAwaitableConfiguration<State, Trigger>();

            smConfig.ForState(State.BackgroundActvity)
                .PermitDynamic(Trigger.ExecuteBackgroundActivity,
                    () => DynamicState.Create(_BackgroundActivityNextState),
                    () => _Log.Debug("---->Leaving background activity state and going to " +
                                      _BackgroundActivityNextState.ToString()));

            smConfig.ForState(State.Start)
                .Permit(Trigger.ReturnUserInput, State.RequestData);

            smConfig.ForState(State.RequestData)
                .OnEntry(async () => await _StateInternals.RetrievePicksAsync())
                .Permit(Trigger.DataRequestFailed, State.DisplayAllDone)
                .Permit(Trigger.DataRequestSucceeded, State.DisplayGetContainers);

            smConfig.ForState(State.DisplayGetContainers)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandleGetContainers;
                });

            smConfig.ForState(State.HandleGetContainers)
                .OnEntry(async () => await _StateInternals.HandleGetContainers())
                .Permit(Trigger.Ready, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.SetSelectedProduct;
                }).Permit(Trigger.Cancel, State.DisplayConfirmNoMore)
                .Permit(Trigger.OrderStatus, State.DisplayPickOrderStatus)
                .Permit(Trigger.NavigateBack, State.DisplayConfirmNoMore);

            smConfig.ForState(State.SetSelectedProduct)
                .OnEntry(async () => await _StateInternals.SetSelectedProductAsync())
                .Permit(Trigger.NoSubstitution, State.DisplayAcknowledgeLocation)
                .Permit(Trigger.Substitution, State.DisplayEnterSubProduct);

            smConfig.ForState(State.DisplayAcknowledgeLocation)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandleAcknowledgeLocation;
                })
                .Permit(Trigger.NavigateBack, State.DisplayConfirmNoMore);

            smConfig.ForState(State.HandleAcknowledgeLocation)
                .OnEntry(async () => await _StateInternals.HandleAcknowledgeLocationAsync())
                .Permit(Trigger.Ready, State.DisplayEnterProduct)
                .Permit(Trigger.SkipProduct, State.DisplayConfirmSkipProduct)
                .Permit(Trigger.Cancel, State.DisplayConfirmNoMore)
                .Permit(Trigger.OrderStatus, State.DisplayPickOrderStatus)
                .Permit(Trigger.NavigateBack, State.DisplayConfirmNoMore);

            smConfig.ForState(State.DisplayEnterProduct)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.VerifyProduct;
                });

            smConfig.ForState(State.VerifyProduct)
                .OnEntry(async () => await _StateInternals.VerifyProductAsync())
                .Permit(Trigger.InvalidEntry, State.DisplayEnterProduct)
                .Permit(Trigger.ValidEntry, State.DisplayEnterQuantity)
                .Permit(Trigger.NavigateBack, State.HandleRevertPick)
                .Permit(Trigger.SkipProduct, State.DisplayConfirmSkipProduct)
                .Permit(Trigger.OrderStatus, State.DisplayPickOrderStatus);

            smConfig.ForState(State.HandleRevertPick)
               .OnEntry(async () => await _StateInternals.HandleRevertPickingActions())
               .Permit(Trigger.EnterSubProduct, State.DisplayEnterSubProduct)
               .Permit(Trigger.EnterProduct, State.DisplayEnterProduct)
               .Permit(Trigger.AcknowledgeLocation, State.DisplayAcknowledgeLocation); ;

            smConfig.ForState(State.DisplayEnterQuantity)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.VerifyQuantity;
                });

            smConfig.ForState(State.VerifyQuantity)
                .OnEntry(async () => await _StateInternals.VerifyQuantityAsync())
                .Permit(Trigger.Overflow, State.DisplayConfirmOverflow)
                .Permit(Trigger.QuantityLess, State.DisplayConfirmQuantity)
                .Permit(Trigger.QuantityGreater, State.DisplayEnterQuantity)
                .Permit(Trigger.QuantityMatches, State.DisplayConfirmQuantity)
                .Permit(Trigger.NavigateBack, State.DisplayEnterProduct)
                .Permit(Trigger.OrderStatus, State.DisplayPickOrderStatus);

            smConfig.ForState(State.DisplayConfirmOverflow)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandleConfirmOverflow;
                });

            smConfig.ForState(State.HandleConfirmOverflow)
                .OnEntry(async () => await _StateInternals.HandleConfirmOverflow())
                .Permit(Trigger.DataRequestSucceeded, State.DisplayEnterQuantity)
                .Permit(Trigger.NavigateBack, State.DisplayEnterQuantity);

            smConfig.ForState(State.UpdateQuantity)
                .OnEntry(async () => await _StateInternals.UpdateQuantityAsync())
                .Permit(Trigger.DataRequestFailed, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.SignOut;
                })
                .Permit(Trigger.DataRequestSucceeded, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.CheckForSubstitution;
                });

            smConfig.ForState(State.CheckForMoreWork)
                .OnEntry(async () => await _StateInternals.CheckForMoreWorkAsync())
                .Permit(Trigger.MoreWork, State.SetSelectedProduct)
                .Permit(Trigger.NoMoreWork, State.HandleNoMoreWork);

            smConfig.ForState(State.HandleNoMoreWork)
                .OnEntry(() => _StateInternals.HandleNoMoreWork())
                .Permit(Trigger.EndWorkflow, State.DisplayAllDone)
                .Permit(Trigger.Staging, State.DisplayGoToStagingLocation);

            smConfig.ForState(State.DisplayConfirmQuantity)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandleConfirmQuantity;
                });

            smConfig.ForState(State.HandleConfirmQuantity)
                .OnEntry(async () => await _StateInternals.HandleConfirmQuantityAsync())
                .Permit(Trigger.NegativeConfirmation, State.DisplayEnterQuantity)
                .Permit(Trigger.AffirmativeConfirmation, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.UpdateQuantity;
                });

            smConfig.ForState(State.CheckForSubstitution)
                .OnEntry(async () => await _StateInternals.CheckForSubstitution())
                .Permit(Trigger.Substitution, State.DisplayEnterSubProduct)
                .Permit(Trigger.NoSubstitution, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.CheckForMoreWork;
                });

            smConfig.ForState(State.DisplayEnterSubProduct)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.VerifyProduct;
                });

            smConfig.ForState(State.DisplayConfirmNoMore)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.CheckNoMoreConfirmation;
                });

            smConfig.ForState(State.CheckNoMoreConfirmation)
                .OnEntry(async () => await _StateInternals.CheckNoMoreConfirmationAsync())
                .Permit(Trigger.NavigateBack, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = _StateInternals.LastState;
                })
                .Permit(Trigger.NegativeConfirmation, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = _StateInternals.LastState;
                })
                .Permit(Trigger.EndWorkflow, State.DisplayAllDone)
                .Permit(Trigger.Staging, State.DisplayGoToStagingLocation);

            smConfig.ForState(State.DisplayConfirmSkipProduct)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.CheckSkipProductConfirmation;
                });

            smConfig.ForState(State.CheckSkipProductConfirmation)
                .OnEntry(async () => await _StateInternals.CheckSkipProductConfirmationAsync())
                .Permit(Trigger.NavigateBack, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = _StateInternals.LastState;
                })
                .Permit(Trigger.NegativeConfirmation, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = _StateInternals.LastState;
                })
                .Permit(Trigger.AffirmativeConfirmation, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.SkipProduct;
                });

            smConfig.ForState(State.SkipProduct)
                .OnEntry(async () => await _StateInternals.SkipProductAsync())
                .Permit(Trigger.DataRequestSucceeded, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.CheckForMoreWork;
                });

            smConfig.ForState(State.DisplayPickOrderStatus)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandlePickOrderStatus;
                });

            smConfig.ForState(State.HandlePickOrderStatus)
                .OnEntry(async () => await _StateInternals.HandlePickOrderStatusAsync())
                .Permit(Trigger.Ready, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = _StateInternals.LastState;
                });

            smConfig.ForState(State.DisplayGoToStagingLocation)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandleGoToStaging;
                });

            smConfig.ForState(State.HandleGoToStaging)
               .OnEntry(async () => await _StateInternals.HandleGoToStagingAsync())
               .Permit(Trigger.Ready, State.DisplayEnterStagingLocation);

            smConfig.ForState(State.DisplayEnterStagingLocation)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandleEnterStagingLocation;
                });

            smConfig.ForState(State.HandleEnterStagingLocation)
               .OnEntry(async () => await _StateInternals.HandleEnterStagingLocationAsync())
               .Permit(Trigger.ValidEntry, State.DisplayConfirmStagingLocation)
               .Permit(Trigger.InvalidEntry, State.DisplayEnterStagingLocation)
               .Permit(Trigger.NavigateBack, State.DisplayGoToStagingLocation);

            smConfig.ForState(State.DisplayConfirmStagingLocation)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandleConfirmStagingLocation;
                });

            smConfig.ForState(State.HandleConfirmStagingLocation)
               .OnEntry(async () => await _StateInternals.HandleConfirmStagingLocationAsync())
               .Permit(Trigger.Staging, State.DisplayEnterStagingLocation)
               .Permit(Trigger.EndWorkflow, State.DisplayAllDone)
               .Permit(Trigger.NavigateBack, State.DisplayEnterStagingLocation);

            smConfig.ForState(State.DisplayAllDone)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput);

            _StateMachine = StateMachineFactory.Create(State.Start, smConfig);

#pragma warning disable 4014
            ExecuteStateAsync();
#pragma warning restore 4014
        }

        public async Task ExecuteStateAsync()
        {
            _Log.Debug("---->Current State:" + _StateMachine.CurrentState.ToString() + " Trigger:" + Trigger.ReturnUserInput.ToString());
            // Execute trigger to process user input
            await _StateMachine.FireAsync(Trigger.ReturnUserInput);

            if (_StateMachine.CurrentState.Equals(State.BackgroundActvity))
            {
                StartBackgroundActivities();
            }
            else
            {
                await RunActivitiesUntilUserInput();
            }
        }

        private async Task StartBackgroundActivities()
        {
            await _StateMachine.FireAsync(Trigger.ExecuteBackgroundActivity);
            await RunActivitiesUntilUserInput();
        }

        private async Task RunActivitiesUntilUserInput()
        {
            // Execute triggers until a user input state is reached
            while (_StateInternals.NextTrigger != Trigger.WaitForUserInput)
            {
                _Log.Debug("---->Until Input State Current State:" + _StateMachine.CurrentState.ToString() + " Trigger:" + _StateInternals.NextTrigger.ToString());
                await _StateMachine.FireAsync(_StateInternals.NextTrigger);

                if (_StateMachine.CurrentState.Equals(State.BackgroundActvity))
                {
                    await _StateMachine.FireAsync(Trigger.ExecuteBackgroundActivity);
                }
            }
        }
    }
}
