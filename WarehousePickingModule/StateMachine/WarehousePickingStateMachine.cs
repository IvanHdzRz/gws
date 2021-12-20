//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Threading.Tasks;
    using LiquidState;
    using LiquidState.Awaitable.Core;
    using Common.Logging;

    public class WarehousePickingStateMachine : IWarehousePickingStateMachine
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(WarehousePickingStateMachine));

        private WarehousePickingStateInternals _StateInternals;
        private IAwaitableStateMachine<State, Trigger> _StateMachine;
        private State _BackgroundActivityNextState;

        public WarehousePickingStateMachine(IWarehousePickingModel model, IWarehousePickingActivityTracker warehousePickingActivityTracker)
        {
            _StateInternals = new WarehousePickingStateInternals(model, warehousePickingActivityTracker);
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
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.StartSignIn;
                });

            smConfig.ForState(State.SignOut)
                .OnEntry(async () => await _StateInternals.SignOutAsync())
                .Permit(Trigger.DataRequestSucceeded, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.StartSignIn;
                });

            smConfig.ForState(State.StartSignIn)
                .OnEntry(async () => await _StateInternals.StartSignInAsync())
                .Permit(Trigger.Ready, State.DisplaySignIn);

            smConfig.ForState(State.DisplaySignIn)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.SignIn;
                });

            smConfig.ForState(State.SignIn)
                .OnEntry(async () => await _StateInternals.SignInAsync())
                .Permit(Trigger.DataRequestFailed, State.DisplaySignIn)
                .Permit(Trigger.InvalidEntry, State.DisplaySignIn)
                .Permit(Trigger.ValidEntry, State.StartOperPrep);

            smConfig.ForState(State.StartOperPrep)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.RetrieveSubcenters;
                });

            smConfig.ForState(State.RetrieveSubcenters)
                .OnEntry(async () => await _StateInternals.RetrieveSubcentersAsync())
                .Permit(Trigger.DataRequestFailed, State.Start)
                .Permit(Trigger.DataRequestSucceeded, State.DisplaySubcenters);

            smConfig.ForState(State.DisplaySubcenters)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandleSubcenter;
                });

            smConfig.ForState(State.HandleSubcenter)
                .OnEntry(async () => await _StateInternals.HandleSubcenterAsync())
                .Permit(Trigger.NavigateBack, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.SignOut;
                })
                .Permit(Trigger.ValidEntry, State.DisplayLabelPrinter);

            smConfig.ForState(State.DisplayLabelPrinter)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.VerifyLabelPrinter;
                });

            smConfig.ForState(State.VerifyLabelPrinter)
                .OnEntry(async () => await _StateInternals.VerifyLabelPrinterAsync())
                .Permit(Trigger.NavigateBack, State.DisplaySubcenters)
                .Permit(Trigger.ValidEntry, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.RetrievePicks;
                });

            smConfig.ForState(State.RetrievePicks)
                .OnEntry(async () => await _StateInternals.RetrievePicksAsync())
                .Permit(Trigger.DataRequestFailed, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.SignOut;
                })
                .Permit(Trigger.DataRequestSucceeded, State.DisplayPickTripInfo);

            smConfig.ForState(State.DisplayPickTripInfo)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandlePickTripInfo;
                }); ;

            smConfig.ForState(State.HandlePickTripInfo)
                .OnEntry(async () => await _StateInternals.HandlePickTripInfoAsync())
                .Permit(Trigger.NavigateBack, State.DisplayLabelPrinter)
                .Permit(Trigger.Ready, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.SetSelectedProduct;
                });

            smConfig.ForState(State.SetSelectedProduct)
                .OnEntry(async () => await _StateInternals.SetSelectedProductAsync())
                .Permit(Trigger.DataRequestSucceeded, State.DisplayAcknowledgeLocation);

            smConfig.ForState(State.DisplayAcknowledgeLocation)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.VerifyLocation;
                });

            smConfig.ForState(State.VerifyLocation)
                .OnEntry(async () => await _StateInternals.VerifyLocationAsync())
                .Permit(Trigger.SkipProduct, State.DisplayConfirmSkipProduct)
                .Permit(Trigger.LastPick, State.DisplayLastPick)
                .Permit(Trigger.OrderStatus, State.DisplayPickOrderStatus)
                .Permit(Trigger.Ready, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.StartSlotTracker;
                })
                .Permit(Trigger.Cancel, State.DisplayConfirmNoMore)
                .Permit(Trigger.NavigateBack, State.DisplayPickTripInfo);

            smConfig.ForState(State.StartSlotTracker)
                .OnEntry(async () => await _StateInternals.StartSlotTrackerAsync())
                .Permit(Trigger.Ready, State.DisplayEnterProduct);

            smConfig.ForState(State.DisplayEnterProduct)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.VerifyProduct;
                });

            smConfig.ForState(State.VerifyProduct)
                .OnEntry(async () => await _StateInternals.VerifyProductAsync())
                .Permit(Trigger.SkipProduct, State.DisplayConfirmSkipProduct)
                .Permit(Trigger.LastPick, State.DisplayLastPick)
                .Permit(Trigger.OrderStatus, State.DisplayPickOrderStatus)
                .Permit(Trigger.InvalidEntry, State.DisplayEnterProduct)
                .Permit(Trigger.ValidEntry, State.DisplayEnterQuantity)
                .Permit(Trigger.Cancel, State.DisplayConfirmNoMore)
                .Permit(Trigger.NavigateBack, State.DisplayAcknowledgeLocation);

            smConfig.ForState(State.DisplayEnterQuantity)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.VerifyQuantity;
                });

            smConfig.ForState(State.VerifyQuantity)
                .OnEntry(async () => await _StateInternals.VerifyQuantityAsync())
                .Permit(Trigger.QuantityLess, State.DisplayConfirmQuantity)
                .Permit(Trigger.QuantityGreater, State.DisplayEnterQuantity)
                .Permit(Trigger.QuantityMatches, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.UpdateQuantity;
                })
                .Permit(Trigger.NavigateBack, State.DisplayEnterProduct);


            smConfig.ForState(State.UpdateQuantity)
                .OnEntry(async () => await _StateInternals.UpdateQuantityAsync())
                .Permit(Trigger.DataRequestFailed, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.SignOut;
                })
                .Permit(Trigger.DataRequestSucceeded, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.CheckForMoreWork;
                });

            smConfig.ForState(State.CheckForMoreWork)
                .OnEntry(async () => await _StateInternals.CheckForMoreWorkAsync())
                .Permit(Trigger.MoreWork, State.SetSelectedProduct)
                .Permit(Trigger.MoreWorkSameLocation, State.StartSlotTracker)
                .Permit(Trigger.NoMoreWork, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.SetOrderComplete;
                });

            smConfig.ForState(State.SetOrderComplete)
                .OnEntry(async () => await _StateInternals.SetOrderCompleteAsync())
                .Permit(Trigger.DataRequestFailed, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.SignOut;
                })
                .Permit(Trigger.DataRequestSucceeded, State.DisplayPickOrderSummary);

            smConfig.ForState(State.DisplayConfirmQuantity)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.CheckShortProductConfirmation;
                });

            smConfig.ForState(State.CheckShortProductConfirmation)
                .OnEntry(async () => await _StateInternals.CheckShortProductConfirmationAsync())
                .Permit(Trigger.NavigateBack, State.DisplayEnterQuantity)
                .Permit(Trigger.NegativeConfirmation, State.DisplayEnterQuantity)
                .Permit(Trigger.AffirmativeConfirmation, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.UpdateQuantity;
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
                .Permit(Trigger.AffirmativeConfirmation, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.DisplayPickOrderSummary;
                });

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

            smConfig.ForState(State.DisplayPickOrderSummary)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandlePickOrderSummary;
                });

            smConfig.ForState(State.HandlePickOrderSummary)
                .OnEntry(async () => await _StateInternals.HandlePickOrderSummaryAsync())
