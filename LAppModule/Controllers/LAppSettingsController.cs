//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Common.Logging;
    using GuidedWork;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;

    /// <summary>
    /// This class handles initialization of the LApp settings menu.
    /// </summary>
    public class LAppSettingsController : CoreViewController
    {
        private readonly ILAppConfigRepository _LAppConfigRepository;
        private readonly ILAppDataProxy _LAppDataProxy;
        private readonly ILog _Log = LogManager.GetLogger(nameof(LAppSettingsController));
        private const string FileDataTransport = "FileDataTransport";
        private const string RESTDataTransport = "RESTDataTransport";

        protected LAppSettingsViewModel _ViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="LAppServerSettingsController"/> class.
        /// </summary>
        public LAppSettingsController(CoreViewControllerDependencies dependencies,
                                      ILAppConfigRepository LAppConfigRepository,
                                      ILAppDataProxy LAppDataProxy) : base(dependencies)
        {
            _LAppConfigRepository = LAppConfigRepository;
            _LAppDataProxy = LAppDataProxy;
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            _ViewModel = (LAppSettingsViewModel)base.CreateViewModel(viewModelName);

            _ViewModel.OnHostEntryLostFocus = new Command(OnHostEntryLosesFocus);
            _ViewModel.OnPortEntryLostFocus = new Command(OnPortEntryLosesFocus);
            _ViewModel.OnCompanyDBEntryLostFocus = new Command(OnCompanyDBEntryLosesFocus);

            _ViewModel.WorkflowFilterSettingsVisible = true;

            //set the available workflow filter choices on the view model
            _ViewModel.WorkflowFilterChoices = new List<string>
            {
                LocalizationHelper.LocalizedEmbeddedDemoWorkflowFilterName,
                LocalizationHelper.LocalizedServerWorkflowFilterName
            };

            //set the selected workflow filter from config on the view model
            _ViewModel.SelectedWorkflowFilter = GetLocalizedText(_LAppConfigRepository.GetConfig("WorkflowFilterChoice").Value);

            //set whether the server settings (host, port) should be visible
            //based on the workflow filter selected
            _ViewModel.ServerSettingsVisible = ShouldShowServerSettings();

            _ViewModel.Host = _LAppConfigRepository.GetConfig("Host").Value;
            _ViewModel.Port = _LAppConfigRepository.GetConfig("Port").Value;
            _ViewModel.CompanyDB = _LAppConfigRepository.GetConfig("CompanyDB").Value;

            if (ShouldShowServerSettings())
            {
                _LAppDataProxy.SelectTransport(RESTDataTransport);
            }
            else
            {
                _LAppDataProxy.SelectTransport(FileDataTransport);
            }

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
            //workflow filter selection changed
            if (e.PropertyName == nameof(_ViewModel.SelectedWorkflowFilter))
            {
                //save the new workflow filter value to config
                var newconfig = new Config("WorkflowFilterChoice",
                    LocalizationHelper.WorkflowReverseTranslate[_ViewModel.SelectedWorkflowFilter]);
                _LAppConfigRepository.SaveConfig(newconfig);

                //server mode was selected
                if (ShouldShowServerSettings())
                {
                    //make the server settings visible
                    _ViewModel.ServerSettingsVisible = true;

                    //make sure the data proxy is using the correct transport
                    _LAppDataProxy.SelectTransport(RESTDataTransport);
                }
                //embedded demo was selected
                else
                {
                    //make the server settings not visible
                    _ViewModel.ServerSettingsVisible = false;

                    //make sure the data proxy is using the correct transport
                    _LAppDataProxy.SelectTransport(FileDataTransport);
                }
            }
        }

        protected virtual void OnHostEntryLosesFocus()
        {
            _LAppConfigRepository.SaveConfig(new Config("Host", _ViewModel.Host));
        }

        protected virtual void OnPortEntryLosesFocus()
        {
            _LAppConfigRepository.SaveConfig(new Config("Port", _ViewModel.Port));
        }

        protected virtual void OnCompanyDBEntryLosesFocus()
        {
            _LAppConfigRepository.SaveConfig(new Config("CompanyDB", _ViewModel.CompanyDB));
        }

        protected bool ShouldShowServerSettings()
        {
            return _LAppConfigRepository.GetConfig("WorkflowFilterChoice").Value
                == LocalizationHelper.ServerLocalizationKey;
        }
    }
}
