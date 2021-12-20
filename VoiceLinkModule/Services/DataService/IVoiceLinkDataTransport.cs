//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using System.Threading.Tasks;
    using GuidedWorkRunner;

    public interface IVoiceLinkDataTransport
    {
        string Name { get; }

        void Initialize();

        Task<string> SendConfigAsync();

        Task<string> GetBreakTypesAsync();

        Task<string> SignOnAsync(Operator oper);

        Task<string> GetValidFunctionsAsync(int taskId);

        Task<string> GetRegionPermissionsForWorkTypeAsync(int workType);

        Task<string> GetPickingRegionForWorkTypeAsync(string pickingRegion, int workType);

        Task<string> GetAssignmentAsync(int numberOfAssignments, int assignmentType);

        Task<string> UpdateStatusAsync(long? groupId, long? locationId, string slotAisle, string setStatusTo, int useLuts);

        Task<string> GetContainersAsync(long? groupId, long assignmentId, string targetContainer, long? pickContainerId, string containerNumber, int operation, string labels);

        Task<string> GetPicksAsync(long? groupId, bool shortsAndSkipsFlag, int goBackForSkipsIndicator, int pickOrderFlag);

        Task<string> VerifyReplenishmentAsync(long locationId, string itemNumber);

        Task<string> ExecutePickAsync(long? groupId, long assignmentId, long locationId, int quantityPicked, bool endOfPartialPickingFlag, long? containerId, long pickId, string lotNumber, double? variableWeight, string itemSerialNumer, int useLuts);

        Task<string> PassAssignmentAsync(long? groupId);

        Task<string> StopAssignmentAsync(long? groupId);

        Task<string> SignOffAsync();

        Task<string> GetRequestWorkAsync(string workId, int scanned, int assignmentType);
    }
}
