using Honeywell.Firebird.CoreLibrary;
using Honeywell.Firebird.WorkflowEngine;
using System.Windows.Input;

namespace BasePickingExample
{
    public class BasePickingExampleSettingsViewModel : CoreViewModel
    {
        private readonly IBasePickingExampleConfigRepository _BasePickingExampleConfigRepository;

        public BasePickingExampleSettingsViewModel(WorkflowViewModelDependencies dependencies,
                                                  IBasePickingExampleConfigRepository BasePickingExampleConfigRepository
        )
            : base(dependencies)
        {
            _BasePickingExampleConfigRepository = BasePickingExampleConfigRepository;
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
