//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System.Collections.Generic;
    using System.Linq;
    using GuidedWork;

    public class LAppDataProxy : ILAppDataProxy
    {
        private readonly IEnumerable<ILAppDataTransport> _DataTransports;
        private readonly ILAppRESTServicePropChangeManager _PropChangeManager;
        public ILAppDataTransport DataTransport { get; private set; }

        public LAppDataProxy(IDataProxy dataProxy, IEnumerable<ILAppDataTransport> dataTransports, ILAppRESTServicePropChangeManager propChangeManager)
        {
            // Enumerate all of the dependencies now.  If there's something wrong in the 
            // dependency hierarchy of one of the data transports, I want to know about it via an exception
            // now, rather than playing whack-a-mole later.
            _DataTransports = new List<ILAppDataTransport>(dataTransports);

            // Handle the notification from the base DataProxy that a 
            // transport has been selected.
            dataProxy.OnTransportSelected += SelectTransport;

            _PropChangeManager = propChangeManager;
        }

        public void SelectTransport(string transportName)
        {
            // Throws if no transport with that name is found.
            string workingTransportName = _PropChangeManager.Enabled ? "RESTDataTransport" : "FileDataTransport";
            DataTransport = _DataTransports.First(transport => transport.Name == workingTransportName);
        }
    }
}
