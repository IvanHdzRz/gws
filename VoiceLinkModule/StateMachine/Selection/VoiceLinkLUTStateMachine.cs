//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using Common.Logging;
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class VoiceLinkLUTStateMachine : SimplifiedBaseServerCommStateMachine<IVoiceLinkModel, VoiceLinkStateMachine, IVoiceLinkConfigRepository>
    {
        private readonly ILog _Log = LogManager.GetLogger(nameof(VoiceLinkLUTStateMachine));

        private bool _OriginalAdditionalVocabEnabled = true;

        public VoiceLinkLUTStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            _OriginalAdditionalVocabEnabled = Model.AdditionalVocabEnabled;
            Model.AdditionalVocabEnabled = false;

            base.ConfigureStates();
        }

        /// <summary>
        /// Executes the server request and returns the response code
        /// </summary>
        /// <returns>Response code</returns>
        protected override async Task<ICoreAppServerCommResponse> ExecuteServiceCallAsync(Enum serviceToCall)
        {
            ICoreAppServerCommResponse response = null;
            switch (serviceToCall)
            {
                case LutType.BreakTypes:
                    response = await Model.DataService.GetBreakTypesAsync();
                    if (response is BreakTypeLUT)
                    {
                        if (response.ResponseCode == 0)
                        {
                            Model.AvailableBreakTypes.AddRange(response as BreakTypeLUT);
                        }
                    }
                    else
                    {
                        _Log.Error($"Invalid response type for BreakTypeLUT.  Expected {nameof(BreakTypeLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    break;
                case LutType.CompleteAssignment:
                    response = await Model.DataService.StopAssignmentAsync(AssignmentsResponse.CurrentResponse[0].GroupID);
                    if (response is GenericResponseLUT)
                    {
                        GenericLUTResponse.UpdateCurrentResponse(response as GenericResponseLUT);
                    }
                    else
                    {
                        _Log.Error($"Invalid response type for GenericResponseLUT.  Expected {nameof(GenericResponseLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    break;
                case LutType.Config:
                    response = await Model.DataService.SendConfigAsync();
                    if (response is VLConfigLUT)
                    {
                        Model.CurrentConfig = (response as VLConfigLUT)[0];
                    }
                    else
                    {
                        _Log.Error($"Invalid response type for VLConfigLUT.  Expected {nameof(VLConfigLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    break;
                case LutType.ExecutePick:
                    var executePickParams = (ExecutePickParam) Model.LutParameters;
                    response = await Model.DataService.ExecutePickAsync(
                        executePickParams.CurrentAssigment,
                        executePickParams.Pick.LocationId,
                        executePickParams.Quantity,
                        executePickParams.Complete,
                        executePickParams.ContainerId,
                        executePickParams.Pick.PickId,
                        executePickParams.LotNumber,
                        executePickParams.Weight,
                        executePickParams.SerialNumber,
                        PickingRegionsResponse.CurrentPickingRegion.UseLut
                        );
                    if (!(response is GenericResponseLUT))
                    {
                        _Log.Error($"Invalid response type for GenericResponseLUT.  Expected {nameof(GenericResponseLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    break;
                case LutType.GetAssignments:
                    var getAssignmentsParams = (GetAssignmentsParam) Model.LutParameters;
                    response = await Model.DataService.GetAssignmentAsync(getAssignmentsParams.AssignmentsToRequest, PickingRegionsResponse.CurrentPickingRegion.AssignmentType);
                    if (response is AssignmentLUT)
                    {
                        AssignmentsResponse.UpdateCurrentResponse(response as AssignmentLUT);
                    }
                    else
                    {
                        _Log.Error($"Invalid response type for AssignmentLUT.  Expected {nameof(AssignmentLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    break;
                case LutType.GetContainers:
                    var getContainersParams = (GetContainersParam) Model.LutParameters;

                    response = await Model.DataService.GetContainersAsync(
                        getContainersParams.Assignment,
                        getContainersParams.TargetContainer,
                        getContainersParams.PickContainerId,
                        getContainersParams.ContainerNumber,
                        getContainersParams.Operation,
                        getContainersParams.Labels
                        );
                    if (response is ContainerLUT)
                    {
                        ContainersResponse.UpdateCurrentResponse(response as ContainerLUT);
                        if (response.ResponseCode > 0 && getContainersParams.Operation == 2)
                        {
                            CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_Selection_NewContainer_Failed");
                        }
                    }
                    else
                    {
                        _Log.Error($"Invalid response type for ContainerLUT.  Expected {nameof(ContainerLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    break;
                case LutType.GetFunctions:
                    response = await Model.DataService.GetValidFunctionsAsync(0);
                    if (response is FunctionLUT)
                    {
                        FunctionsResponse.UpdateCurrentResponse(response as FunctionLUT);
                    }
                    else
                    {
                        _Log.Error($"Invalid response type for FunctionLUT.  Expected {nameof(FunctionLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    break;
                case LutType.GetPickingRegion:
                    response = await Model.DataService.GetPickingRegionForWorkTypeAsync(Model.CurrentRegion.Number.ToString(), Model.CurrentFunction.Number);
                    if (response is PickingRegionLUT)
                    {
                        PickingRegionsResponse.UpdateCurrentResponse(response as PickingRegionLUT);
                    }
                    else
                    {
                        _Log.Error($"Invalid response type for PickingRegionLUT.  Expected {nameof(PickingRegionLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    break;
                case LutType.GetRegionPermissions:
                    response = await Model.DataService.GetRegionPermissionsForWorkTypeAsync(Model.CurrentFunction.Number);
                    if (response is RegionLUT)
                    {
                        RegionsResponse.UpdateCurrentResponse(response as RegionLUT);
                    }
                    else
                    {
                        _Log.Error($"Invalid response type for RegionLUT.  Expected {nameof(RegionLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    break;
                case LutType.PassAssignment:
                    response = await Model.DataService.PassAssignmentAsync(AssignmentsResponse.CurrentResponse[0].GroupID);
                    if (!(response is GenericResponseLUT))
                    {
                        _Log.Error($"Invalid response type for GenericResponseLUT.  Expected {nameof(GenericResponseLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    GenericLUTResponse.UpdateCurrentResponse(response as GenericResponseLUT);
                    break;
                case LutType.GetPicks:
                    response = await Model.DataService.GetPicksAsync(AssignmentsResponse.FirstAssignment().GroupID,
                        AssignmentsResponse.CurrentResponse.Any(a => a.PassAssignment == 1),
                        PickingRegionsResponse.CurrentPickingRegion.GoBackForShorts,
                        0);
                    if (response is PickLUT)
                    {
                        PicksResponse.UpdateCurrentResponse(response as PickLUT);
                    }
                    else
                    {
                        _Log.Error($"Invalid response type for PickLUT.  Expected {nameof(PickLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    break;
                case LutType.Replenishment:
                    var ReplenishmentParams = (ReplenishmentParam) Model.LutParameters;
                    Pick picklist0 = ReplenishmentParams.Pick;

                    response = await Model.DataService.VerifyReplenishmentAsync(picklist0.LocationId, picklist0.ItemNumber);
                    if (response is VerifyReplenishmentLUT)
                    {
                        VerifyReplenishmentResponse.UpdateCurrentResponse(response as VerifyReplenishmentLUT);
                    }
                    else
                    {
                        _Log.Error($"Invalid response type for VerifyReplenishmentLUT.  Expected {nameof(VerifyReplenishmentLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    break;
                case LutType.RequestWork:
                    response = await Model.DataService.GetRequestWorkAsync(Model.WorkId, Convert.ToInt32(!Model.WorkIdScanned), PickingRegionsResponse.CurrentPickingRegion.AssignmentType);
                    if (response is RequestWorkLUT)
                    {
                        RequestWorkResponse.UpdateCurrentResponse(response as RequestWorkLUT);
                    }
                    else
                    {
                        _Log.Error($"Invalid response type for RequestWorkLUT.  Expected {nameof(RequestWorkLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    break;
                case LutType.SignOff:
                    response = await Model.DataService.SignOffAsync();
                    if (!(response is GenericResponseLUT))
                    {
                        _Log.Error($"Invalid response type for GenericResponseLUT.  Expected {nameof(GenericResponseLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    break;
                case LutType.SignOn:
                    response = await Model.DataService.SignOnAsync(Model.NewOperator);
                    if (!(response is SignOnLUT))
                    {
                        _Log.Error($"Invalid response type for SignOnLUT.  Expected {nameof(SignOnLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    break;
                case LutType.UpdateStatus:
                    var updateStatusParams = (UpdateStatusParam) Model.LutParameters;

                    response = await Model.DataService.UpdateStatusAsync(
                        AssignmentsResponse.FirstAssignment().GroupID,
                        updateStatusParams.Location,
                        updateStatusParams.Scope,
                        updateStatusParams.Status,
                        PickingRegionsResponse.CurrentPickingRegion.UseLut);
                    if (!(response is GenericResponseLUT))
                    {
                        _Log.Error($"Invalid response type for GenericResponseLUT.  Expected {nameof(GenericResponseLUT)} but was {response.GetType()}");
                        throw new InvalidResponseException("VoiceLink_Xmit_InvalidResponse");
                    }
                    break;
            }

            return response;
        }

        // <summary>
        // Process specific response code
        // </summary>
        protected override void ValidateResponse()
        {
            var error = Model.CurrentCommResponse.ResponseCode;

            if (error == GenericResponseLUT.Warning) //Voicelink standard message code (not an error)
            {
                Common.DisplayMessage = true;
                Common.RetryTransmit = false;
                Model.MessageType = UserMessageType.Standard;
            }
            else if (error == GenericResponseLUT.Critical) //Fatal error from server, force sign off
            {
                Common.DisplayMessage = true;
                Common.RetryTransmit = false;
                Common.ReturnToState = VoiceLinkStateMachine.SignOff;
                Model.MessageType = UserMessageType.Error;
            }
            else if (error < 0)
            {
                Common.DisplayMessage = true;
                Common.RetryTransmit = true;
                Model.CurrentCommResponse = new GenericResponseLUT(-1, Translate.GetLocalizedTextForKey("VoiceLink_Error_Contacting_Host_Prompt"));
                Model.MessageType = UserMessageType.Error;
            }
            else
            {
                base.ValidateResponse();
            }
        }

        protected override WorkflowObjectContainer EncodeDisplayCommMessage(IVoiceLinkModel model)
        {
            //Check message key based on speaking an error or warning.
            string key = model.CurrentCommResponse.ResponseCode == GenericResponseLUT.Warning ?
                "VoiceLink_Say_Ready_Continue_Prompt" : "VoiceLink_Say_Ready_Try_Again_Prompt";

            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("VoiceLink_Server_Message_Header"),
                                                              "readyErrorHandling",
                                                              Translate.GetLocalizedTextForKey(key, model.CurrentCommResponse.ErrorMessage),
                                                              model.CurrentCommResponse.ErrorMessage,
                                                              isPriorityPrompt: false);

            //disable voice if operator is not signed in yet
            //May cause noise sample to occur during dialogue if not disabled
            wfo.VoiceEnabled = model.CurrentOperator != null || model.RunnerRequiresVoiceEnabled;

            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        /// <summary>
        /// For voicelink reset AddtionalVocabEnabled before returning
        /// </summary>
        protected override void OnReturnFromStateMachine()
        {
            Model.AdditionalVocabEnabled = _OriginalAdditionalVocabEnabled;
        }
    }
}
