using Honeywell.Firebird.CoreLibrary;
using System.Collections.Generic;

namespace BasePickingExample
{
    public class BasePickingExampleConfigRepository : ConfigRepository, IBasePickingExampleConfigRepository
    {
        private Dictionary<string, string> _DefaultValues;

        public BasePickingExampleConfigRepository(IConfigService configService) : base(configService)
        {
        }

        public override string ConfigCategoryName => "BasePickingExampleConfig";

        protected override Dictionary<string, string> DefaultValues => _DefaultValues ?? (_DefaultValues = new Dictionary<string, string>
        {
            {"SecureConnections", "true"},
            {"Host", "ddabb0ff-6c0c-4a4e-8d24-a767cc6f13e4.mock.pstmn.io"},
            {"Port", "443"},
        });
    }
}