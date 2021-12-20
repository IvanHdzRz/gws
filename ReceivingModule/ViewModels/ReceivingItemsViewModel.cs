//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Honeywell.Firebird.WorkflowEngine;
    using Honeywell.Firebird.CoreLibrary;

    public class ReceivingItemsViewModel : DigitEntryViewModel
    {
        public string Header { get; set; }
        public ReceivingItemsListViewModel CurrentWorkItem { get; set; }
        public ObservableCollection<ReceivingItemsListViewModel> CurrentAndUpcomingPicks { get; set; }

        public IReadOnlyList<string> OverflowMenuItems { get; set; }

        public ReceivingItemsViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {

        }
    }
}
