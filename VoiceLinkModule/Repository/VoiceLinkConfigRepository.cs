//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using System.Collections.Generic;
    using Honeywell.Firebird.CoreLibrary;

    public class VoiceLinkConfigRepository : ConfigRepository, IVoiceLinkConfigRepository
    {
        public override string ConfigCategoryName => "VoiceLinkConfig";

        private Dictionary<string, string> _DefaultValues;

        protected override Dictionary<string, string> DefaultValues
        {
            get
            {
                return _DefaultValues ?? (_DefaultValues = new Dictionary<string, string>
                    {
                        {"WorkflowFilterChoice", "Server"},
                        {"ODRPort", "80"},
                        {"Host", ""},
                        {"Port", "9443"},
                        {"SiteName", "Default"},
                        {"SecureConnections", "true"}

                    });
            }
        }

        public VoiceLinkConfigRepository(IConfigService configService) : base(configService)
        {
        }
    }
}

