//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Logging;
    using GuidedWork;
    using Newtonsoft.Json;

    public class BasePickingDataService : IBasePickingDataService
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(BasePickingDataService));
        private readonly IBasePickingDataProxy _DataProxy;
        private readonly IBasePickingConfigRepository _BasePickingConfigRepository;

        public BasePickingDataService(IBasePickingDataProxy dataProxy, IBasePickingConfigRepository basePickingConfigRepository)
        {
            _DataProxy = dataProxy;
            _BasePickingConfigRepository = basePickingConfigRepository;
        }

        public void Initialize()
        {
            if (_DataProxy.DataTransport == null)
            {
                SelectTransport();
            }
            _DataProxy.DataTransport.Initialize();
        }

        #region IBasePickingDataService Implemenation

        public async Task<bool> SignOnAsync(string operatorId, string password)
        {
            string response = await _DataProxy.DataTransport.SignOnAsync(operatorId, password);
            // TODO: Determine success of sign on
            return true;
        }

        public async Task SignOffAsync()
        {
            string response = await _DataProxy.DataTransport.SignOffAsync();
        }

        public async Task<List<Pick>> GetPicksAsync()
        {
            string response = await _DataProxy.DataTransport.GetPicksAsync();
            return JsonConvert.DeserializeObject<List<Pick>>(response);
        }

        public async Task UpdatePickAsync(long pickId, int quantityPicked)
        {
            string response = await _DataProxy.DataTransport.UpdatePickAsync(pickId, quantityPicked);
        }

        #endregion

        private void SelectTransport()
        {
            if (_BasePickingConfigRepository.GetConfig("WorkflowFilterChoice")?.Value == LocalizationHelper.ServerLocalizationKey)
            {
                _DataProxy.SelectTransport("RESTDataTransport");
            }
            else if (_BasePickingConfigRepository.GetConfig("WorkflowFilterChoice")?.Value == LocalizationHelper.LegacySocketServerLocalizationKey)
            {
                _DataProxy.SelectTransport("TCPSocketDataTransport");
            }
            else
            {
                _DataProxy.SelectTransport("FileDataTransport");
            }
        }
    }
}
