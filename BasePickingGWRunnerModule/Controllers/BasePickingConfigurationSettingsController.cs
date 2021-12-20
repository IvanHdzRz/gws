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

    /// <summary>
    /// This class handles initialization of the BasePicking configuration settings menu.
    /// </summary>
    public class BasePickingConfigurationSettingsController : NavigatingMenuController
    {
        private readonly IBasePickingConfigRepository _BasePickingConfigRepository;
        private readonly ILog _Log = LogManager.GetLogger(nameof(BasePickingConfigurationSettingsController));

        protected BasePickingConfigurationSettingsViewModel _ViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePickingConfigurationSettingsController"/> class.
        /// </summary>
        public BasePickingConfigurationSettingsController(CoreViewControllerDependencies dependencies,
                                            IBasePickingConfigRepository basePickingConfigRepository)
            : base(dependencies)
        {
            _BasePickingConfigRepository = basePickingConfigRepository;
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            _ViewModel = (BasePickingConfigurationSettingsViewModel)base.CreateViewModel(viewModelName);

            _ViewModel.PickMethodChoices = new List<string> { BasePickingPickMethod.Discrete, BasePickingPickMethod.Cluster };
            _ViewModel.SelectedPickMethod = _BasePickingConfigRepository.GetConfig("PickMethod").Value;
            _ViewModel.PickQuantityCountdown = bool.Parse(_BasePickingConfigRepository.GetConfig("PickQuantityCountdown").Value);
            _ViewModel.ConfirmLocation = bool.Parse(_BasePickingConfigRepository.GetConfig("ConfirmLocation").Value);
            _ViewModel.ConfirmProduct = bool.Parse(_BasePickingConfigRepository.GetConfig("ConfirmProduct").Value);
            _ViewModel.ConfirmQuantityVoiceInput = bool.Parse(_BasePickingConfigRepository.GetConfig("ConfirmQuantityVoiceInput").Value);
            _ViewModel.ConfirmQuantityScreenInput = bool.Parse(_BasePickingConfigRepository.GetConfig("ConfirmQuantityScreenInput").Value);
            _ViewModel.HideConfigurationSettings = !bool.Parse(_BasePickingConfigRepository.GetConfig("ShowConfigurationSettings").Value);
            _ViewModel.ShowHints = bool.Parse(_BasePickingConfigRepository.GetConfig("ShowHints").Value);

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
            if (e.PropertyName == nameof(_ViewModel.SelectedPickMethod))
            {
                _BasePickingConfigRepository.SaveConfig(new Config("PickMethod", _ViewModel.SelectedPickMethod));
            }
            else if (e.PropertyName == nameof(_ViewModel.PickQuantityCountdown))
            {
                _BasePickingConfigRepository.SaveConfig(new Config("PickQuantityCountdown", _ViewModel.PickQuantityCountdown.ToString()));
            }
            else if (e.PropertyName == nameof(_ViewModel.ConfirmLocation))
            {
                _BasePickingConfigRepository.SaveConfig(new Config("ConfirmLocation", _ViewModel.ConfirmLocation.ToString()));
            }
            else if (e.PropertyName == nameof(_ViewModel.ConfirmProduct))
            {
                _BasePickingConfigRepository.SaveConfig(new Config("ConfirmProduct", _ViewModel.ConfirmProduct.ToString()));
            }
            else if (e.PropertyName == nameof(_ViewModel.ConfirmQuantityVoiceInput))
            {
                _BasePickingConfigRepository.SaveConfig(new Config("ConfirmQuantityVoiceInput", _ViewModel.ConfirmQuantityVoiceInput.ToString()));
            }
            else if (e.PropertyName == nameof(_ViewModel.ConfirmQuantityScreenInput))
            {
                _BasePickingConfigRepository.SaveConfig(new Config("ConfirmQuantityScreenInput", _ViewModel.ConfirmQuantityScreenInput.ToString()));
            }
            else if (e.PropertyName == nameof(_ViewModel.HideConfigurationSettings))
            {
                _BasePickingConfigRepository.SaveConfig(new Config("ShowConfigurationSettings", (!_ViewModel.HideConfigurationSettings).ToString()));
            }
            else if (e.PropertyName == nameof(_ViewModel.ShowHints))
            {
                _BasePickingConfigRepository.SaveConfig(new Config("ShowHints", _ViewModel.ShowHints.ToString()));
            }
        }
    }
}
