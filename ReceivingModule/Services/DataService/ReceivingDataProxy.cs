//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Collections.Generic;
    using System.Linq;
    using GuidedWork;

    public class ReceivingDataProxy : IReceivingDataProxy
    {
        private readonly IEnumerable<IReceivingDataTransport> _DataTransports;
        public IReceivingDataTransport DataTransport { get; private set; }

        public ReceivingDataProxy(IDataProxy dataProxy, IEnumerable<IReceivingDataTransport> dataTransports)
        {
            // Enumerate all of the dependencies now.  If there's something wrong in the 
            // dependency hierarchy of one of the data transports, I want to know about it via an exception
            // now, rather than playing whack-a-mole later.
            _DataTransports = new List<IReceivingDataTransport>(dataTransports);

            // Handle the notification from the base DataProxy that a 
            // transport has been selected.
            dataProxy.OnTransportSelected += SelectTransport;
        }

        private void SelectTransport(string transportName)
        {
            // Throws if no transport with that name is found.
            DataTransport = _DataTransports.First(transport => transport.Name == transportName);
        }
    }
}
