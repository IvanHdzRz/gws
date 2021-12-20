//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IReceivingRESTServiceProvider
    {
        /// <summary>
        /// Fetches a JSON-encoded string that corresponds to a receiving assignment list for a
        /// specified worker Id.
        /// </summary>
        /// <param name="siteId">The site Id.</param>
        /// <param name="workerId">A worker Id.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task to indicate the availabily of the JSON-encoded OrdersDTO instance.</returns>
        Task<string> FetchReceivingDTOAsync(string siteId, string workerId, CancellationToken cancellationToken = default);
    }
}
