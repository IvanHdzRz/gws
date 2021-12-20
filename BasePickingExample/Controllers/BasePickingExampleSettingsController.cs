using Honeywell.Firebird;
using Honeywell.Firebird.CoreLibrary;
using Honeywell.Firebird.WorkflowEngine;
using System;
using System.ComponentModel;

namespace BasePickingExample
{
    /// <summary>
    /// This class handles initialization of the BasePickingExample settings menu.
    /// </summary>
    public class BasePickingExampleSettingsController : CoreViewController
    {
        private readonly IBasePickingExampleConfigRepository _BasePickingExampleConfigRepository;

        protected BasePickingExampleSettingsViewModel _ViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePickingExampleSettingsController"/> class.
        /// </summary>
        public BasePickingExampleSettingsController(CoreViewControllerDependencies dependencies,
                                            IBasePickingExampleConfigRepository BasePickingExampleConfigRepository)
            : base(dependencies)
        {
            _BasePickingExampleConfigRepository = BasePickingExampleConfigRepository;
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            _ViewModel = (BasePickingExampleSettingsViewModel)base.CreateViewModel(viewModelName);

            _ViewModel.OnHostEntryLostFocus = new Command(OnHostEntryLosesFocus);
            _ViewModel.OnPortEntryLostFocus = new Command(OnPortEntryLosesFocus);

            bool serverSecureConnections;
            Boolean.TryParse(_BasePickingExampleConfigRepository.GetConfig("SecureConnections").Value, out serverSecureConnections);
            _ViewModel.ServerSecureConnections = serverSecureConnections;

            _ViewModel.Host = _BasePickingExampleConfigRepository.GetConfig("Host").Value;
            _ViewModel.Port = _BasePickingExampleConfigRepository.GetConfig("Port").Value;

            return _ViewModel;
        }

        protected override void OnStop()
        {
            _ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            base.OnStop();
        }

        protected override void OnStart(NavigationReason reason)
        {
            _ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            base.OnStart(reason);
        }

        protected virtual void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_ViewModel.ServerSecureConnections))
            {
                _BasePickingExampleConfigRepository.SaveConfig(new Config("SecureConnections", _ViewModel.ServerSecureConnections.ToString()));
            }
        }

        protected virtual void OnHostEntryLosesFocus()
        {
            _BasePickingExampleConfigRepository.SaveConfig(new Config("Host", _ViewModel.Host));
        }

        protected virtual void OnPortEntryLosesFocus()
        {
            _BasePickingExampleConfigRepository.SaveConfig(new Config("Port", _ViewModel.Port));
        }
    }
}
