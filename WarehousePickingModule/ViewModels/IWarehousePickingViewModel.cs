//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Collections.Generic;
    using GuidedWork;

    /// <summary>
    /// interface containing properties neccessary for view models being
    /// used by WarehousePickingView
    /// </summary>
    public interface IWarehousePickingViewModel
    {
        string CurrentProductIndex { get; set; }
        List<LocationDescriptor> LocationDescriptors { get; set; }
        string ProductIdentifier { get; set; }
        string TotalProducts { get; set; }
    }
}
