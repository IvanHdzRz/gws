//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System.Runtime.Serialization;
    using Honeywell.GuidedWork.Communication;
    using TCPSocketCommunication;
    using XFusion.Infrastructure.FileStorage;

    public class BasePickingTCPSocketQueue : TCPSocketQueue, IBasePickingTCPSocketQueue
    {
        public BasePickingTCPSocketQueue(IFileSystem fileSystem, IFormatter formatter, PinningUtil pinningUtil) : base(fileSystem, formatter, pinningUtil)
        {
        }
    }
}
