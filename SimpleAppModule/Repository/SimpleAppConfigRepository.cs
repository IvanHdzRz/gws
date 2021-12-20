//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace SimpleApp
{
    using System.Collections.Generic;
    using Honeywell.Firebird.CoreLibrary;

    public class SimpleAppConfigRepository : ConfigRepository, ISimpleAppConfigRepository
    {
        public const string HOST = "Host";
        public const string PORT = "Port";
        public const string OPERATOR_ID = "OperatorID";

        public override string ConfigCategoryName => "SimpleAppConfig";

        private Dictionary<string, string> _DefaultValues;

        protected override Dictionary<string, string> DefaultValues
        {
            get
            {
                return _DefaultValues ?? (_DefaultValues = new Dictionary<string, string>
                    {
                        {HOST, ""},
                        {PORT, "9090"},
                    });
            }
        }

        public SimpleAppConfigRepository(IConfigService configService) : base(configService)
        {
        }
    }
}

