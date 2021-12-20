//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Collections.ObjectModel;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;

    public class WarehousePickingOrderStatusViewModel : SingleResponseViewModel
    {
        public string Header { get; set; }
        public WarehousePickingOrderStatusListItemViewModel CurrentWorkItem { get; set; }
        public ObservableCollection<WarehousePickingOrderStatusListItemViewModel> CurrentAndUpcomingPicks { get; set; }
        public WarehousePickingOrderStatusViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {

        }
    }
}
