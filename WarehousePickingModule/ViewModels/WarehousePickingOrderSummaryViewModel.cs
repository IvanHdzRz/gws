//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Collections.ObjectModel;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;

    public class WarehousePickingOrderSummaryViewModel : SingleResponseViewModel
    {
        public string Header { get; set; }
        public WarehousePickingOrderSummaryListItemViewModel CurrentWorkItem { get; set; }
        public ObservableCollection<WarehousePickingOrderSummaryListItemViewModel> CurrentAndUpcomingPicks { get; set; }
        public WarehousePickingOrderSummaryViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {

        }
    }
}
