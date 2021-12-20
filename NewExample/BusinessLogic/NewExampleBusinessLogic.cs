using Common.Logging;
using GuidedWork;
using GuidedWorkRunner;
using Honeywell.Firebird.CoreLibrary;
using System.Linq;
using TinyIoC;

namespace NewExample
{
    public class NewExampleModuleVocab : DefaultModuleVocab
    {
        public override VocabWordInfo[] PlatformIndependentVocab { get; } =
            BaseRequiredVocab
                .Concat(Numerics).ToArray();
    }

    public interface INewExampleModel : IGenericIntentModel<INewExampleModel>
    {
        // TODO: Add model property interfaces here
    }

    public class NewExampleModel : SimplifiedIntentModel<NewExampleBusinessLogic, INewExampleModel>, INewExampleModel
    {
        // TODO: Add model property implemenations here
    }

    public class NewExampleBusinessLogic : SimplifiedBaseBusinessLogic<INewExampleModel, NewExampleBusinessLogic, NewExampleConfigRepository>
    {
        // TODO: Add state machine states here

        private readonly INewExampleRESTServiceProvider _ServiceProvider;

        private readonly ILog _Log = LogManager.GetLogger(nameof(NewExampleBusinessLogic));

        public override bool IsPrimaryStateMachine => true;

        public NewExampleBusinessLogic(SimplifiedStateMachineManager<NewExampleBusinessLogic, INewExampleModel> manager, INewExampleModel model) : base(manager, model)
        {
            _ServiceProvider = TinyIoCContainer.Current.Resolve<INewExampleRESTServiceProvider>();
        }

        public override void ConfigureStates()
        {
            // TODO: Add state configuration here
        }

        // TODO: Add state encoder, decoder and additional logic methods here
    }
}
