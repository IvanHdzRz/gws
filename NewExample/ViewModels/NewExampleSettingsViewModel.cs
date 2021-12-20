using Honeywell.Firebird.CoreLibrary;
using Honeywell.Firebird.WorkflowEngine;
using System.Windows.Input;

namespace NewExample
{
    public class NewExampleSettingsViewModel : CoreViewModel
    {
        private readonly INewExampleConfigRepository _NewExampleConfigRepository;

        public NewExampleSettingsViewModel(WorkflowViewModelDependencies dependencies,
                                                  INewExampleConfigRepository NewExampleConfigRepository
        )
            : base(dependencies)
        {
            _NewExampleConfigRepository = NewExampleConfigRepository;
        }

        /// <summary>
        /// Toggle that backs the switch for enabling/disabling secure connections (https)
        /// </summary>
        private bool _ServerSecureConnections;
        public bool ServerSecureConnections
        {
            get
            {
                return _ServerSecureConnections;
            }

            set
            {
                _ServerSecureConnections = value;
                NotifyPropertyChanged();
            }
        }

        public string Host { get; set; }

        public string Port { get; set; }

        public ICommand OnHostEntryLostFocus { get; set; }

        public ICommand OnPortEntryLostFocus { get; set; }
    }
}
