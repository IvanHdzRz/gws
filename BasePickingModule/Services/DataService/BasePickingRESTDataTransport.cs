//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System.Threading.Tasks;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;

    /// <summary>
    /// This class contains methods to make GET and POST requests to the server server. These
    /// methods can throw exceptions when network connectivity is poor that must be caught
    /// and handled at a higher level.
    /// </summary>
    public class BasePickingRESTDataTransport : WorkflowRESTDataTransport, IBasePickingDataTransport
    {
        private readonly IBasePickingRESTServiceProvider _RestServiceProvider;
        private readonly IBasePickingConfigRepository _BasePickingConfigRepository;
        private readonly ITimeoutHandler _RESTTimeoutHandler;

        public BasePickingRESTDataTransport(IBasePickingRESTServiceProvider restServiceProvider,
                                          IBasePickingConfigRepository basePickingConfigRepository,
                                          ITimeoutHandler restTimeoutHandler)
        {
            _RestServiceProvider = restServiceProvider;
            _BasePickingConfigRepository = basePickingConfigRepository;
            _RESTTimeoutHandler = restTimeoutHandler;
        }

        public void Initialize()
        {
        }

        public async Task<string> SignOnAsync(string operatorId, string password)
        {
            return await _RestServiceProvider.SignOnAsync(operatorId, password, _RESTTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> SignOffAsync()
        {
            return await _RestServiceProvider.SignOffAsync(_BasePickingConfigRepository.GetConfig("OperIdent").Value, _RESTTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> GetPicksAsync()
        {
            return await _RestServiceProvider.GetPicksAsync(_RESTTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> UpdatePickAsync(long pickId, int quantityPicked)
        {
            return await _RestServiceProvider.UpdatePickAsync(pickId, quantityPicked, _RESTTimeoutHandler.GetTimeoutToken());
        }
    }
}
