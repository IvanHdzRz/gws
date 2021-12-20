//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using System.Threading.Tasks;

    public class ClusterPickStateMachine : SimplifiedBaseBusinessLogic<IBasePickingModel, BasePickingStateMachine, IBasePickingConfigRepository>
    {
        public static readonly CoreAppSMState StartQuantityClusterSM = new CoreAppSMState(nameof(StartQuantityClusterSM));

        public static readonly CoreAppSMState CheckQuantityClusterSMComplete = new CoreAppSMState(nameof(CheckQuantityClusterSMComplete));

        public static readonly CoreAppSMState DisplayPutInContainer = new CoreAppSMState(nameof(DisplayPutInContainer));
        public static readonly CoreAppSMState VerifyPickPutInContainer = new CoreAppSMState(nameof(VerifyPickPutInContainer));

        private QuantityStateMachine _QuantitySM;
        private QuantityStateMachine QuantitySM { get { return Manager.CreateStateMachine(ref _QuantitySM); } }

        public ClusterPickStateMachine(SimplifiedStateMachineManager<BasePickingStateMachine, IBasePickingModel> manager, IBasePickingModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            // start the quantity state machine
            ConfigureLogicState(StartQuantityClusterSM,
                                async () =>
                                {
                                    NextState = CheckQuantityClusterSMComplete;
                                    await StartQuantityStateMachineAsync();
                                },
                                CheckQuantityClusterSMComplete);

            // if a product was shorted with a quantity of zero
            // we should return to previous state machine
            // so that it can check for more work and proceed to next pick
            ConfigureReturnLogicState(CheckQuantityClusterSMComplete,
                                      () =>
                                      {
                                          if (Model.CurrentPick.QuantityPicked != 0)
                                          {
                                              NextState = DisplayPutInContainer;
                                          }

                                          // Leave NextState null to return to the previous state machine
                                      },
                                      DisplayPutInContainer);

            // Put item in a container

            ConfigureDisplayState(DisplayPutInContainer,
                                  VerifyPickPutInContainer,
                                  encodeAction: EncodePutInContainer,
                                  decodeAction: DecodePutInContainer);

            ConfigureReturnLogicState(VerifyPickPutInContainer,
                                      () =>
                                      {
                                          // if (validation fails)
                                          // {
                                          //     NextState = DisplayPutInContainer;
                                          //     Model.CurrentUserMessage = Translate.GetLocalizedTextForKey("Error_message_resource_key");
                                          // }

                                          // Leave NextState null to return to the previous state machine
                                      },
                                      DisplayPutInContainer);
        }

        protected virtual WorkflowObjectContainer EncodePutInContainer(IBasePickingModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("BasePicking_PutInContainer_Cluster_Header",
                                                                                                  model.CurrentPick.QuantityPicked.ToString(),
                                                                                                  model.CurrentPick.ContainerPosition.ToString()),
                                                                "putInContainerCluster",
                                                                Translate.GetLocalizedTextForKey("BasePicking_PutInContainer_Cluster_Label"),
                                                                Translate.GetLocalizedTextForKey("BasePicking_PutInContainer_Cluster_Prompt",
                                                                                                 model.CurrentPick.QuantityPicked.ToString(),
                                                                                                 model.CurrentPick.ContainerPosition.ToString()),
                                                                null,
                                                                model.CurrentUserMessage,
                                                                initialPrompt: model.CurrentUserMessage);

            wfo.ValueProperties.MinRequiredLength = model.CurrentPick.ContainerSpokenVerification.Length;
            wfo.ValueProperties.MaxAllowedLength = model.CurrentPick.ContainerSpokenVerification.Length;
            wfo.ValueProperties.ExpectedScannedValues.Add(model.CurrentPick.ContainerScannedVerification);
            wfo.ValueProperties.ExpectedSpokenOrTypedValues.Add(model.CurrentPick.ContainerSpokenVerification);

            bool.TryParse(_ConfigRepo.GetConfig("ShowHints").Value, out bool showHints);
            if (showHints)
            {
                wfo.ValueProperties.Placeholder = model.CurrentPick.ContainerSpokenVerification;
            }

            wfo.MessageType = model.MessageType;
            model.ConfigureProgressBar(wfo);
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodePutInContainer(SlotContainer slotContainer, IBasePickingModel basePickingModel)
        {
            string response = GenericBaseEncoder<IBasePickingModel>.DecodeValueEntry(slotContainer);
            basePickingModel.EnteredContainerVerification = response;
        }

        private async Task StartQuantityStateMachineAsync()
        {
            QuantitySM.Reset();
            await QuantitySM.InitializeStateMachineAsync();
        }
    }
}
