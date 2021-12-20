//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Collections.Generic;
    using System.Linq;
    using GuidedWork;

    /// <summary>
    /// An class that proxies between different
    /// <see cref="IWarehousePickingDataTransport"/> implementations.
    /// </summary>
    public class WarehousePickingDataProxy : IWarehousePickingDataProxy
    {
        private readonly IEnumerable<IWarehousePickingDataTransport> _DataTransports;
        public IWarehousePickingDataTransport DataTransport { get; private set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:WarehousePicking.WarehousePickingDataProxy"/> class.
        /// </summary>
        /// <param name="dataProxy">The proxy for the current Warehouse Picking
        /// Data transport.</param>
        /// <param name="dataTransports">The available Warehouse Picking Data
        /// transports.</param>
        public WarehousePickingDataProxy(IDataProxy dataProxy, IEnumerable<IWarehousePickingDataTransport> dataTransports)
        {
            // Enumerate all of the dependencies now.  If there's something wrong in the 
            // dependency hierarchy of one of the data transports, I want to know about it via an exception
            // now, rather than playing whack-a-mole later.
            _DataTransports = new List<IWarehousePickingDataTransport>(dataTransports);

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
