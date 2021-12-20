//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using Honeywell.Firebird.CoreLibrary;
    using RESTCommunication;

    public class BasePickingRESTService : RESTService, IBasePickingRESTService
    {
        public BasePickingRESTService(IBasePickingRESTServicePropChangeManager RESTServicePropChangeManager,
            IRESTHeaderUtilities RESTHeaderUtilities,
            IRESTQueue restQueue,
            ITimeoutHandler timeoutHandler) :
            base(RESTServicePropChangeManager, RESTHeaderUtilities, restQueue, timeoutHandler)
        {
        }
    }
}
