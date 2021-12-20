//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    public interface IBasePickingDataProxy
    {
        // The currently selected transport
        IBasePickingDataTransport DataTransport { get; }

        /// <summary>
        /// Request that the proxy select the named transport.
        /// </summary>
        /// <param name="transportName">The name of the requested transport</param>
        void SelectTransport(string transportName);
    }
}
