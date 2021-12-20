//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using System.Threading.Tasks;
    using GuidedWork;
    using GuidedWorkRunner;

    public class VoiceLinkDataService : IVoiceLinkDataService
    {
        private readonly IVoiceLinkDataProxy _DataProxy;
        private readonly IVoiceLinkConfigRepository _VoiceLinkConfigRepository;

        public VoiceLinkDataService(IVoiceLinkDataProxy dataProxy, IVoiceLinkConfigRepository voiceLinkConfigRepository)
        {
            _DataProxy = dataProxy;
            _VoiceLinkConfigRepository = voiceLinkConfigRepository;
        }

        public void Initialize()
        {
            if (_DataProxy.DataTransport == null)
            {
                SelectTransport();
            }
            _DataProxy.DataTransport.Initialize();
        }

        private void SelectTransport()
        {
            if (_VoiceLinkConfigRepository.GetConfig("WorkflowFilterChoice")?.Value ==
                LocalizationHelper.ServerLocalizationKey)
            {
                _DataProxy.SelectTransport("RESTDataTransport");
            }
            else if (_VoiceLinkConfigRepository.GetConfig("WorkflowFilterChoice")?.Value ==
                LocalizationHelper.LegacySocketServerLocalizationKey)
            {
                _DataProxy.SelectTransport("TCPSocketDataTransport");
            }
            else
            {
                _DataProxy.SelectTransport("FileDataTransport");
            }
        }

        public async Task<GenericResponseLUT> ExecutePickAsync(Assignment assignment, long locationId, int quantityPicked, bool endOfPartialPickingFlag, long? containerId, long pickId, string lotNumber, double? variableWeight, string itemSerialNumer, int useLuts)
        {
            var response = await _DataProxy.DataTransport.ExecutePickAsync(assignment.GroupID, assignment.AssignmentID, locationId, quantityPicked, endOfPartialPickingFlag, containerId, pickId, lotNumber, variableWeight, itemSerialNumer, useLuts);
            return new GenericResponseLUT(response);
        }

        public async Task<AssignmentLUT> GetAssignmentAsync(int numberOfAssignments, int assignmentType)
        {
            var response = await _DataProxy.DataTransport.GetAssignmentAsync(numberOfAssignments, assignmentType);
            return new AssignmentLUT(response);
        }

        public async Task<BreakTypeLUT> GetBreakTypesAsync()
        {
            var response = await _DataProxy.DataTransport.GetBreakTypesAsync();
            return new BreakTypeLUT(response);
        }

        public async Task<ContainerLUT> GetContainersAsync(Assignment assignment, string targetContainer, long? pickContainerId, string containerNumber, int operation, string labels)
        {
            var response = await _DataProxy.DataTransport.GetContainersAsync(assignment.GroupID, assignment.AssignmentID, targetContainer, pickContainerId, containerNumber, operation, labels);
            return new ContainerLUT(response);
        }

        public async Task<PickingRegionLUT> GetPickingRegionForWorkTypeAsync(string pickingRegion, int workType)
        {
            var response = await _DataProxy.DataTransport.GetPickingRegionForWorkTypeAsync(pickingRegion, workType);
            return new PickingRegionLUT(response);
        }

        public async Task<PickLUT> GetPicksAsync(long? groupId, bool shortsAndSkipsFlag, int goBackForSkipsIndicator, int pickOrderFlag)
        {
            var response = await _DataProxy.DataTransport.GetPicksAsync(groupId, shortsAndSkipsFlag, goBackForSkipsIndicator, pickOrderFlag);
            return new PickLUT(response);
        }

        public async Task<RegionLUT> GetRegionPermissionsForWorkTypeAsync(int workType)
        {
            var response = await _DataProxy.DataTransport.GetRegionPermissionsForWorkTypeAsync(workType);
            return new RegionLUT(response);
        }

        public async Task<FunctionLUT> GetValidFunctionsAsync(int taskId)
        {
            var response = await _DataProxy.DataTransport.GetValidFunctionsAsync(taskId);
            return new FunctionLUT(response);
        }

        public async Task<GenericResponseLUT> PassAssignmentAsync(long? groupId)
        {
            var response = await _DataProxy.DataTransport.PassAssignmentAsync(groupId);
            return new GenericResponseLUT(response);
        }

        public async Task<VLConfigLUT> SendConfigAsync()
        {
            var response = await _DataProxy.DataTransport.SendConfigAsync();
            return new VLConfigLUT(response);
        }

        public async Task<GenericResponseLUT> SignOffAsync()
        {
            var response = await _DataProxy.DataTransport.SignOffAsync();
            return new GenericResponseLUT(response);
        }

        public async Task<SignOnLUT> SignOnAsync(GuidedWorkRunner.Operator oper)
        {
            var response = await _DataProxy.DataTransport.SignOnAsync(oper);
            if(response == "") //handle unlicensed system case
            {
                response = "-1,\n\n";
            }
            return new SignOnLUT(response);
        }

        public async Task<GenericResponseLUT> StopAssignmentAsync(long? groupId)
        {
            var response = await _DataProxy.DataTransport.StopAssignmentAsync(groupId);
            return new GenericResponseLUT(response);
        }

        public async Task<GenericResponseLUT> UpdateStatusAsync(long? groupId, long? locationId, string slotAisle, string setStatusTo, int useLuts)
        {
            var response = await _DataProxy.DataTransport.UpdateStatusAsync(groupId, locationId, slotAisle, setStatusTo, useLuts);
            return new GenericResponseLUT(response);
        }

        public async Task<VerifyReplenishmentLUT> VerifyReplenishmentAsync(long locationId, string itemNumber)
        {
            var response = await _DataProxy.DataTransport.VerifyReplenishmentAsync(locationId, itemNumber);
            return new VerifyReplenishmentLUT(response);
        }

        public async Task<RequestWorkLUT> GetRequestWorkAsync(string workId, int scanned, int assignmentType)
        {
            var response = await _DataProxy.DataTransport.GetRequestWorkAsync(workId, scanned, assignmentType);
            return new RequestWorkLUT(response);
        }
    }
}
