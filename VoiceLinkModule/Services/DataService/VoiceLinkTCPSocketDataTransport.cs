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
    using Honeywell.Firebird.CoreLibrary;

    public class VoiceLinkTCPSocketDataTransport : WorkflowTCPSocketDataTransport, IVoiceLinkDataTransport
    {
        private readonly IVoiceLinkTCPSocketServiceProvider _TCPSocketServiceProvider;
        private readonly GuidedWork.IDeviceInfo _DeviceInfo;
        private readonly IVoiceLinkConfigRepository _VoiceLinkConfigRepository;
        private readonly ITimeoutHandler _TCPTimeoutHandler;

        private readonly string _DeviceSN;

        private const string _TaskVersion = "CT-31-03-076";

        public VoiceLinkTCPSocketDataTransport(IVoiceLinkTCPSocketServiceProvider tcpSocketServiceProvider,
                                          GuidedWork.IDeviceInfo deviceInfo,
                                          IVoiceLinkConfigRepository voiceLinkConfigRepository,
                                          ITimeoutHandler tcpTimeoutHandler)
        {
            _TCPSocketServiceProvider = tcpSocketServiceProvider;
            _DeviceInfo = deviceInfo;
            _DeviceSN = _DeviceInfo.GetDeviceSerialNumber();
            _VoiceLinkConfigRepository = voiceLinkConfigRepository;
            _TCPTimeoutHandler = tcpTimeoutHandler;
        }

        public void Initialize()
        {
        }

        public async Task<string> ExecutePickAsync(long? groupId, long assignmentId, long locationId, int quantityPicked, bool endOfPartialPickingFlag, long? containerId, long pickId, string lotNumber, double? variableWeight, string itemSerialNumer, int useLuts)
        {
            return await _TCPSocketServiceProvider.ExecutePickAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, groupId, assignmentId, locationId, quantityPicked, endOfPartialPickingFlag, containerId, pickId, lotNumber, variableWeight, itemSerialNumer, useLuts, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> GetAssignmentAsync(int numberOfAssignments, int assignmentType)
        {
            return await _TCPSocketServiceProvider.GetAssignmentAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, numberOfAssignments, assignmentType, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> GetBreakTypesAsync()
        {
            return await _TCPSocketServiceProvider.GetBreakTypesAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent")?.Value, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> GetContainersAsync(long? groupId, long assignmentId, string targetContainer, long? pickContainerId, string containerNumber, int operation, string labels)
        {
            return await _TCPSocketServiceProvider.GetContainersAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, groupId, assignmentId, targetContainer, pickContainerId, containerNumber, operation, labels, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> GetPickingRegionForWorkTypeAsync(string pickingRegion, int workType)
        {
            return await _TCPSocketServiceProvider.GetPickingRegionForWorkTypeAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, pickingRegion, workType, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> GetPicksAsync(long? groupId, bool shortsAndSkipsFlag, int goBackForSkipsIndicator, int pickOrderFlag)
        {
            return await _TCPSocketServiceProvider.GetPicksAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, groupId, shortsAndSkipsFlag, goBackForSkipsIndicator, pickOrderFlag, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> GetRegionPermissionsForWorkTypeAsync(int workType)
        {
            return await _TCPSocketServiceProvider.GetRegionPermissionsForWorkTypeAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, workType, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> GetValidFunctionsAsync(int taskId)
        {
            return await _TCPSocketServiceProvider.GetValidFunctionsAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, taskId, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> PassAssignmentAsync(long? groupId)
        {
            return await _TCPSocketServiceProvider.PassAssignmentAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, groupId, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> SendConfigAsync()
        {
            return await _TCPSocketServiceProvider.SendConfigAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent")?.Value, CultureInfo.CurrentCulture.Name.Replace('-', '_'), _VoiceLinkConfigRepository.GetConfig("SiteName").Value, _TaskVersion, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> SignOffAsync()
        {
            return await _TCPSocketServiceProvider.SignOffAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> SignOnAsync(GuidedWorkRunner.Operator oper)
        {
            return await _TCPSocketServiceProvider.SignOnAsync(DateTime.Now, _DeviceSN, oper.OperatorIdentifier, oper.Password, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> StopAssignmentAsync(long? groupId)
        {
            return await _TCPSocketServiceProvider.StopAssignmentAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, groupId, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> UpdateStatusAsync(long? groupId, long? locationId, string slotAisle, string setStatusTo, int useLuts)
        {
            return await _TCPSocketServiceProvider.UpdateStatusAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, groupId, locationId, slotAisle, setStatusTo, useLuts, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> VerifyReplenishmentAsync(long locationId, string itemNumber)
        {
            return await _TCPSocketServiceProvider.VerifyReplenishmentAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, locationId, itemNumber, _TCPTimeoutHandler.GetTimeoutToken());
        }

        public async Task<string> GetRequestWorkAsync(string workId, int scanned, int assignmentType)
        {
            return await _TCPSocketServiceProvider.GetRequestWorkTypeAsync(DateTime.Now, _DeviceSN, _VoiceLinkConfigRepository.GetConfig("OperIdent").Value, workId, scanned, assignmentType, _TCPTimeoutHandler.GetTimeoutToken());
        }
    }
}
