//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using System.Threading.Tasks;
    using GuidedWorkRunner;

    public interface IVoiceLinkDataService
    {
        void Initialize();

        Task<VLConfigLUT> SendConfigAsync();

        Task<BreakTypeLUT> GetBreakTypesAsync();

        Task<SignOnLUT> SignOnAsync(Operator oper);

        Task<FunctionLUT> GetValidFunctionsAsync(int taskId);

        Task<RegionLUT> GetRegionPermissionsForWorkTypeAsync(int workType);

        Task<PickingRegionLUT> GetPickingRegionForWorkTypeAsync(string pickingRegion, int workType);

        Task<AssignmentLUT> GetAssignmentAsync(int numberOfAssignments, int assignmentType);

        Task<GenericResponseLUT> UpdateStatusAsync(long? groupId, long? locationId, string slotAisle, string setStatusTo, int useLuts);

        Task<ContainerLUT> GetContainersAsync(Assignment assignment, string targetContainer, long? pickContainerId, string containerNumber, int operation, string labels);

        Task<PickLUT> GetPicksAsync(long? groupId, bool shortsAndSkipsFlag, int goBackForSkipsIndicator, int pickOrderFlag);

        Task<VerifyReplenishmentLUT> VerifyReplenishmentAsync(long locationId, string itemNumber);

        Task<GenericResponseLUT> ExecutePickAsync(Assignment assignment, long locationId, int quantityPicked, bool endOfPartialPickingFlag, long? containerId, long pickId, string lotNumber, double? variableWeight, string itemSerialNumer, int useLuts);

        Task<GenericResponseLUT> PassAssignmentAsync(long? groupId);

        Task<GenericResponseLUT> StopAssignmentAsync(long? groupId);

        Task<GenericResponseLUT> SignOffAsync();

        Task<RequestWorkLUT> GetRequestWorkAsync(string workId, int scanned, int assignmentType);
    }
}
