//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Collections.ObjectModel;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;

    public class OrderPickingOrderStatusViewModel : SingleResponseViewModel
    {
        public string Header { get; set; }
        public OrderPickingOrderStatusListItemViewModel CurrentWorkItem { get; set; }
        public ObservableCollection<OrderPickingOrderStatusListItemViewModel> CurrentAndUpcomingPicks { get; set; }
        public OrderPickingOrderStatusViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {

        }
    }
}