#if false
                .Permit(Trigger.Ready, State.DisplayPickPerformance);
#else
                .Permit(Trigger.Ready, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.ResetData;
                });
#endif

            smConfig.ForState(State.DisplayPickPerformance)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandlePickPerformance;
                });

            smConfig.ForState(State.HandlePickPerformance)
                .OnEntry(async () => await _StateInternals.HandlePickPerformanceAsync())
                .Permit(Trigger.Ready, State.BackgroundActvity, () =>
                {
#if false
                    _BackgroundActivityNextState = State.SignOut;
#else
                    _BackgroundActivityNextState = State.ResetData;
#endif
                })
                .Permit(Trigger.NavigateBack, State.DisplayPickOrderSummary);

            smConfig.ForState(State.ResetData)
                .OnEntry(async () => await _StateInternals.HandleResetDataAsync())
                .Permit(Trigger.Ready, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.RetrieveSubcenters;
                });

            smConfig.ForState(State.DisplayLastPick)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandleLastPick;
                });

            smConfig.ForState(State.HandleLastPick)
                .OnEntry(async () => await _StateInternals.HandleLastPickAsync())
                .Permit(Trigger.Ready, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = _StateInternals.LastState;
                })
                .Permit(Trigger.NavigateBack, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = _StateInternals.LastState;
                });


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
            while (_StateInternals.NextTrigger != Trigger.WaitForUserInput && !_StateMachine.CurrentState.Equals(State.StartOperPrep))
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
