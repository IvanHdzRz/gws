//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace SimpleApp
{
    using Honeywell.Firebird.CoreLibrary;
    using RESTCommunication;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Inspection REST service
    /// </summary>
    public class SimpleAppRESTService : RESTService, ISimpleAppRESTService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simpleAppRESTServicePropChangeManager">Prop change manager</param>
        /// <param name="RESTHeaderUtilities">Header utilities</param>
        /// <param name="restQueue">REST Queue</param>
        /// <param name="restTimeoutHandler">The timeout handler for the service.</param>
        public SimpleAppRESTService(ISimpleAppRESTServicePropChangeManager simpleAppRESTServicePropChangeManager,
            IRESTHeaderUtilities RESTHeaderUtilities,
            IRESTQueue restQueue,
            ITimeoutHandler restTimeoutHandler) :
            base(simpleAppRESTServicePropChangeManager, RESTHeaderUtilities, restQueue, restTimeoutHandler)
        {
            restQueue.LogFormatData = SimpleAppLogFormatData;
        }

        private string SimpleAppLogFormatData(string data, bool requestData)
        {
            var log_content = Regex.Replace(data, "\"password\": [^,]*,", "\"password\": ******,");
            log_content = Regex.Replace(log_content, "\"sessionId\": [^,]*,", "\"sessionId\": ******,");
            return log_content;
        }
    }
}
