//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System.Collections.Generic;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;


    public static class BasePickingPickMethod
    {
        public static string Discrete = "Discrete";
        public static string Cluster = "Cluster";
    }

    public class BasePickingConfigRepository : ConfigRepository, IBasePickingConfigRepository
    {
        private Dictionary<string, string> _DefaultValues;

        public BasePickingConfigRepository(IConfigService configService) : base(configService)
        {
        }

        public override string ConfigCategoryName => "BasePickingConfig";

        protected override Dictionary<string, string> DefaultValues => _DefaultValues ?? (_DefaultValues = new Dictionary<string, string>
        {
            {"Host", "192.168.1.1"},
            {"Port", "8080" },
            {"ODRPort", "15001" },
            {"SecureConnections", "true"},
            {"WorkflowFilterChoice", LocalizationHelper.EmbeddedDemoLocalizationKey},
            {"PickQuantityCountdown", "true"},
            {"PickMethod", BasePickingPickMethod.Discrete },
            {"ConfirmLocation", "true"},
            {"ConfirmProduct", "true"},
            {"ConfirmQuantityVoiceInput", "true"},
            {"ConfirmQuantityScreenInput", "false"},
            {"ShowConfigurationSettings", "true"},
            {"ShowHints", "true" }
        });
    }
}
