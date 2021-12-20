//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Collections.ObjectModel;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;

    public class WarehousePickingPerformanceViewModel : SingleResponseViewModel
    {
        public string Header { get; set; }
        public WarehousePickingPerformanceListItemViewModel CurrentWorkItem { get; set; }
        public ObservableCollection<WarehousePickingPerformanceListItemViewModel> CurrentAndUpcomingPicks { get; set; }
        public WarehousePickingPerformanceViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {

        }
    }
}
