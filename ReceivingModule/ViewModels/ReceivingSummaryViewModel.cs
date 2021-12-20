//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System;
    using System.Collections.ObjectModel;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;

    public class ReceivingSummaryViewModel : SingleResponseViewModel
    {
        public string Header { get; set; }
        public ReceivingSummaryListViewModel CurrentWorkItem { get; set; }
        public ObservableCollection<ReceivingSummaryListViewModel> CurrentAndUpcomingPicks { get; set; }
        public ReceivingSummaryViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {

        }
    }
}
