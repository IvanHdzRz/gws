//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using System.Collections.Generic;
    using Honeywell.Firebird.CoreLibrary;

    public class LAppConfigRepository : ConfigRepository, ILAppConfigRepository
    {
        public static string OperatorId = "OperatorId";

        private Dictionary<string, string> _DefaultValues;

        public LAppConfigRepository(IConfigService configService) : base(configService)
        {
        }

        public string EmbeddedDemoValue => "EmbeddedDemo";
        public string ServerValue => "Server";

        public override string ConfigCategoryName => "LAppConfig";

        protected override Dictionary<string, string> DefaultValues => _DefaultValues ?? (_DefaultValues = new Dictionary<string, string>
        {
            {"Host", string.Empty},
            {"Port", string.Empty},
            {"CompanyDB", string.Empty},
            {OperatorId, string.Empty},
            {"Proxy", "false"},
            {"Secure", "false"},
            {"WorkflowFilterChoice", EmbeddedDemoValue}
        });
    }
}