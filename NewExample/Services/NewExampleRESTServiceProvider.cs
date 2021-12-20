using Common.Logging;
using System.Threading.Tasks;

namespace NewExample
{
    /// <summary>
    /// NewExample REST service provider
    /// </summary>
    public class NewExampleRESTServiceProvider : INewExampleRESTServiceProvider
    {
        private readonly INewExampleRESTService _RESTService;

        private readonly ILog _Log = LogManager.GetLogger(nameof(NewExampleRESTServiceProvider));

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="RESTService">REST service</param>
        public NewExampleRESTServiceProvider(INewExampleRESTService RESTService)
        {
            _RESTService = RESTService;
        }

        // TODO: Add REST service implementations here
    }
}
