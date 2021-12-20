//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using Honeywell.Firebird.CoreLibrary;
    using GuidedWork;

    public class ReceivingSummaryListViewModel : BindableBase
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
