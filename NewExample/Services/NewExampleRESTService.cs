using Honeywell.Firebird.CoreLibrary;
using RESTCommunication;
using System.Text.RegularExpressions;

namespace NewExample
{
    /// <summary>
    /// Inspection REST service
    /// </summary>
    public class NewExampleRESTService : RESTService, INewExampleRESTService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="RESTServicePropChangeManager">Prop change manager</param>
        /// <param name="RESTHeaderUtilities">Header utilities</param>
        /// <param name="restQueue">REST Queue</param>
        /// <param name="restTimeoutHandler">The timeout handler for the service.</param>
        public NewExampleRESTService(INewExampleRESTServicePropChangeManager RESTServicePropChangeManager,
            IRESTHeaderUtilities RESTHeaderUtilities,
            IRESTQueue restQueue,
            ITimeoutHandler restTimeoutHandler) :
            base(RESTServicePropChangeManager, RESTHeaderUtilities, restQueue, restTimeoutHandler)
        {
            restQueue.LogFormatData = NewExampleLogFormatData;
        }

        private string NewExampleLogFormatData(string data, bool requestData)
        {
            var log_content = Regex.Replace(data, "\"password\": [^,]*,", "\"password\": ******,");

            // Replace text that appears in post requests that shouldn't appear in logs

            return log_content;
        }
    }
}
