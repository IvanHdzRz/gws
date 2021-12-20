//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Common.Logging;
    using GuidedWork;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;

    /// <summary>
    /// This class handles initialization of the VoiceLink settings menu.
    /// </summary>
    public class VoiceLinkServerSettingsController : NavigatingMenuController
    {
        private readonly IVoiceLinkConfigRepository _VoiceLinkConfigRepository;
        private readonly IVoiceLinkDataProxy _VoiceLinkDataProxy;
        private readonly ILog _Log = LogManager.GetLogger(nameof(VoiceLinkServerSettingsController));
        private const string FileDataTransport = "FileDataTransport";
        private const string RESTDataTransport = "RESTDataTransport";
        private const string TCPSocketDataTransport = "TCPSocketDataTransport";

        protected VoiceLinkServerSettingsViewModel _ViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceLinkServerSettingsController"/> class.
        /// </summary>
        public VoiceLinkServerSettingsController(CoreViewControllerDependencies dependencies,
                                                 IVoiceLinkConfigRepository voiceLinkConfigRepository,
                                                 IVoiceLinkDataProxy voiceLinkDataProxy)
            : base(dependencies)
        {
            _VoiceLinkConfigRepository = voiceLinkConfigRepository;
            _VoiceLinkDataProxy = voiceLinkDataProxy;
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            _ViewModel = (VoiceLinkServerSettingsViewModel)base.CreateViewModel(viewModelName);

            _ViewModel.OnHostEntryLostFocus = new Command(OnHostEntryLosesFocus);
            _ViewModel.OnPortEntryLostFocus = new Command(OnPortEntryLosesFocus);
            _ViewModel.OnODRPortEntryLostFocus = new Command(OnODRPortEntryLosesFocus);
            _ViewModel.OnSiteNameEntryLostFocus = new Command(OnSiteNameEntryLosesFocus);

            _ViewModel.WorkflowFilterSettingsVisible = true;

            //set the available workflow filter choices on the view model
            _ViewModel.WorkflowFilterChoices = new List<string>
            {
                LocalizationHelper.LocalizedEmbeddedDemoWorkflowFilterName,
                LocalizationHelper.LocalizedServerWorkflowFilterName,
                LocalizationHelper.LocalizedLegacySocketServerWorkflowFilterName
            };

            bool serverSecureConnections;
            Boolean.TryParse(_VoiceLinkConfigRepository.GetConfig("SecureConnections").Value, out serverSecureConnections);
            _ViewModel.ServerSecureConnections = serverSecureConnections;

            //set the selected workflow filter from config on the view model
            _ViewModel.SelectedWorkflowFilter = GetLocalizedText(_VoiceLinkConfigRepository.GetConfig("WorkflowFilterChoice").Value);

            //set whether the server settings (host, port) should be visible
            //based on the workflow filter selected
            _ViewModel.ServerSettingsVisible = ShouldShowServerSettings() || ShouldShowLegacyServerSettings();
            _ViewModel.LegacyServerSettingsVisible = ShouldShowLegacyServerSettings();

            _ViewModel.Host = _VoiceLinkConfigRepository.GetConfig("Host").Value;
            _ViewModel.Port = _VoiceLinkConfigRepository.GetConfig("Port").Value;
            _ViewModel.ODRPort = _VoiceLinkConfigRepository.GetConfig("ODRPort").Value;
            _ViewModel.SiteName = _VoiceLinkConfigRepository.GetConfig("SiteName").Value;
            _VoiceLinkDataProxy.SelectTransport(DataTransport());

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
                _VoiceLinkConfigRepository.SaveConfig(new Config("SecureConnections", _ViewModel.ServerSecureConnections.ToString()));
            }
           
            //workflow filter selection changed
            if (e.PropertyName == nameof(_ViewModel.SelectedWorkflowFilter))
            {
                //save the new workflow filter value to config
                var newconfig = new Config("WorkflowFilterChoice",
                    LocalizationHelper.WorkflowReverseTranslate[_ViewModel.SelectedWorkflowFilter]);
                _VoiceLinkConfigRepository.SaveConfig(newconfig);

                _ViewModel.ServerSettingsVisible = ShouldShowServerSettings() || ShouldShowLegacyServerSettings();
                _ViewModel.LegacyServerSettingsVisible = ShouldShowLegacyServerSettings();

                //make sure the data proxy is using the correct transport
                _VoiceLinkDataProxy.SelectTransport(DataTransport());
            }
        }

        protected virtual void OnHostEntryLosesFocus()
        {
            _VoiceLinkConfigRepository.SaveConfig(new Config("Host", _ViewModel.Host));
        }

        protected virtual void OnPortEntryLosesFocus()
        {
            _VoiceLinkConfigRepository.SaveConfig(new Config("Port", _ViewModel.Port));
        }

        protected virtual void OnODRPortEntryLosesFocus()
        {
            _VoiceLinkConfigRepository.SaveConfig(new Config("ODRPort", _ViewModel.ODRPort));
        }

        protected virtual void OnSiteNameEntryLosesFocus()
        {
            _VoiceLinkConfigRepository.SaveConfig(new Config("SiteName", _ViewModel.SiteName));
        }

        protected bool ShouldShowServerSettings()
        {
            return _VoiceLinkConfigRepository.GetConfig("WorkflowFilterChoice").Value
                == LocalizationHelper.ServerLocalizationKey;
        }

        protected bool ShouldShowLegacyServerSettings()
        {
            return _VoiceLinkConfigRepository.GetConfig("WorkflowFilterChoice").Value
                == LocalizationHelper.LegacySocketServerLocalizationKey;
        }

        protected string DataTransport()
        {
            string workflow = _VoiceLinkConfigRepository.GetConfig("WorkflowFilterChoice").Value;
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
