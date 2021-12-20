//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System.Collections.Generic;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;

    public class BasePickingConfigurationSettingsViewModel : NavigatingMenuViewModel
    {
        public BasePickingConfigurationSettingsViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {
        }

        public IList<string> PickMethodChoices { get; set; }

        private string _SelectedPickMethod;
        public string SelectedPickMethod
        {
            get
            {
                return _SelectedPickMethod;
            }

            set
            {
                _SelectedPickMethod = value;
                NotifyPropertyChanged();
            }
        }

        private bool _PickQuantityCountdown;
        public bool PickQuantityCountdown
        {
            get
            {
                return _PickQuantityCountdown;
            }

            set
            {
                _PickQuantityCountdown = value;
                NotifyPropertyChanged();
            }
        }

        private bool _ConfirmLocation;
        public bool ConfirmLocation
        {
            get
            {
                return _ConfirmLocation;
            }

            set
            {
                _ConfirmLocation = value;
                NotifyPropertyChanged();
            }
        }

        private bool _ConfirmProduct;
        public bool ConfirmProduct
        {
            get
            {
                return _ConfirmProduct;
            }

            set
            {
                _ConfirmProduct = value;
                NotifyPropertyChanged();
            }
        }

        private bool _ConfirmQuantityVoiceInput;
        public bool ConfirmQuantityVoiceInput
        {
            get
            {
                return _ConfirmQuantityVoiceInput;
            }

            set
            {
                _ConfirmQuantityVoiceInput = value;
                NotifyPropertyChanged();
            }
        }

        private bool _ConfirmQuantityScreenInput;
        public bool ConfirmQuantityScreenInput
        {
            get
            {
                return _ConfirmQuantityScreenInput;
            }

            set
            {
                _ConfirmQuantityScreenInput = value;
                NotifyPropertyChanged();
            }
        }

        private bool _HideConfigurationSettings;
        public bool HideConfigurationSettings
        {
            get
            {
                return _HideConfigurationSettings;
            }

            set
            {
                _HideConfigurationSettings = value;
                NotifyPropertyChanged();
            }
        }

        private bool _ShowHints;
        public bool ShowHints
        {
            get
            {
                return _ShowHints;
            }

            set
            {
                _ShowHints = value;
                NotifyPropertyChanged();
            }
        }
    }
}
