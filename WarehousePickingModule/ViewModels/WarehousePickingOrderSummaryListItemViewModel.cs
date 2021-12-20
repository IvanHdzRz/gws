﻿//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using Honeywell.Firebird.CoreLibrary;
    using GuidedWork;

    public class WarehousePickingOrderSummaryListItemViewModel : BindableBase
    {
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public string Aisle { get; set; }
        public string SlotID { get; set; }
        public string PickQuantity { get; set; }
        public string ShortedQuantity { get; set; }
        public bool IsComplete { get; set; }
    }
}
