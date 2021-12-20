//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using GuidedWork;
    using GuidedWorkRunner;

    public class VoiceLinkRESTDataTransport : WorkflowRESTDataTransport, IVoiceLinkDataTransport
    {
        private readonly IVoiceLinkRESTServiceProvider _RestServiceProvider;
        private readonly IDeviceInfo _DeviceInfo;
        private readonly IVoiceLinkConfigRepository _VoiceLinkConfigRepository;

        private readonly string _DeviceSN;

        private const string _TaskVersion = "CT-31-03-076";

        public VoiceLinkRESTDataTransport(IVoiceLinkRESTServiceProvider restServiceProvider,
                                          IDeviceInfo deviceInfo,
                                          IVoiceLinkConfigRepository voiceLinkConfigRepository)
        {
            _RestServiceProvider = restServiceProvider;
            _DeviceInfo = deviceInfo;
            _DeviceSN = _DeviceInfo.GetDeviceSerialNumber();
            _VoiceLinkConfigRepository = voiceLinkConfigRepository;
        }

        public void Initialize()
        {
        }

        public async Task<string> ExecutePickAsync(long? groupId, long assignmentId, long locationId, int quantityPicked, bool endOfPartialPickingFlag, long? containerId, long pickId, string lotNumber, double? variableWeight, string itemSerialNumer, int useLuts)
        {
            return await _RestServiceProvider.ExecutePickAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, groupId, assignmentId, locationId, quantityPicked, endOfPartialPickingFlag, containerId, pickId, lotNumber, variableWeight, itemSerialNumer, useLuts);
        }

        public async Task<string> GetAssignmentAsync(int numberOfAssignments, int assignmentType)
        {
            return await _RestServiceProvider.GetAssignmentAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, numberOfAssignments, assignmentType);
        }

        public async Task<string> GetBreakTypesAsync()
        {
            return await _RestServiceProvider.GetBreakTypesAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent")?.Value);
        }

        public async Task<string> GetContainersAsync(long? groupId, long assignmentId, string targetContainer, long? pickContainerId, string containerNumber, int operation, string labels)
        {
            return await _RestServiceProvider.GetContainersAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, groupId, assignmentId, targetContainer, pickContainerId, containerNumber, operation, labels);
        }

        public async Task<string> GetPickingRegionForWorkTypeAsync(string pickingRegion, int workType)
        {
            return await _RestServiceProvider.GetPickingRegionForWorkTypeAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, pickingRegion, workType);
        }

        public async Task<string> GetPicksAsync(long? groupId, bool shortsAndSkipsFlag, int goBackForSkipsIndicator, int pickOrderFlag)
        {
            return await _RestServiceProvider.GetPicksAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, groupId, shortsAndSkipsFlag, goBackForSkipsIndicator, pickOrderFlag);
        }

        public async Task<string> GetRegionPermissionsForWorkTypeAsync(int workType)
        {
            return await _RestServiceProvider.GetRegionPermissionsForWorkTypeAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, workType);
        }

        public async Task<string> GetValidFunctionsAsync(int taskId)
        {
            return await _RestServiceProvider.GetValidFunctionsAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, taskId);
        }

        public async Task<string> PassAssignmentAsync(long? groupId)
        {
            return await _RestServiceProvider.PassAssignmentAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, groupId);
        }

        public async Task<string> SendConfigAsync()
        {
            return await _RestServiceProvider.SendConfigAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent")?.Value, CultureInfo.CurrentCulture.Name.Replace('-', '_'), _VoiceLinkConfigRepository.GetConfig("SiteName").Value, _TaskVersion);
        }

        public async Task<string> SignOffAsync()
        {
            return await _RestServiceProvider.SignOffAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value);
        }

        public async Task<string> SignOnAsync(GuidedWorkRunner.Operator oper)
        {
            return await _RestServiceProvider.SignOnAsync(DateTime.Now, _DeviceSN, oper.OperatorIdentifier, oper.Password);
        }

        public async Task<string> StopAssignmentAsync(long? groupId)
        {
            return await _RestServiceProvider.StopAssignmentAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, groupId);
        }

        public async Task<string> UpdateStatusAsync(long? groupId, long? locationId, string slotAisle, string setStatusTo, int useLuts)
        {
            return await _RestServiceProvider.UpdateStatusAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, groupId, locationId, slotAisle, setStatusTo, useLuts);
        }

        public async Task<string> VerifyReplenishmentAsync(long locationId, string itemNumber)
        {
            return await _RestServiceProvider.VerifyReplenishmentAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, locationId, itemNumber);
        }

        public async Task<string> GetRequestWorkAsync(string workId, int scanned, int assignmentType)
        {
            return await _RestServiceProvider.GetRequestWorkTypeAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, workId, scanned, assignmentType);
        }

    }
}
