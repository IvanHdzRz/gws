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
    using RESTCommunication;

    public class VoiceLinkRESTServicePropChangeManager : IVoiceLinkRESTServicePropChangeManager
    {
        // Dependencies
        readonly IVoiceLinkConfigRepository _VoiceLinkConfigRepository;

        // Constants
        const string _ServicePath = "/VoiceLink/services/proxyRestService/executeCommand";

        public event EventHandler<ConnectionPropEventArgs> ConnectionPropChangedEvent;
        public event EventHandler<ServiceEnabledEventArgs> ServiceEnabledChangedEvent;

        public VoiceLinkRESTServicePropChangeManager(IVoiceLinkConfigRepository voiceLinkConfigRepository,
                                                     IRESTDataPath dataPath)
        {
            _VoiceLinkConfigRepository = voiceLinkConfigRepository;
            QueuePath = Path.Combine(dataPath.Path, "voicelinkmodule_http_queue.bin");

            _VoiceLinkConfigRepository.ConfigChangedEvent += HandleServerConfigEvent;
            _VoiceLinkConfigRepository.ConfigChangedEvent += HandleWorkflowFilterConfigEvent;
        }

        /// <summary>
        /// Gets the queue path.
        /// </summary>
        /// <value>The queue path.</value>
        public string QueuePath { get; }

        /// <summary>
        /// Gets the base URL.
        /// </summary>
        /// <value>The base URL.</value>
        public string BaseUrl
        {
            get
            {
                string protocol;
                if (bool.Parse(_VoiceLinkConfigRepository.GetConfig("SecureConnections").Value))
                {
                    protocol = "https";
                }
                else
                {
                    protocol = "http";
                }
                var host = _VoiceLinkConfigRepository.GetConfig("Host").Value;
                var port = _VoiceLinkConfigRepository.GetConfig("Port").Value;

                return $"{protocol}://{host}:{port}{_ServicePath}";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this
        /// <see cref="T:RESTCommunication.IRESTServicePropChangeManager"/> is
        /// enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled => _VoiceLinkConfigRepository.GetConfig("WorkflowFilterChoice").Value ==
                               LocalizationHelper.ServerLocalizationKey;

        /// <summary>
        /// Gets a value indicating whether this
        /// <see cref="T:RESTCommunication.IRESTServicePropChangeManager"/>
        /// has authentication enabled.
        /// </summary>
        /// <value><c>true</c> if authentication enabled; otherwise, <c>false</c>.</value>
        public bool AuthenticationEnabled => false;

        /// <summary>
        /// Gets the authentication login URI.
        /// </summary>
        /// <value>The authentication login URI.</value>
        public string AuthenticationLoginUri => throw new NotImplementedException();

        void HandleServerConfigEvent(object sender, ConfigEventArgs e)
        {
            ConnectionPropChangedEvent?.Invoke(this, new ConnectionPropEventArgs(BaseUrl));
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
