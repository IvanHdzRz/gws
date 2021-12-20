//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using System;
    using System.IO;
    using System.Linq;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;
    using TCPSocketCommunication;

    public class VoiceLinkTCPSocketServicePropChangeManager : IVoiceLinkTCPSocketServicePropChangeManager
    {
        // Dependencies
        readonly IVoiceLinkConfigRepository _VoiceLinkConfigRepository;

        public event EventHandler<ConnectionPropEventArgs> ConnectionPropChangedEvent;
        public event EventHandler<ServiceEnabledEventArgs> ServiceEnabledChangedEvent;

        public VoiceLinkTCPSocketServicePropChangeManager(IVoiceLinkConfigRepository voiceLinkConfigRepository,
                                                          IDataPath dataPath)
        {
            _VoiceLinkConfigRepository = voiceLinkConfigRepository;
            QueuePath = Path.Combine(dataPath.ExternalPath, "voicelinkmodule_tcp_queue.bin");

            _VoiceLinkConfigRepository.ConfigChangedEvent += HandleServerConfigEvent;
            _VoiceLinkConfigRepository.ConfigChangedEvent += HandleWorkflowFilterConfigEvent;
        }

        public string QueuePath { get; }

        public string Host { get { return _VoiceLinkConfigRepository.GetConfig("Host").Value; } }
        public int LUTPort { get { return Int32.TryParse(_VoiceLinkConfigRepository.GetConfig("Port").Value, out int value) ? value : 0; } }
        public int ODRPort { get { return Int32.TryParse(_VoiceLinkConfigRepository.GetConfig("ODRPort").Value, out int value) ? value : 0; } }
        public bool UseSecureConnection { get { return bool.Parse(_VoiceLinkConfigRepository.GetConfig("SecureConnections").Value); } }


        public bool Enabled => _VoiceLinkConfigRepository.GetConfig("WorkflowFilterChoice").Value ==
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
                    ServiceEnabledChangedEvent?.Invoke(this, new ServiceEnabledEventArgs(true));
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
                    _VoiceLinkConfigRepository.ConfigChangedEvent -= HandleServerConfigEvent;
                    _VoiceLinkConfigRepository.ConfigChangedEvent -= HandleWorkflowFilterConfigEvent;
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
