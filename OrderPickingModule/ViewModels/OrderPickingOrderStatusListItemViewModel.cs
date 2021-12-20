//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using Honeywell.Firebird.CoreLibrary;

    public class OrderPickingOrderStatusListItemViewModel : BindableBase
    {
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public string RequestedQuantity { get; set; }
    }
}
