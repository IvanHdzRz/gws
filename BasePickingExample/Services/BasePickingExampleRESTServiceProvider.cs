using Common.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using System.Net;
using System.Net.Http;

namespace BasePickingExample
{
    /// <summary>
    /// BasePickingExample REST service provider
    /// </summary>
    public class BasePickingExampleRESTServiceProvider : IBasePickingExampleRESTServiceProvider
    {
        private readonly IBasePickingExampleRESTService _RESTService;

        private readonly ILog _Log = LogManager.GetLogger(nameof(BasePickingExampleRESTServiceProvider));

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="RESTService">REST service</param>
        public BasePickingExampleRESTServiceProvider(IBasePickingExampleRESTService RESTService)
        {
            _RESTService = RESTService;
        }

        public async Task<SignOnResponse> SignOnAsync(string operatorIdenitifier, string password, CancellationToken cancellationToken = default(CancellationToken))
        {
            var signOnReqData = new SignOnRequest
            {
                OperatorId = operatorIdenitifier,
                Password = password
            };
            var response = await _RESTService.ExecuteRESTPOSTDataAsync("/BasePickingExample/signon", JsonConvert.SerializeObject(signOnReqData), false, cancellationToken);

            return JsonConvert.DeserializeObject<SignOnResponse>(response);
        }
        public async Task<GetPicksResponse> GetPicksAsync(string operatorIdentifier, CancellationToken cancellationToken = default(CancellationToken))
        {
            var getPicksReqData = new GetPicksRequest
            {
                OperatorId = operatorIdentifier
            };
            var response = await _RESTService.ExecuteRESTPOSTDataAsync("/BasePickingExample/getpicks", JsonConvert.SerializeObject(getPicksReqData), false, cancellationToken);
            return JsonConvert.DeserializeObject<GetPicksResponse>(response);
        }
        // TODO: Add REST service implementations here
        public async Task<UpdatePickResponse> UpdatePickAsync(long pickId, int quantityPicked, CancellationToken cancellationToken = default(CancellationToken))
        {
            var updatePickRequest = new UpdatePickRequest
            {
                PickId = pickId,
                QuantityPicked = quantityPicked
            };
            var response = await _RESTService.ExecuteRESTPOSTDataAsync("/BasePickingExample/updatepick", JsonConvert.SerializeObject(updatePickRequest), false, cancellationToken);

            return JsonConvert.DeserializeObject<UpdatePickResponse>(response);
        }
        public async Task<SignOffResponse> SignOffAsync(string operatorIdenitifier, CancellationToken cancellationToken = default(CancellationToken))
        {
            var signOffReqData = new SignOffRequest
            {
                OperatorId = operatorIdenitifier
            };
            var response = await _RESTService.ExecuteRESTPOSTDataAsync("/BasePickingExample/signoff", JsonConvert.SerializeObject(signOffReqData), false, cancellationToken, responseHandler: SignOffResponseHandler);

            return JsonConvert.DeserializeObject<SignOffResponse>(response);
        }

        private Task<(bool, bool)> SignOffResponseHandler(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.RedirectMethod)
            {
                // Code 303 is expected when signing off but would result in automatic
                // retries if not handled with a custom handler.
                return Task.FromResult((true, false));
            }

            // Status is not 303 so don't handle (return false in the first part of the tuple)
            // and let the built in response handling occur.
            return Task.FromResult((false, false));
        }
        //Async Methods for Examen
        public async Task<GetContainerResponce> GetContainersById(string operatorId, string containerId, CancellationToken cancellationToken = default(CancellationToken)) {
            var getContainerRequest = new GetContainerRequest()
            {
                OperatorId = operatorId,
                ContainerId = containerId
            };
            var response = await _RESTService.ExecuteRESTPOSTDataAsync("/BasePickingExample/getcontainers", JsonConvert.SerializeObject(getContainerRequest), false, cancellationToken);

            return JsonConvert.DeserializeObject<GetContainerResponce>(response);
        }

        public async Task<OpenContainerResponce> OpenContainer(string assignId, string contId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var openContainerRequest = new OpenContainerRequest()
            {
                containerId=contId,
                assignmentId=assignId
            };
            var response = await _RESTService.ExecuteRESTPOSTDataAsync("/BasePickingExample/opencontainer", JsonConvert.SerializeObject(openContainerRequest), false, cancellationToken);

            return JsonConvert.DeserializeObject<OpenContainerResponce>(response);
        }
    }
}
