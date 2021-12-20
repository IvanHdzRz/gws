//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System.Threading;
    using System.Threading.Tasks;

    public class BasePickingRESTServiceProvider : IBasePickingRESTServiceProvider
    {
        private readonly IBasePickingRESTService _RESTService;

        public BasePickingRESTServiceProvider(IBasePickingRESTService RESTService)
        {
            _RESTService = RESTService;
        }

        public Task<string> GetPicksAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        public Task<string> SignOffAsync(string operatorIdentifier, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        public Task<string> SignOnAsync(string operatorIdenitifier, string password, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        public Task<string> UpdatePickAsync(long pickId, int quantityPicked, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }
    }
}
