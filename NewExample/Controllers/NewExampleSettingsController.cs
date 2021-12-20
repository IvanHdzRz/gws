using Honeywell.Firebird;
using Honeywell.Firebird.CoreLibrary;
using Honeywell.Firebird.WorkflowEngine;
using System;
using System.ComponentModel;

namespace NewExample
{
    /// <summary>
    /// This class handles initialization of the NewExample settings menu.
    /// </summary>
    public class NewExampleSettingsController : CoreViewController
    {
        private readonly INewExampleConfigRepository _NewExampleConfigRepository;

        protected NewExampleSettingsViewModel _ViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewExampleSettingsController"/> class.
        /// </summary>
        public NewExampleSettingsController(CoreViewControllerDependencies dependencies,
                                            INewExampleConfigRepository NewExampleConfigRepository)
            : base(dependencies)
        {
            _NewExampleConfigRepository = NewExampleConfigRepository;
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            _ViewModel = (NewExampleSettingsViewModel)base.CreateViewModel(viewModelName);

            _ViewModel.OnHostEntryLostFocus = new Command(OnHostEntryLosesFocus);
            _ViewModel.OnPortEntryLostFocus = new Command(OnPortEntryLosesFocus);

            bool serverSecureConnections;
            Boolean.TryParse(_NewExampleConfigRepository.GetConfig("SecureConnections").Value, out serverSecureConnections);
            _ViewModel.ServerSecureConnections = serverSecureConnections;

            _ViewModel.Host = _NewExampleConfigRepository.GetConfig("Host").Value;
            _ViewModel.Port = _NewExampleConfigRepository.GetConfig("Port").Value;

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
                _NewExampleConfigRepository.SaveConfig(new Config("SecureConnections", _ViewModel.ServerSecureConnections.ToString()));
            }
        }

        protected virtual void OnHostEntryLosesFocus()
        {
            _NewExampleConfigRepository.SaveConfig(new Config("Host", _ViewModel.Host));
        }

        protected virtual void OnPortEntryLosesFocus()
        {
            _NewExampleConfigRepository.SaveConfig(new Config("Port", _ViewModel.Port));
        }
    }
}
