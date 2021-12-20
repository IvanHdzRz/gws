//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    /// <summary>
    /// interface containing properties neccessary for view models being
    /// used by OrderPickingView
    /// </summary>
    public interface IReceivingViewModel
    {
        string CurrentProductIndex { get; set; }
        string ProductIdentifier { get; set; }
        string TotalProducts { get; set; }
    }
}
