//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using System.Threading;
    using System.Threading.Tasks;

    public class VoiceLinkTCPSocketServiceProvider : VoiceLinkBaseServiceProvider, IVoiceLinkTCPSocketServiceProvider
    {
        private readonly IVoiceLinkTCPSocketService _TCPSocketService;

        public VoiceLinkTCPSocketServiceProvider(IVoiceLinkTCPSocketService TCPSocketService)
        {
            _TCPSocketService = TCPSocketService;
        }

        protected override uint RequestsPending()
        {
            return _TCPSocketService.PersistedRequestsPending;
        }

        protected override Task<string> SendRequest(Command command, bool asODR, CancellationToken cancellationToken, Command logCommand)
        {
            string logData = null;
            if (logCommand != null)
            {
                logData = logCommand.TCPFormat();
            }

            if (asODR)
            {
                _TCPSocketService.ExecuteTCPSocketDataAsync(command.TCPFormat(), true, asODR, cancellationToken, logData);
                return Task.FromResult("0");
            }
            return _TCPSocketService.ExecuteTCPSocketDataAsync(command.TCPFormat(), false, asODR, cancellationToken, logData);
        }
    }
}
