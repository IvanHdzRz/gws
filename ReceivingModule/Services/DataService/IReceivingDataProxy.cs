//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    public interface IReceivingDataProxy
    {
        // The currently selected transport
        IReceivingDataTransport DataTransport { get; }
    }
}
