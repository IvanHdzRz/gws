//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using SQLite;
    using GuidedWork;

    public class OrderPickingContainer
    {
        public bool ContainsProduct { get; set; }
        public bool Full { get; set; }
        public string Identifier { get; set; }
        public long OrderId { get; set; } 
        public string OrderIdentifier { get; set; }
        public string StagingLocation { get; set; }
    }

    public class OrderPickingWorkItem : WorkItem
    {
        public long ProductID { get; set; }
        public long OrderInfoID { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; }
    }

    public class OrderPickingOrderInfo 
    {
        [PrimaryKey]
        public long ID { get; set; }
        public string OrderIdentifier { get; set; }
        public string RequestedCompletionDate { get; set; }
    }

    public class ProductSubstitutionMap
    {
        [PrimaryKey]
        public long ID { get; set; }
        public long OrderPickingWorkItemID { get; set; }
        public long ProductID { get; set; }
        public int SubstitutionPriority { get; set; }
        public bool Used { get; set; }
    }

    public class OrderPickingSummaryItem
    {
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public string Quantity { get; set; }
    }
}
