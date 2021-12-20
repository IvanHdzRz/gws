//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using System.Collections.Generic;
    using System.Linq;

    public class VoiceLinkDataProxy : IVoiceLinkDataProxy
    {
        private readonly IEnumerable<IVoiceLinkDataTransport> _DataTransports;
        public IVoiceLinkDataTransport DataTransport { get; private set; }

        public VoiceLinkDataProxy(IEnumerable<IVoiceLinkDataTransport> dataTransports)
        {
            // Enumerate all of the dependencies now.  If there's something wrong in the 
            // dependency hierarchy of one of the data transports, I want to know about it via an exception
            // now, rather than playing whack-a-mole later.
            _DataTransports = new List<IVoiceLinkDataTransport>(dataTransports);
        }

        public void SelectTransport(string transportName)
        {
            // Throws if no transport with that name is found.
            DataTransport = _DataTransports.First(transport => transport.Name == transportName);
        }
    }
}
