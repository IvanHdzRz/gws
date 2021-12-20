//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Threading.Tasks;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;

    /// <summary>
    /// This class contains methods to make GET and POST requests to the server server. These
    /// methods can throw exceptions when network connectivity is poor that must be caught
    /// and handled at a higher level.
    /// </summary>
    public class ReceivingRESTDataTransport : WorkflowRESTDataTransport, IReceivingDataTransport
    {
        private readonly IReceivingRESTServiceProvider _RestServiceProvider;
        private readonly IServerConfigRepository _ServerConfigRepository;

        public ReceivingRESTDataTransport(IReceivingRESTServiceProvider restServiceProvider,
            IServerConfigRepository serverConfigRepository)
        {
            _RestServiceProvider = restServiceProvider;
            _ServerConfigRepository = serverConfigRepository;
        }

        /// <summary>
        /// Fetches a JSON-encoded string from the REST service that corresponds to a
        /// receiving assignment list for the currently selected worker.
        /// </summary>
        /// <returns>A Task to indicate the availabily of the JSON-encoded LTCDTO instance.</returns>
        public async Task<string> FetchReceivingDTOAsync()
        {
            return await _RestServiceProvider.FetchReceivingDTOAsync(_ServerConfigRepository.GetConfig("SiteId").Value, _ServerConfigRepository.GetConfig("WorkerID").Value);
        }
    }
}
