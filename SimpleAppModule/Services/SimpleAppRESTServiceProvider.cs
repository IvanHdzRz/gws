//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace SimpleApp
{
    using Common.Logging;
    using GuidedWorkRunner;
    using Newtonsoft.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// SimpleApp REST service provider
    /// </summary>
    public class SimpleAppRESTServiceProvider : ISimpleAppRESTServiceProvider
    {
        private readonly ISimpleAppRESTService _RESTService;

        private readonly ILog _Log = LogManager.GetLogger(nameof(SimpleAppRESTServiceProvider));

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="RESTService">REST service</param>
        public SimpleAppRESTServiceProvider(ISimpleAppRESTService RESTService)
        {
            _RESTService = RESTService;
        }

        public async Task<SimpleAppResponse> SignOnAsync(Operator newOperator)
        {
            // This type of line would be used if communicating with a real server
            //var responseString = await _RESTService.ExecuteRESTPOSTDataAsync("SignOnURLExtension", JsonConvert.SerializeObject(requestObject), false);

            // This is just test code since there is no server to communicate with
            var testResponse = new SimpleAppResponse();
            if (newOperator.Password.ToLower().Trim() == "fail")
            {
                testResponse.ErrorCode = 1;
            }
            var responseString = JsonConvert.SerializeObject(testResponse);
            await Task.CompletedTask;
            // End test code

            return JsonConvert.DeserializeObject<SimpleAppResponse>(responseString);
        }

        public async Task<SimpleAppResponse> DoSimpleAppRequestAsync(string someData)
        {
            var requestObject = new SimpleAppRequest
            {
                RequestData = someData
            };

            // This type of line would be used if communicating with a real server
            //var responseString = await _RESTService.ExecuteRESTPOSTDataAsync("SomeSpecificRESTFunctionURLExtension", JsonConvert.SerializeObject(requestObject), false);

            // This is just test code since there is no server to communicate with
            var testResponse = new SimpleAppResponse
            {
                ResponseData = requestObject.RequestData + "_Processed"
            };
            var responseString = JsonConvert.SerializeObject(testResponse);
            await Task.Delay(2000);
            // End test code

            return JsonConvert.DeserializeObject<SimpleAppResponse>(responseString);
        }
    }

    public class SimpleAppRequest
    {
        /// <summary>
        /// Requestion data
        /// </summary>
        [JsonProperty("SomeRandomRequestData")]
        public string RequestData { get; set; }
    }

    public class SimpleAppResponse
    {
        /// <summary>
        /// Error code
        /// </summary>
        public int ErrorCode { get; set; } = 0;

        /// <summary>
        /// Response data
        /// </summary>
        [JsonProperty("SomeRandomResponseData")]
        public string ResponseData { get; set; }
    }
}
