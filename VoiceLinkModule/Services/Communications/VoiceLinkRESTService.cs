//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using Honeywell.Firebird.CoreLibrary;
    using RESTCommunication;

    public class VoiceLinkRESTService : RESTService, IVoiceLinkRESTService
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="voiceLinkRESTServicePropChangeManager">Property change manager</param>
        /// <param name="RESTHeaderUtilities">REST header utilities</param>
        /// <param name="restQueue">REST queue</param>
        public VoiceLinkRESTService(IVoiceLinkRESTServicePropChangeManager voiceLinkRESTServicePropChangeManager,
            IRESTHeaderUtilities RESTHeaderUtilities,
            IRESTQueue restQueue,
            ITimeoutHandler restTimeoutHandler) :
            base(voiceLinkRESTServicePropChangeManager, RESTHeaderUtilities, restQueue, restTimeoutHandler)
        {
        }

        /// <summary>
        /// Constructor for when running on VoiceCatalyst Device that includes prohibitor to prevent
        /// accidental task loads and operator changes while there is still data queued 
        /// up to be sent.
        /// </summary>
        /// <param name="voiceLinkRESTServicePropChangeManager">Property change manager</param>
        /// <param name="RESTHeaderUtilities">REST header utilities</param>
        /// <param name="restQueue">REST queue</param>
        /// <param name="prohibitor">Prohibitor for when running on VoiceCatalystDevice</param>
        public VoiceLinkRESTService(IVoiceLinkRESTServicePropChangeManager voiceLinkRESTServicePropChangeManager,
            IRESTHeaderUtilities RESTHeaderUtilities,
            IRESTQueue restQueue, IVoiceCatalystProhibitor prohibitor,
            ITimeoutHandler restTimeoutHandler) :
            base(voiceLinkRESTServicePropChangeManager, RESTHeaderUtilities, restQueue, restTimeoutHandler)
        {
            //Register prohibitor, and call acquire with initial number of items that were
            //loaded when queue was opened.
            Prohibitor = prohibitor;
            Prohibitor.ProhibitAcquire((int)restQueue.Count);
        }
    }
}
