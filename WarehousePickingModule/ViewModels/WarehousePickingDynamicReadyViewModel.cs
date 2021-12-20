//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWork;

    public class WarehousePickingDynamicReadyViewModel : ReadyViewModel
    {
        public string TripStandardTime { get; set; }
        public string TotalCases { get; set; }
        public string Stats { get; set; }
        public string LabelPrinter { get; set; }

        public WarehousePickingDynamicReadyViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {
        }
    }
}
