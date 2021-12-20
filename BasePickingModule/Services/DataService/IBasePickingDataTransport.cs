//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System.Threading.Tasks;

    // Extend the IWorkflowDataTransport to include the opertions required of the BasePicking module.
    public interface IBasePickingDataTransport 
    {
        string Name { get; }

        void Initialize();

        Task<string> SignOnAsync(string operatorId, string password);

        Task<string> SignOffAsync();

        Task<string> GetPicksAsync();

        Task<string> UpdatePickAsync(long pickId, int quantityPicked);
    }
}
