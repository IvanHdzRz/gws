//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using GuidedWork;
    using GuidedWorkRunner;
    using System.Threading.Tasks;

    public class DiscretePickStateMachine : SimplifiedBaseBusinessLogic<IBasePickingModel, BasePickingStateMachine, IBasePickingConfigRepository>
    {
        public static readonly CoreAppSMState StartQuantitySM = new CoreAppSMState(nameof(StartQuantitySM));
        public static readonly CoreAppSMState CheckQuantitySMComplete = new CoreAppSMState(nameof(CheckQuantitySMComplete));

        private QuantityStateMachine _QuantitySM;
        private QuantityStateMachine QuantitySM { get { return Manager.CreateStateMachine(ref _QuantitySM); } }

        public DiscretePickStateMachine(SimplifiedStateMachineManager<BasePickingStateMachine, IBasePickingModel> manager, IBasePickingModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            // start the quantity state machine
            ConfigureLogicState(StartQuantitySM,
                                async () =>
                                {
                                    NextState = CheckQuantitySMComplete;
                                    await StartQuantityStateMachineAsync();
                                },
                                CheckQuantitySMComplete);

            ConfigureReturnLogicState(CheckQuantitySMComplete,
                                      () =>
                                      {
                                          // Perform post quantity processing

                                          // Leave NextState null to return to the previous state machine
                                      });
        }

        private async Task StartQuantityStateMachineAsync()
        {
            QuantitySM.Reset();
            await QuantitySM.InitializeStateMachineAsync();
        }
    }
}
