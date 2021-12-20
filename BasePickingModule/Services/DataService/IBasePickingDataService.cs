//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IBasePickingDataService
    {
        void Initialize();

        Task<bool> SignOnAsync(string operatorId, string password);

        Task SignOffAsync();

        Task<List<Pick>> GetPicksAsync();

        Task UpdatePickAsync(long pickId, int quantityPicked);
    }
}
