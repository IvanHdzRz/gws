//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    public interface IVoiceLinkDataProxy
    {
        // The currently selected transport
        IVoiceLinkDataTransport DataTransport { get; }

        /// <summary>
        /// Request that the proxy select the named transport.
        /// </summary>
        /// <param name="transportName">The name of the requested transport</param>
        void SelectTransport(string transportName);
    }
}
