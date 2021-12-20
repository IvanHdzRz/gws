//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using Honeywell.Firebird.CoreLibrary;
    using TCPSocketCommunication;

    public class VoiceLinkTCPSocketService : TCPSocketService, IVoiceLinkTCPSocketService
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="voiceLinkTCPSocketServicePropChangeManager">Property change manager</param>
        /// <param name="tcpQueue">Socket Queue</param>
        public VoiceLinkTCPSocketService(IVoiceLinkTCPSocketServicePropChangeManager voiceLinkTCPSocketServicePropChangeManager,
            IVoiceLinkTCPSocketQueue tcpQueue, ITimeoutHandler tcpTimeoutHandler) :
            base(voiceLinkTCPSocketServicePropChangeManager, tcpQueue, tcpTimeoutHandler)
        {
        }

        /// <summary>
        /// Constructor for when running on VoiceCatayst device that includes prohibitor to prevent
        /// accidental task loads and operator changes while there is still data queue 
        /// up o be sent.
        /// </summary>
        /// <param name="voiceLinkTCPSocketServicePropChangeManager">Property change manager</param>
        /// <param name="tcpQueue">Socket Queue</param>
        /// <param name="prohibitor">Prohibitor for when running on VoiceCatalyst device</param>
        public VoiceLinkTCPSocketService(IVoiceLinkTCPSocketServicePropChangeManager voiceLinkTCPSocketServicePropChangeManager,
            IVoiceLinkTCPSocketQueue tcpQueue, IVoiceCatalystProhibitor prohibitor, ITimeoutHandler tcpTimeoutHandler) :
            base(voiceLinkTCPSocketServicePropChangeManager, tcpQueue, tcpTimeoutHandler)
        {
            Prohibitor = prohibitor;
            Prohibitor.ProhibitAcquire((int)tcpQueue.Count);
        }
    }
}
