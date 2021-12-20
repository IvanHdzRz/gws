//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Collections.Generic;
    using Retail;

    /// <summary>
    /// interface containing properties neccessary for view models being
    /// used by OrderPickingView
    /// </summary>
    public interface IOrderPickingViewModel
    {
        string CurrentProductIndex { get; set; }
        uint ExpectedStockCodeResponseLength { get; set; }
        List<LocationDescriptor> LocationDescriptors { get; set; }
        IReadOnlyList<string> OverflowMenuItems { get; set; }
        string ProductIdentifier { get; set; }
        string TotalProducts { get; set; }
    }
}
