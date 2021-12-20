using Honeywell.Firebird.CoreLibrary;
using System.Collections.Generic;

namespace NewExample
{
    public class NewExampleConfigRepository : ConfigRepository, INewExampleConfigRepository
    {
        private Dictionary<string, string> _DefaultValues;

        public NewExampleConfigRepository(IConfigService configService) : base(configService)
        {
        }

        public override string ConfigCategoryName => "NewExampleConfig";

        protected override Dictionary<string, string> DefaultValues => _DefaultValues ?? (_DefaultValues = new Dictionary<string, string>
        {
            {"SecureConnections", "true"},
            {"Host", string.Empty},
            {"Port", "80"},
        });
    }
}