//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System;
    using System.IO;
    using System.Linq;
    using Honeywell.Firebird.CoreLibrary;
    using RESTCommunication;

    public class LAppRESTServicePropChangeManager : ILAppRESTServicePropChangeManager
    {
        private readonly ILAppConfigRepository _ConfigRepository;

        public event EventHandler<ConnectionPropEventArgs> ConnectionPropChangedEvent;
        public event EventHandler<ServiceEnabledEventArgs> ServiceEnabledChangedEvent;

        public LAppRESTServicePropChangeManager(ILAppConfigRepository configRepository,
            IRESTDataPath dataPath)
        {
            _ConfigRepository = configRepository;
            QueuePath = Path.Combine(dataPath.Path, "LAppmodule_http_queue.bin");

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
                bool secure = bool.Parse(_ConfigRepository.GetConfig("Secure").Value);
                string scheme = secure ? "https" : "http";
                if (string.IsNullOrEmpty(port))
                {
                    port = secure ? "443" : "80";
                }
                return $"{scheme}://{host}:{port}/API/Demo/";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this
        /// <see cref="T:RESTCommunication.IRESTServicePropChangeManager"/> is
        /// enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled => _ConfigRepository.GetConfig("WorkflowFilterChoice").Value == _ConfigRepository.ServerValue;

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

        private void HandleWorkflowFilterConfigEvent(object sender, ConfigEventArgs e)
        {
            // If workflow filter changed, cancel or start persisted request task as needed
            var workflowFilter = e.Configs.FirstOrDefault((config) => config.Key == "WorkflowFilterChoice");
            if (workflowFilter != null)
            {
                if (workflowFilter.Value == _ConfigRepository.EmbeddedDemoValue)
                {
                    ServiceEnabledChangedEvent?.Invoke(this, new ServiceEnabledEventArgs(false));
                }
                else if (workflowFilter.Value == _ConfigRepository.ServerValue)
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
