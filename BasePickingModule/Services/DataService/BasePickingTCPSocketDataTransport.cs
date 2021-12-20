//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System.Threading.Tasks;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;

    public class BasePickingTCPSocketDataTransport : WorkflowTCPSocketDataTransport, IBasePickingDataTransport
    {
        private readonly IBasePickingTCPSocketServiceProvider _TCPSocketServiceProvider;
        private readonly IBasePickingConfigRepository _BasePickingConfigRepository;
        private readonly ITimeoutHandler _TCPTimeoutHandler;

        public BasePickingTCPSocketDataTransport(IBasePickingTCPSocketServiceProvider tcpSocketServiceProvider,
                                                 IBasePickingConfigRepository basePickingConfigRepository,
                                                 ITimeoutHandler tcpTimeoutHandler)
        {
            _TCPSocketServiceProvider = tcpSocketServiceProvider;
            _BasePickingConfigRepository = basePickingConfigRepository;
            _TCPTimeoutHandler = tcpTimeoutHandler;
        }

        public void Initialize()
        {
        }

        public async Task<string> SignOnAsync(string operatorId, string password)
        {
            return await _TCPSocketServiceProvider.SignOnAsync(operatorId, password, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> SignOffAsync()
        {
            return await _TCPSocketServiceProvider.SignOffAsync(_BasePickingConfigRepository.GetConfig("OperIdent").Value, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> GetPicksAsync()
        {
            return await _TCPSocketServiceProvider.GetPicksAsync(_TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> UpdatePickAsync(long pickId, int quantityPicked)
        {
            return await _TCPSocketServiceProvider.UpdatePickAsync(pickId, quantityPicked, _TCPTimeoutHandler.GetTimeoutToken());
        }
    }
}
