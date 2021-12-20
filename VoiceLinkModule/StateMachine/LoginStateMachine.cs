//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using GuidedWork;
    using GuidedWorkRunner;
    using System.Threading.Tasks;

    public class LoginStateMachine : SimplifiedBaseBusinessLogic<IVoiceLinkModel, VoiceLinkStateMachine, IVoiceLinkConfigRepository>
    {
        public static readonly CoreAppSMState InitLogin = new CoreAppSMState(nameof(InitLogin));
        public static readonly CoreAppSMState SignOn = new CoreAppSMState(nameof(SignOn));

        private GuidedWorkRunner.Operator _OperatorToLogin { get; set; }

        public LoginStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
        }

        public async Task InitializeStateMachineAsync(GuidedWorkRunner.Operator operatorToLogin)
        {
            _OperatorToLogin = operatorToLogin;

            await InitializeStateMachineAsync();
        }

        public override void ConfigureStates()
        {
            ConfigureLogicState(InitLogin,
                                async () =>
                                {
                                    Model.ResetOperator();
                                    _OperatorUpdateService.ClearOperator();

                                    // Resend device config if operator changed
                                    if (_OperatorToLogin.OperatorIdentifier != Model.CurrentConfig.OperatorId)
                                    {
                                        await Model.LUTtransmit(LutType.Config, "VoiceLink_BackgroundActivity_Header_Loading_Config",
                                            goToStateIfFail: VoiceLinkStateMachine.ExecuteSignOn
                                        );
                                    }

                                    NextState = SignOn;
                                },
                                SignOn);

            ConfigureReturnLogicState(SignOn,
                                      async () =>
                                      {
                                          await Model.LUTtransmit(LutType.SignOn,
                                              goToStateIfFail: VoiceLinkStateMachine.ExecuteSignOn
                                          );
                                      });
        }
    }
}
