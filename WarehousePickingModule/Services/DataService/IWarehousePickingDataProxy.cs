//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    /// <summary>
    /// An interface that proxies between different
    /// <see cref="IWarehousePickingDataTransport"/> implementations.
    /// </summary>
    public interface IWarehousePickingDataProxy
    {
        /// <summary>
        /// The currently selected <see cref="IWarehousePickingDataTransport"/>
        /// transport.
        /// </summary>
        IWarehousePickingDataTransport DataTransport { get; }
    }
}
