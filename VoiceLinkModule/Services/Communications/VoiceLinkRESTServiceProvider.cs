//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using Newtonsoft.Json;
    using System.Threading;
    using System.Threading.Tasks;

    public class VoiceLinkRESTServiceProvider : VoiceLinkBaseServiceProvider, IVoiceLinkRESTServiceProvider
    {
        private readonly IVoiceLinkRESTService _RESTService;

        public VoiceLinkRESTServiceProvider(IVoiceLinkRESTService RESTService)
        {
            _RESTService = RESTService;
        }

        protected override uint RequestsPending()
        {
            return _RESTService.PersistedRequestsPending;
        }

        protected override Task<string> SendRequest(Command command, bool asODR, CancellationToken cancellationToken, Command logCommand = null)
        {
            string logData = null;
            if (logCommand != null)
            {
                logData = JsonConvert.SerializeObject(logCommand);
            }
            Task<string> result = _RESTService.ExecuteRESTPOSTDataAsync("", JsonConvert.SerializeObject(command), asODR, cancellationToken, logData);
            return asODR ? Task.FromResult("0") : result;
        }
    }
}
