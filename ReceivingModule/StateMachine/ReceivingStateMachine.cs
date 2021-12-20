//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Threading.Tasks;
    using Common.Logging;
    using LiquidState;
    using LiquidState.Awaitable.Core;

    public class ReceivingStateMachine : IReceivingStateMachine
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(ReceivingStateMachine));

        private ReceivingStateInternals _StateInternals;
        private IAwaitableStateMachine<State, Trigger> _StateMachine;
        private State _BackgroundActivityNextState;

        public ReceivingStateMachine(IReceivingModel model)
        {
            _StateInternals = new ReceivingStateInternals(model);
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
                    _BackgroundActivityNextState = State.RetrieveOrders;
                });

            smConfig.ForState(State.RetrieveOrders)
                .OnEntry(async () => await _StateInternals.RetrieveOrdersAsync())
                .Permit(Trigger.DataRequestFailed, State.DisplaySignIn)
                .Permit(Trigger.DataRequestSucceeded, State.DisplayOrders);

            smConfig.ForState(State.DisplayOrders)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.VerifyOrder;
                });

            smConfig.ForState(State.VerifyOrder)
                .OnEntry(async () => await _StateInternals.VerifyOrderAsync())
                .Permit(Trigger.NavigateBack, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.SignOut;
                })
                .Permit(Trigger.InvalidEntry, State.DisplayOrders)
                .Permit(Trigger.ValidEntry, State.DisplayHiQuantity)
                .Permit(Trigger.Cancel, State.DisplayInvoiceSummary);

            smConfig.ForState(State.DisplayHiQuantity)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.VerifyHiQuantity;
                });

            smConfig.ForState(State.VerifyHiQuantity)
                .OnEntry(async () => await _StateInternals.VerifyHiQuantityAsync())
                .Permit(Trigger.NavigateBack, State.DisplayOrders)
                .Permit(Trigger.InvalidEntry, State.DisplayOrders)
                .Permit(Trigger.ValidEntry, State.DisplayTiQuantity);

            smConfig.ForState(State.DisplayTiQuantity)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.VerifyTiQuantity;
                });

            smConfig.ForState(State.VerifyTiQuantity)
                .OnEntry(async () => await _StateInternals.VerifyTiQuantityAsync())
                .Permit(Trigger.NavigateBack, State.DisplayHiQuantity)
                .Permit(Trigger.InvalidEntry, State.DisplayOrders)
                .Permit(Trigger.ValidEntry, State.DisplayConfirmQuantity);

            smConfig.ForState(State.DisplayConfirmQuantity)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.CheckQuantityConfirmation;
                });

            smConfig.ForState(State.CheckQuantityConfirmation)
                .OnEntry(async () => await _StateInternals.CheckQuantityConfirmationAsync())
                .Permit(Trigger.NavigateBack, State.DisplayHiQuantity)
                .Permit(Trigger.NegativeConfirmation, State.DisplayHiQuantity)
                .Permit(Trigger.AffirmativeConfirmation, State.DisplayPrintingLabel);

            smConfig.ForState(State.DisplayPrintingLabel)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.DisplayApplyLabel );

            smConfig.ForState(State.DisplayApplyLabel)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandleApplyLabel;
                });

            smConfig.ForState(State.HandleApplyLabel)
                .OnEntry(async () => await _StateInternals.HandleApplyLabelAsync())
                .Permit(Trigger.Ready, State.DisplayPalletCondition);

            smConfig.ForState(State.DisplayPalletCondition)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.CheckPalletCondition;
                });

            smConfig.ForState(State.CheckPalletCondition)
                .OnEntry(async () => await _StateInternals.CheckPalletConditionAsync())
                .Permit(Trigger.DamagedCondition, State.DisplayDamagedReason)
                .Permit(Trigger.GoodCondition, State.DisplayOrders);

            smConfig.ForState(State.DisplayDamagedReason)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandleDamagedReason;
                });

            smConfig.ForState(State.HandleDamagedReason)
                .OnEntry(async () => await _StateInternals.HandleDamagedReasonAsync())
                .Permit(Trigger.NavigateBack, State.DisplayPalletCondition)
                .Permit(Trigger.Ready, State.DisplayOrders);

            smConfig.ForState(State.DisplayInvoiceSummary)
                .OnEntry(() => _StateInternals.NextTrigger = Trigger.WaitForUserInput)
                .Permit(Trigger.ReturnUserInput, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.HandleInvoiceSummary;
                });

            smConfig.ForState(State.HandleInvoiceSummary)
                .OnEntry(async () => await _StateInternals.HandleInvoiceSummaryAsync())
                .Permit(Trigger.Ready, State.BackgroundActvity, () =>
                {
                    _BackgroundActivityNextState = State.SignOut;
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
