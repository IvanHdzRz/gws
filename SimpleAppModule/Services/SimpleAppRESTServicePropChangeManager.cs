//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace SimpleApp
{
    using System;
    using System.IO;
    using Honeywell.Firebird.CoreLibrary;
    using RESTCommunication;

    /// <summary>
    /// SimpleApp property change manager
    /// </summary>
    public class SimpleAppRESTServicePropChangeManager : ISimpleAppRESTServicePropChangeManager
    {
        // Dependencies
        readonly ISimpleAppConfigRepository _SimpleAppConfigRepository;

        /// <summary>
        /// Handler for when connection properties change in the config repository
        /// </summary>
        public event EventHandler<ConnectionPropEventArgs> ConnectionPropChangedEvent;

#pragma warning disable CS0067
        /// <summary>
        /// Handler for when the REST service is enabled or disabled in the config repository
        /// </summary>
        public event EventHandler<ServiceEnabledEventArgs> ServiceEnabledChangedEvent;
#pragma warning restore CS0067

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simpleAppConfigRepository">Config repository</param>
        /// <param name="dataPath">Path to store the queue file</param>
        public SimpleAppRESTServicePropChangeManager(ISimpleAppConfigRepository simpleAppConfigRepository,
                                                     IRESTDataPath dataPath)
        {
            _SimpleAppConfigRepository = simpleAppConfigRepository;
            QueuePath = Path.Combine(dataPath.Path, "simpleappmodule_http_queue.bin");

            _SimpleAppConfigRepository.ConfigChangedEvent += HandleServerConfigEvent;
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
                string protocol = "http";
                var host = _SimpleAppConfigRepository.GetConfig(SimpleAppConfigRepository.HOST).Value;
                var port = _SimpleAppConfigRepository.GetConfig(SimpleAppConfigRepository.PORT).Value;

                return $"{protocol}://{host}:{port}/SimpleAppServer/services/dummyService";
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
                    _SimpleAppConfigRepository.ConfigChangedEvent -= HandleServerConfigEvent;
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
