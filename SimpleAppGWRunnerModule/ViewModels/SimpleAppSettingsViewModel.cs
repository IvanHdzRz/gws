//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace SimpleApp
{
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using System.Windows.Input;

    public class SimpleAppSettingsViewModel : NavigatingMenuViewModel
    {
        public SimpleAppSettingsViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {
        }

        public string Host { get; set; }

        public string Port { get; set; }

        public ICommand OnHostEntryLostFocus { get; set; }

        public ICommand OnPortEntryLostFocus { get; set; }
    }
}
