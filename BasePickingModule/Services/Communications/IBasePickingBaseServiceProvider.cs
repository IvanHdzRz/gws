//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IBasePickingBaseServiceProvider
    {
        Task<string> SignOnAsync(string operatorIdenitifier, string password, CancellationToken cancellationToken = default(CancellationToken));

        Task<string> SignOffAsync(string operatorIdentifier, CancellationToken cancellationToken = default(CancellationToken));

        Task<string> GetPicksAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<string> UpdatePickAsync(long pickId, int quantityPicked, CancellationToken cancellationToken = default(CancellationToken));
    }
}
