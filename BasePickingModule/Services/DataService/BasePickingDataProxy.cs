//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System.Collections.Generic;
    using System.Linq;

    public class BasePickingDataProxy : IBasePickingDataProxy
    {
        private readonly IEnumerable<IBasePickingDataTransport> _DataTransports;
        private readonly IBasePickingTCPSocketServicePropChangeManager _PropChangeManager;
        public IBasePickingDataTransport DataTransport { get; private set; }

        public BasePickingDataProxy(IEnumerable<IBasePickingDataTransport> dataTransports, IBasePickingTCPSocketServicePropChangeManager propChangeManager)
        {
            // Enumerate all of the dependencies now.  If there's something wrong in the 
            // dependency hierarchy of one of the data transports, I want to know about it via an exception
            // now, rather than playing whack-a-mole later.
            _DataTransports = new List<IBasePickingDataTransport>(dataTransports);

            _PropChangeManager = propChangeManager;
        }

        public void SelectTransport(string transportName)
        {
            // Throws if no transport with that name is found.
            DataTransport = _DataTransports.First(transport => transport.Name == transportName);
        }
    }
}
