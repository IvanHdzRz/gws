//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using GuidedWork;

    public class ReceivingWorkItem : WorkItem
    {
        public string ProductIdentifier { get; set; }
        public string ProductNumbersString { get; set; }
        public string ProductName { get; set; }
        public string ProductDestinationText { get; set; }
        public string ProductImageName { get; set; }
        public string ProductImagePath { get; set; }
        public int RequestedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public int LastReceivedQuantity { get; set; }
        public bool Damaged { get; set; }
    }

    public class ReceivingSummaryItem
    {
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public string ProductIdentifier { get; set; }
        public string RequestedQuantity { get; set; }
        public string RemainingQuantity { get; set; }
        public bool IsComplete { get; set; }
        public bool IsDamaged { get; set; }
    }
}
