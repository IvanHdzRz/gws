//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using System.Runtime.Serialization;
    using Honeywell.GuidedWork.Communication;
    using TCPSocketCommunication;
    using XFusion.Infrastructure.FileStorage;

    public class VoiceLinkTCPSocketQueue : TCPSocketQueue, IVoiceLinkTCPSocketQueue
    {
        public VoiceLinkTCPSocketQueue(IFileSystem fileSystem, IFormatter formatter, PinningUtil pinningUtil) : base(fileSystem, formatter, pinningUtil)
        {
        }
    }
}
