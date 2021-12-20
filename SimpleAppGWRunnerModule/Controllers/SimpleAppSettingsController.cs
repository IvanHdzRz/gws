//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace SimpleApp
{
    using System;
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;

    public class SimpleAppSettingsController : NavigatingMenuController
    {
        private readonly ISimpleAppConfigRepository _SimpleAppConfigRepository;

        protected SimpleAppSettingsViewModel _ViewModel;

        public SimpleAppSettingsController(CoreViewControllerDependencies dependencies,
                                           ISimpleAppConfigRepository simpleAppConfigRepository)
            : base(dependencies)
        {
            _SimpleAppConfigRepository = simpleAppConfigRepository;
        }

        protected override IWorkflowViewModel CreateViewModel(string viewModelName)
        {
            _ViewModel = (SimpleAppSettingsViewModel)base.CreateViewModel(viewModelName);

            _ViewModel.OnHostEntryLostFocus = new Command(OnHostEntryLosesFocus);
            _ViewModel.OnPortEntryLostFocus = new Command(OnPortEntryLosesFocus);

            _ViewModel.Host = _SimpleAppConfigRepository.GetConfig(SimpleAppConfigRepository.HOST).Value;
            _ViewModel.Port = _SimpleAppConfigRepository.GetConfig(SimpleAppConfigRepository.PORT).Value;

            return _ViewModel;
        }

        protected virtual void OnHostEntryLosesFocus()
        {
            _SimpleAppConfigRepository.SaveConfig(new Config(SimpleAppConfigRepository.HOST, _ViewModel.Host));
        }

        protected virtual void OnPortEntryLosesFocus()
        {
            _SimpleAppConfigRepository.SaveConfig(new Config(SimpleAppConfigRepository.PORT, _ViewModel.Port));
        }
    }
}
