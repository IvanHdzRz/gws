//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    public interface IOrderPickingDataProxy
    {
        // The currently selected transport
        IOrderPickingDataTransport DataTransport { get; }
    }
}
