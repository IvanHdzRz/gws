//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using RESTCommunication;
    using System.Threading;
    using System.Threading.Tasks;

    public class ReceivingRESTServiceProvider : IReceivingRESTServiceProvider
    {
        private readonly IRESTService _RESTService;

        public ReceivingRESTServiceProvider(IRESTService RESTService)
        {
            _RESTService = RESTService;
        }

        public Task<string> FetchReceivingDTOAsync(string siteId, string workerID, CancellationToken cancellationToken = default)
        {
            return _RESTService.ExecuteRESTGETAsync($"devicecomm/assignments/receiving/{siteId}/{workerID}", false, cancellationToken);
        }
    }
}
