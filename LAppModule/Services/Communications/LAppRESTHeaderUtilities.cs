//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Logging;
    using Newtonsoft.Json;
    using RESTCommunication;

    /// <summary>
    /// A collection of properties and methods that is used to modify REST
    /// requests for authentication.
    /// </summary>
    public class LAppRESTHeaderUtilities : ILAppRESTHeaderUtilities
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(LAppRESTHeaderUtilities));
        private readonly ILAppConfigRepository _LAppConfigRepository;
        private string _CompanyDB => _LAppConfigRepository.GetConfig("CompanyDB").Value;

        /// <summary>
        /// The property change manager used to retrieve properties. This property must be set
        /// prior to making RESTUtilities.
        /// </summary>
        public IRESTServicePropChangeManager RESTServicePropChangeManager { get; set; }

        public LAppRESTHeaderUtilities(ILAppConfigRepository lappConfigRepository)
        {
            _LAppConfigRepository = lappConfigRepository;
        }

        /// <summary>
        /// Handles a REST response asynchronously.  Does nothing in this
        /// implementation.
        /// </summary>
        /// <returns>A Task that completes when the response has been
        /// handled.</returns>
        /// <param name="response">Response.</param>
        /// <param name="httpClient">Http client.</param>
        /// <param name="cancellationToken">A Cancellation token.</param>
        public Task<bool> HandleResponseAsync(HttpResponseMessage response, HttpClient httpClient, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Adds headers to a REST request asynchronously.
        /// </summary>
        /// <returns>A Task that completes when the headers have been
        /// added to the request.</returns>
        /// <param name="request">The request to add headers to.</param>
        /// <param name="httpClient">The Http client for the request.</param>
        /// <param name="cancellationToken">A Cancellation token.</param>
        public Task AddHeadersAsync(HttpRequestMessage request, HttpClient httpClient, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void ResetCredentials()
        {
        }

        public async Task<bool> ValidateCredentialsAsync(string userName, string password)
        {
            var login = new Login { User = userName, Password = password, CompanyName = _CompanyDB };
            string loginString = JsonConvert.SerializeObject(login);

            string relativeURL = "Login";

            var loginContent = new StringContent(loginString, Encoding.UTF8, "application/json");

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(RESTServicePropChangeManager.BaseUrl);
                    var request = new HttpRequestMessage(HttpMethod.Post, new Uri(relativeURL, UriKind.Relative))
                    {
                        Content = loginContent
                    };

                    var response = await client.SendAsync(request).ConfigureAwait(false);
                    var loginResponseString = await response.Content.ReadAsStringAsync();

                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (Exception e)
            {
                _Log.Error(m => m("Error http request: {0}{1}{2}", e.Message, Environment.NewLine, e.StackTrace));
            }

            return false;
        }
    }
}
