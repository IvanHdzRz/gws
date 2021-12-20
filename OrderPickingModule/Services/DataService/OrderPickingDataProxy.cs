//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Collections.Generic;
    using System.Linq;
    using GuidedWork;
    using Retail;

    public class OrderPickingDataProxy : IOrderPickingDataProxy
    {
        private readonly IEnumerable<IOrderPickingDataTransport> _DataTransports;
        private readonly IRetailRESTServicePropChangeManager _PropChangeManager;
        public IOrderPickingDataTransport DataTransport { get; private set; }

        public OrderPickingDataProxy(IDataProxy dataProxy, IEnumerable<IOrderPickingDataTransport> dataTransports, IRetailRESTServicePropChangeManager propChangeManager)
        {
            // Enumerate all of the dependencies now.  If there's something wrong in the 
            // dependency hierarchy of one of the data transports, I want to know about it via an exception
            // now, rather than playing whack-a-mole later.
            _DataTransports = new List<IOrderPickingDataTransport>(dataTransports);

            // Handle the notification from the base DataProxy that a 
            // transport has been selected.
            dataProxy.OnTransportSelected += SelectTransport;

            _PropChangeManager = propChangeManager;
        }

        private void SelectTransport(string transportName)
        {
            // Throws if no transport with that name is found.
            string workingTransportName = _PropChangeManager.Enabled ? "RESTDataTransport" : "FileDataTransport";
            DataTransport = _DataTransports.First(transport => transport.Name == workingTransportName);
        }
    }
}
