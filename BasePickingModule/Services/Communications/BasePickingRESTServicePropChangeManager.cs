//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System;
    using System.IO;
    using System.Linq;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;
    using RESTCommunication;

    public class BasePickingRESTServicePropChangeManager : IBasePickingRESTServicePropChangeManager
    {
        private readonly IBasePickingConfigRepository _ConfigRepository;

        public event EventHandler<ConnectionPropEventArgs> ConnectionPropChangedEvent;
        public event EventHandler<ServiceEnabledEventArgs> ServiceEnabledChangedEvent;

        public BasePickingRESTServicePropChangeManager(IBasePickingConfigRepository configRepository,
            IRESTDataPath dataPath)
        {
            _ConfigRepository = configRepository;
            QueuePath = Path.Combine(dataPath.Path, "BasePickingmodule_http_queue.bin");

            _ConfigRepository.ConfigChangedEvent += HandleServerConfigEvent;
            _ConfigRepository.ConfigChangedEvent += HandleWorkflowFilterConfigEvent;
        }

        /// <summary>
        /// Gets the base URL.
        /// </summary>
        /// <value>The base URL.</value>
        public string BaseUrl
        {
            get
            {
                string host = _ConfigRepository.GetConfig("Host").Value;
                string port = _ConfigRepository.GetConfig("Port").Value;
                bool.TryParse(_ConfigRepository.GetConfig("SecureConnections").Value, out bool secure);
                string protocol = secure ? "https" : "http";
                return $"{protocol}://{host}:{port}/API/";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this
        /// <see cref="T:RESTCommunication.IRESTServicePropChangeManager"/> is
        /// enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled => _ConfigRepository.GetConfig("WorkflowFilterChoice").Value == LocalizationHelper.ServerLocalizationKey;

        /// <summary>
        /// Gets the queue path.
        /// </summary>
        /// <value>The queue path.</value>
        public string QueuePath { get; }

        /// <summary>
        /// Gets a value indicating whether this
        /// <see cref="T:RESTCommunication.IRESTServicePropChangeManager"/>
        /// has authentication enabled.
        /// </summary>
        /// <value><c>true</c> if authentication enabled; otherwise, <c>false</c>.</value>
        public bool AuthenticationEnabled => true;

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
                    ServiceEnabledChangedEvent?.Invoke(this, new ServiceEnabledEventArgs(false));
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
