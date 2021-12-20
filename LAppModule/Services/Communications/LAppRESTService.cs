//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using Honeywell.Firebird.CoreLibrary;
    using RESTCommunication;

    public class LAppRESTService : RESTService, ILAppRESTService
    {
        public LAppRESTService(ILAppRESTServicePropChangeManager RESTServicePropChangeManager,
            ILAppRESTHeaderUtilities RESTHeaderUtilities,
            IRESTQueue restQueue,
            ITimeoutHandler restTimeoutHandler) :
            base(RESTServicePropChangeManager, RESTHeaderUtilities, restQueue, restTimeoutHandler)
        {
        }
    }
}
