//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using GuidedWork;

    public class WarehousePickingWorkItem : WorkItem
    {
        public long ProductID { get; set; }

        public string Aisle { get; set; }
        public string SlotID { get; set; }
        public int SubCenterID { get; set; }
        public string CheckDigit { get; set; }
        public int PickQuantity { get; set; }
        public int PickedQuantity { get; set; }
        public bool ShortedIndicator { get; set; }
        public string TripID { get; set; }
        public string StoreNumber { get; set; }
        public string DoorNumber { get; set; }
        public string CheckPattern { get; set; }
        public string RouteNumber { get; set; }
    }

    public class WarehousePickingSummaryItem
    {
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public string PickQuantity { get; set; }
        public string Aisle { get; set; }
        public string SlotID { get; set; }
        public string ShortedQuantity { get; set; }
        public bool IsComplete { get; set; }
    }
}
