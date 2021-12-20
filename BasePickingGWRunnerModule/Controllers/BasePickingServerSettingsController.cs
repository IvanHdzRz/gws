//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Common.Logging;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWork;

    /// <summary>
    /// This class handles initialization of the BasePicking settings menu.
    /// </summary>
    public class BasePickingServerSettingsController : NavigatingMenuController
    {
        private readonly IBasePickingConfigRepository _BasePickingConfigRepository;
        private readonly IBasePickingDataProxy _BasePickingDataProxy;
        private readonly ILog _Log = LogManager.GetLogger(nameof(BasePickingServerSettingsController));
        private const string FileDataTransport = "FileDataTransport";
        private const string RESTDataTransport = "RESTDataTransport";
        private const string TCPSocketDataTransport = "TCPSocketDataTransport";

        protected BasePickingServerSettingsViewModel _ViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePickingServerSettingsController"/> class.
        /// </summary>
        public BasePickingServerSettingsController(CoreViewControllerDependencies dependencies,
                                            IBasePickingConfigRepository basePickingConfigRepository,
                                             IBasePickingDataProxy basePickingDataProxy)
            : base(dependencies)
        {
            _BasePickingConfigRepository = basePickingConfigRepository;
            _BasePickingDataProxy = basePickingDataProxy;
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            _ViewModel = (BasePickingServerSettingsViewModel)base.CreateViewModel(viewModelName);

            _ViewModel.ShowConfigurationSettings = bool.Parse(_BasePickingConfigRepository.GetConfig("ShowConfigurationSettings").Value);

            _ViewModel.OnHostEntryLostFocus = new Command(OnHostEntryLosesFocus);
            _ViewModel.OnPortEntryLostFocus = new Command(OnPortEntryLosesFocus);
            _ViewModel.OnODRPortEntryLostFocus = new Command(OnODRPortEntryLosesFocus);

            _ViewModel.WorkflowFilterSettingsVisible = true;

            //set the available workflow filter choices on the view model
            _ViewModel.WorkflowFilterChoices = new List<string>
            {
                LocalizationHelper.LocalizedEmbeddedDemoWorkflowFilterName,
                LocalizationHelper.LocalizedServerWorkflowFilterName,
                LocalizationHelper.LocalizedLegacySocketServerWorkflowFilterName
            };

            bool serverSecureConnections;
            Boolean.TryParse(_BasePickingConfigRepository.GetConfig("SecureConnections").Value, out serverSecureConnections);
            _ViewModel.ServerSecureConnections = serverSecureConnections;

            //set the selected workflow filter from config on the view model
            _ViewModel.SelectedWorkflowFilter = GetLocalizedText(_BasePickingConfigRepository.GetConfig("WorkflowFilterChoice").Value);

            //set whether the server settings (host, port) should be visible
            //based on the workflow filter selected
            _ViewModel.ServerSettingsVisible = ShouldShowServerSettings() || ShouldShowLegacyServerSettings();
            _ViewModel.LegacyServerSettingsVisible = ShouldShowLegacyServerSettings();

            _ViewModel.Host = _BasePickingConfigRepository.GetConfig("Host").Value;
            _ViewModel.Port = _BasePickingConfigRepository.GetConfig("Port").Value;
            _ViewModel.ODRPort = _BasePickingConfigRepository.GetConfig("ODRPort").Value;
            _BasePickingDataProxy.SelectTransport(DataTransport());

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

        protected override void OnResume(NavigationReason reason)
        {
            _ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            base.OnResume(reason);
        }

        protected override void OnPause()
        {
            _ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            base.OnPause();
        }

        protected virtual void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_ViewModel.ServerSecureConnections))
            {
                _BasePickingConfigRepository.SaveConfig(new Config("SecureConnections", _ViewModel.ServerSecureConnections.ToString()));
            }
            //workflow filter selection changed
            else if (e.PropertyName == nameof(_ViewModel.SelectedWorkflowFilter))
            {
                //save the new workflow filter value to config
                var newconfig = new Config("WorkflowFilterChoice",
                    LocalizationHelper.WorkflowReverseTranslate[_ViewModel.SelectedWorkflowFilter]);
                _BasePickingConfigRepository.SaveConfig(newconfig);

                _ViewModel.ServerSettingsVisible = ShouldShowServerSettings() || ShouldShowLegacyServerSettings();
                _ViewModel.LegacyServerSettingsVisible = ShouldShowLegacyServerSettings();

                //make sure the data proxy is using the correct transport
                _BasePickingDataProxy.SelectTransport(DataTransport());
            }
        }

        protected virtual void OnHostEntryLosesFocus()
        {
            _BasePickingConfigRepository.SaveConfig(new Config("Host", _ViewModel.Host));
        }

        protected virtual void OnPortEntryLosesFocus()
        {
            _BasePickingConfigRepository.SaveConfig(new Config("Port", _ViewModel.Port));
        }

        protected virtual void OnODRPortEntryLosesFocus()
        {
            _BasePickingConfigRepository.SaveConfig(new Config("ODRPort", _ViewModel.ODRPort));
        }

        protected bool ShouldShowServerSettings()
        {
            return _BasePickingConfigRepository.GetConfig("WorkflowFilterChoice").Value
                == LocalizationHelper.ServerLocalizationKey;
        }

        protected bool ShouldShowLegacyServerSettings()
        {
            return _BasePickingConfigRepository.GetConfig("WorkflowFilterChoice").Value
                == LocalizationHelper.LegacySocketServerLocalizationKey;
        }

        protected string DataTransport()
        {
            string workflow = _BasePickingConfigRepository.GetConfig("WorkflowFilterChoice").Value;
            if (workflow == LocalizationHelper.ServerLocalizationKey)
            {
                return RESTDataTransport;
            }
            if (workflow == LocalizationHelper.LegacySocketServerLocalizationKey)
            {
                return TCPSocketDataTransport;
            }

            return FileDataTransport;
        }
    }
}
