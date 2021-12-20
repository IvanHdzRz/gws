//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IVoiceLinkBaseServiceProvider
    {
        Task<string> SendConfigAsync(DateTime commandDate, string serialNumber, string operatorId, string locale, string siteName, string taskVersion, CancellationToken cancellationToken = default);

        Task<string> GetBreakTypesAsync(DateTime commandDate, string serialNumber, string operatorId, CancellationToken cancellationToken = default);

        Task<string> SignOnAsync(DateTime commandDate, string serialNumber, string operatorId, string password, CancellationToken cancellationToken = default);

        Task<string> GetValidFunctionsAsync(DateTime commandDate, string serialNumber, string operatorId, int taskId, CancellationToken cancellationToken = default);

        Task<string> GetRegionPermissionsForWorkTypeAsync(DateTime commandDate, string serialNumber, string operatorId, int workType, CancellationToken cancellationToken = default);

        Task<string> GetPickingRegionForWorkTypeAsync(DateTime commandDate, string serialNumber, string operatorId, string pickingRegion, int workType, CancellationToken cancellationToken = default);

        Task<string> GetAssignmentAsync(DateTime commandDate, string serialNumber, string operatorId, int numberOfAssignments, int assignmentType, CancellationToken cancellationToken = default);

        Task<string> GetContainersAsync(DateTime commandDate, string serialNumber, string operatorId, long? groupId, long assignmentId, string targetContainer, long? pickContainerId, string containerNumber, int operation, string labels, CancellationToken cancellationToken = default);

        Task<string> GetPicksAsync(DateTime commandDate, string serialNumber, string operatorId, long? groupId, bool shortsAndSkipsFlag, int goBackForSkipsIndicator, int pickOrderFlag, CancellationToken cancellationToken = default);

        Task<string> ExecutePickAsync(DateTime commandDate, string serialNumber, string operatorId, long? groupId, long assignmentId, long locationId, int quantityPicked, bool endOfPartialPickingFlag, long? containerId, long pickId, string lotNumber, double? variableWeight, string itemSerialNumer, int useLuts, CancellationToken cancellationToken = default);

        Task<string> PassAssignmentAsync(DateTime commandDate, string serialNumber, string operatorId, long? groupId, CancellationToken cancellationToken = default);

        Task<string> StopAssignmentAsync(DateTime commandDate, string serialNumber, string operatorId, long? groupId, CancellationToken cancellationToken = default);

        Task<string> SignOffAsync(DateTime commandDate, string serialNumber, string operatorId, CancellationToken cancellationToken = default);

        Task<string> UpdateStatusAsync(DateTime commandDate, string serialNumber, string operatorId, long? groupId, long? locationId, string slotAisle, string setStatusTo, int useLuts, CancellationToken cancellationToken = default);

        Task<string> VerifyReplenishmentAsync(DateTime commandDate, string serialNumber, string operatorId, long locationId, string itemNumber, CancellationToken cancellationToken = default);

        Task<string> GetRequestWorkTypeAsync(DateTime commandDate, string serialNumber, string operatorId, string workId, int scanned, int assignmentType, CancellationToken cancellationToken = default);
    }
}
