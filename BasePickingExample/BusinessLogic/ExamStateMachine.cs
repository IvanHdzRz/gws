
namespace BasePickingExample
{
    using Common.Logging;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using System;
    using System.Linq;
    using Honeywell.DialogueRunner;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    public class ExamStateMachine : SimplifiedBaseBusinessLogic<IBasePickingExampleModel, BasePickingExampleBusinessLogic, BasePickingExampleConfigRepository>
    {
        public static readonly CoreAppSMState DisplayEnterContainerId = new CoreAppSMState(nameof(DisplayEnterContainerId));
        public static readonly CoreAppSMState GetContainers = new CoreAppSMState(nameof(GetContainers));
        public static readonly CoreAppSMState OpenContainer = new CoreAppSMState(nameof(OpenContainer));
        public static readonly CoreAppSMState ExitExamSM = new CoreAppSMState(nameof(ExitExamSM));
        private readonly ILog _Log = LogManager.GetLogger(nameof(BasePickingExamplePickStateMachine));
        public ExamStateMachine(SimplifiedStateMachineManager<BasePickingExampleBusinessLogic, IBasePickingExampleModel> manager, IBasePickingExampleModel model) : base(manager, model)
        {
        }
        public override void ConfigureStates() {

            ConfigureDisplayState(DisplayEnterContainerId,
                     GetContainers,
                      followedByBackgroundActivity: true,
                      backgroundActivityHeaderKey: Translate.GetLocalizedTextForKey("ExamSM_Getting_Containers"),
                     encodeAction: EncodeEnterContainer,
                     decodeAction: DecodeEnterContainer);

            //todo add async call
            ConfigureReturnLogicState(GetContainers,
                async () => {
                    try
                    {
                        Model.Containers = await Model.ServiceProvider.GetContainersById(Model.CurrentOperator.OperatorIdentifier,Model.idContainer);
                        Model.idAssignament = Model.Containers[0].AssignmentId;
                        NextState = OpenContainer;

                    }
                    catch (Exception e)
                    {
                        _Log.Debug("Operation canceled via communication Timeout Handler");
                        Model.CurrentUserMessage = Translate.GetLocalizedTextForKey("BasePicking_NetworkTimeout_Error");
                    }

                },
                OpenContainer
                );
            ConfigureReturnLogicState(OpenContainer,
                async()=> {
                    try
                    {
                        Model.StatusOpenContainer = await Model.ServiceProvider.OpenContainer(Model.idAssignament,Model.idContainer);
                        NextState = ExitExamSM;

                    }
                    catch (Exception e)
                    {
                        _Log.Debug("Operation canceled via communication Timeout Handler");
                        Model.CurrentUserMessage = Translate.GetLocalizedTextForKey("BasePicking_NetworkTimeout_Error");
                    }
                    
                },
                ExitExamSM);
            //return to last StateMachine
            ConfigureReturnLogicState(ExitExamSM,
                                      () =>
                                      {
                                          // Leave NextState null to return to the previous state machine
                                      });
        }

        private WorkflowObjectContainer EncodeEnterContainer(IBasePickingExampleModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("Open_Container_Title"),
                                                                  "enterContainerId",
                                                                  Translate.GetLocalizedTextForKey("IntroduceContainerLabel"),
                                                                  Translate.GetLocalizedTextForKey("IntroduceContainerLabel"),
                                                                  null,
                                                                  model.CurrentUserMessage,
                                                                  initialPrompt: model.CurrentUserMessage);

            

            wfo.ValueProperties.Placeholder = Translate.GetLocalizedTextForKey("IntroduceContainerLabel");

            wfo.MessageType = model.MessageType;
            
            wfoContainer.Add(wfo);
            return wfoContainer;
        }
        private void DecodeEnterContainer(SlotContainer slotContainer, IBasePickingExampleModel basePickingModel)
        {
            string response = GenericBaseEncoder<IBasePickingExampleModel>.DecodeValueEntry(slotContainer);
            basePickingModel.idContainer = response;
        }
    }
}
