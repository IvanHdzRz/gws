//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using Honeywell.Firebird.CoreLibrary;
    using TCPSocketCommunication;

    public class BasePickingTCPSocketService : TCPSocketService, IBasePickingTCPSocketService
    {
        public BasePickingTCPSocketService(IBasePickingTCPSocketServicePropChangeManager basePickingTCPSocketServicePropChangeManager,
            IBasePickingTCPSocketQueue tcpQueue,
            ITimeoutHandler timeoutHandler) :
            base(basePickingTCPSocketServicePropChangeManager, tcpQueue, timeoutHandler)
        {
        }
    }
}
