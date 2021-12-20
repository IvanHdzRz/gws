using Honeywell.Firebird.CoreLibrary;
using RESTCommunication;
using System;
using System.IO;

namespace NewExample
{
    /// <summary>
    /// NewExample property change manager
    /// </summary>
    public class NewExampleRESTServicePropChangeManager : INewExampleRESTServicePropChangeManager
    {
        // Dependencies
        readonly INewExampleConfigRepository _ConfigRepository;

        /// <summary>
        /// Handler for when connection properties change in the config repository
        /// </summary>
        public event EventHandler<ConnectionPropEventArgs> ConnectionPropChangedEvent;

        /// <summary>
        /// Handler for when the REST service is enabled or disabled in the config repository
        /// </summary>
        public event EventHandler<ServiceEnabledEventArgs> ServiceEnabledChangedEvent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configRepository">Config repository</param>
        /// <param name="dataPath">Path to store the queue file</param>
        public NewExampleRESTServicePropChangeManager(INewExampleConfigRepository configRepository,
                                                     IRESTDataPath dataPath)
        {
            _ConfigRepository = configRepository;
            QueuePath = Path.Combine(dataPath.Path, "NewExamplemodule_http_queue.bin");

            _ConfigRepository.ConfigChangedEvent += HandleServerConfigEvent;
        }

        /// <summary>
        /// Gets the queue path.
        /// </summary>
        /// <value>The queue path.</value>
        public string QueuePath { get; protected set; }

        /// <summary>
        /// Gets the base URL.
        /// </summary>
        /// <value>The base URL.</value>
        public string BaseUrl
        {
            get
            {
                string protocol;
                if (bool.Parse(_ConfigRepository.GetConfig("SecureConnections").Value))
                {
                    protocol = "https";
                }
                else
                {
                    protocol = "http";
                }
                var host = _ConfigRepository.GetConfig("Host").Value;
                var port = _ConfigRepository.GetConfig("Port").Value;

                // TODO: Change to be correct service URL
                return $"{protocol}://{host}:{port}/NewExampleServer/services/dummyService";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this
        /// <see cref="T:RESTCommunication.IRESTServicePropChangeManager"/> is
        /// enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled => true;

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

        #region IDisposable Support
        bool disposedValue; // To detect redundant calls

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _ConfigRepository.ConfigChangedEvent -= HandleServerConfigEvent;
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}