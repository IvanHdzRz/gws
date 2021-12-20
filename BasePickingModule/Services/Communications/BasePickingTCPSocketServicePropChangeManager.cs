//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System;
    using System.IO;
    using System.Linq;
    using Honeywell.Firebird.CoreLibrary;
    using TCPSocketCommunication;
    using GuidedWork;

    public class BasePickingTCPSocketServicePropChangeManager : IBasePickingTCPSocketServicePropChangeManager
    {
        // Dependencies
        readonly IBasePickingConfigRepository _ConfigRepository;

        public event EventHandler<ConnectionPropEventArgs> ConnectionPropChangedEvent;
        public event EventHandler<ServiceEnabledEventArgs> ServiceEnabledChangedEvent;

        public BasePickingTCPSocketServicePropChangeManager(IBasePickingConfigRepository configRepository,
                                                          IDataPath dataPath)
        {
            _ConfigRepository = configRepository;
            QueuePath = Path.Combine(dataPath.ExternalPath, "basePickingmodule_tcp_queue.bin");

            _ConfigRepository.ConfigChangedEvent += HandleServerConfigEvent;
            _ConfigRepository.ConfigChangedEvent += HandleWorkflowFilterConfigEvent;
        }

        public string QueuePath { get; }

        public string Host { get { return _ConfigRepository.GetConfig("Host").Value; } }
        public int LUTPort {
            get
            {
                int.TryParse(_ConfigRepository.GetConfig("Port").Value, out int port);
                return port;
            }
        }
        public int ODRPort
        {
            get
            {
                int.TryParse(_ConfigRepository.GetConfig("ODRPort").Value, out int port);
                return port;
            }
        }

        public bool UseSecureConnection
        {
            get
            {
                bool.TryParse(_ConfigRepository.GetConfig("SecureConnections").Value, out bool secureConnection);
                return secureConnection;
            }
        }

        public bool Enabled => _ConfigRepository.GetConfig("WorkflowFilterChoice").Value ==
                               LocalizationHelper.LegacySocketServerLocalizationKey;

        public bool AuthenticationEnabled => false;

        public string AuthenticationLoginUri => throw new NotImplementedException();

        void HandleServerConfigEvent(object sender, ConfigEventArgs e)
        {
            ConnectionPropChangedEvent?.Invoke(this, new ConnectionPropEventArgs(Host, LUTPort, ODRPort, useSecureConnection: UseSecureConnection));
        }

        void HandleWorkflowFilterConfigEvent(object sender, ConfigEventArgs e)
        {
            // If workflow filter changed, cancel or start persisted request task as needed
            var workflowFilter = e.Configs.FirstOrDefault((config) => config.Key == "WorkflowFilterChoice");
            if (workflowFilter != null)
            {
                if (workflowFilter.Value == LocalizationHelper.EmbeddedDemoLocalizationKey)
                {
                    ServiceEnabledChangedEvent?.Invoke(this, new ServiceEnabledEventArgs(false));
                }
                else if (workflowFilter.Value == LocalizationHelper.ServerLocalizationKey)
                {
                    ServiceEnabledChangedEvent?.Invoke(this, new ServiceEnabledEventArgs(false));
                }
                else if (workflowFilter.Value == LocalizationHelper.LegacySocketServerLocalizationKey)
                {
                    ServiceEnabledChangedEvent?.Invoke(this, new ServiceEnabledEventArgs(true));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        #region IDisposable Support
        bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _ConfigRepository.ConfigChangedEvent -= HandleServerConfigEvent;
                    _ConfigRepository.ConfigChangedEvent -= HandleWorkflowFilterConfigEvent;
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
