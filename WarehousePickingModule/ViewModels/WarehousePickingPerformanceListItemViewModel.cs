//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using Honeywell.Firebird.CoreLibrary;
    using GuidedWork;

    public class WarehousePickingPerformanceListItemViewModel : BindableBase
    {
        public string ActivityName { get; set; }
        public string ActivityDuration { get; set; }
    }
}
