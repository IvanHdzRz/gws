//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class VoiceLinkBaseServiceProvider : IVoiceLinkBaseServiceProvider
    {
        private static readonly ILog _Log = LogManager.GetLogger(nameof(VoiceLinkBaseServiceProvider));

        protected enum LUTFlag
        {
            USE_ODR,
            USE_LUT_IF_NO_PENDING_ODR,
            USE_LUT
        }

        protected class Command
        {
            [JsonProperty("method")]
            public string Method;

            [JsonProperty("param")]
            public List<string> Params;

            public string TCPFormat()
            {
                return Method + "('" + String.Join("','", Params.ToArray()) + "')" + "\r\n\n";
            }

            public override string ToString()
            {
                return Method + "('" + String.Join("','", Params.ToArray()) + "')";
            }
        }

        private string FormatDateTime(DateTime timeToFormat)
        {
            var timeToTZ = TZEnvDateTime.ConvertLocalTimeToTZTime(timeToFormat);
            return timeToTZ.ToString("MM'-'dd'-'yy HH':'mm':'ss'.'fff");
        }

        public Task<string> ExecutePickAsync(DateTime commandDate, string serialNumber, string operatorId, long? groupId, long assignmentId, long locationId, int quantityPicked, bool endOfPartialPickingFlag, long? containerId, long pickId, string lotNumber, double? variableWeight, string itemSerialNumer, int useLuts, CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "Picked",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                    groupId?.ToString(),
                    assignmentId.ToString(),
                    locationId.ToString(),
                    quantityPicked.ToString(),
                    Convert.ToInt32(endOfPartialPickingFlag).ToString(),
                    containerId?.ToString(),
                    pickId.ToString(),
                    lotNumber,
                    variableWeight?.ToString(),
                    itemSerialNumer
                }
            };

            return SendVoiceLinkRequest(command, useLuts, cancellationToken);
        }

        public Task<string> GetAssignmentAsync(DateTime commandDate, string serialNumber, string operatorId, int numberOfAssignments, int assignmentType, CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "GetAssignment",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                    numberOfAssignments.ToString(),
                    assignmentType.ToString()
                }
            };

            return SendVoiceLinkRequest(command, cancellationToken: cancellationToken);
        }

        public Task<string> GetBreakTypesAsync(DateTime commandDate, string serialNumber, string operatorId, CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "CoreBreakTypes",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId
                }
            };

            return SendVoiceLinkRequest(command, cancellationToken: cancellationToken);
        }

        public Task<string> GetContainersAsync(DateTime commandDate, string serialNumber, string operatorId, long? groupId, long assignmentId, string targetContainer, long? pickContainerId, string containerNumber, int operation, string labels, CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "Container",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                    groupId?.ToString(),
                    assignmentId.ToString(),
                    targetContainer,
                    pickContainerId?.ToString(),
                    containerNumber,
                    operation.ToString(),
                    labels
                }
            };
            command.Params = command.Params.Select(p => { return p ?? ""; }).ToList();

            return SendVoiceLinkRequest(command, cancellationToken: cancellationToken);
        }

        public Task<string> GetValidFunctionsAsync(DateTime commandDate, string serialNumber, string operatorId, int taskId, CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "CoreValidFunctions",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                    taskId.ToString()
                }
            };

            return SendVoiceLinkRequest(command, cancellationToken: cancellationToken);
        }

        public Task<string> GetPickingRegionForWorkTypeAsync(DateTime commandDate, string serialNumber, string operatorId, string pickingRegion, int workType, CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "PickingRegion",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                    pickingRegion,
                    workType.ToString()
                }
            };

            return SendVoiceLinkRequest(command, cancellationToken: cancellationToken);
        }

        public Task<string> GetPicksAsync(DateTime commandDate, string serialNumber, string operatorId, long? groupId, bool shortsAndSkipsFlag, int goBackForSkipsIndicator, int pickOrderFlag, CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "GetPicks",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                    groupId?.ToString(),
                    Convert.ToInt32(shortsAndSkipsFlag).ToString(),
                    goBackForSkipsIndicator.ToString(),
                    pickOrderFlag.ToString()
                }
            };

            return SendVoiceLinkRequest(command, cancellationToken: cancellationToken);
        }

        public Task<string> GetRegionPermissionsForWorkTypeAsync(DateTime commandDate, string serialNumber, string operatorId, int workType, CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "RegionPermissionsForWorkType",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                    workType.ToString()
                }
            };

            return SendVoiceLinkRequest(command, cancellationToken: cancellationToken);
        }

        public Task<string> PassAssignmentAsync(DateTime commandDate, string serialNumber, string operatorId, long? groupId, CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "PassAssignment",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                    groupId?.ToString()
                }
            };

            return SendVoiceLinkRequest(command, cancellationToken: cancellationToken);
        }

        public Task<string> SendConfigAsync(DateTime commandDate, string serialNumber, string operatorId, string locale, string siteName, string taskVersion, CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "CoreConfiguration",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                    locale,
                    siteName,
                    taskVersion
                }
            };

            return SendVoiceLinkRequest(command, cancellationToken: cancellationToken);
        }

        public Task<string> SignOffAsync(DateTime commandDate, string serialNumber, string operatorId, CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "CoreSignOff",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                }
            };

            return SendVoiceLinkRequest(command, cancellationToken: cancellationToken);
        }

        public Task<string> SignOnAsync(DateTime commandDate, string serialNumber, string operatorId, string password, CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "CoreSignOn",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                    password
                }
            };

            var logCommand = new Command
            {
                Method = "CoreSignOn",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                    "*****************"
                }
            };

            return SendVoiceLinkRequest(command, cancellationToken: cancellationToken, logCommand: logCommand);
        }

        public Task<string> StopAssignmentAsync(DateTime commandDate, string serialNumber, string operatorId, long? groupId, CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "StopAssignment",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                    groupId?.ToString()
                }
            };

            return SendVoiceLinkRequest(command, cancellationToken: cancellationToken);
        }

        public Task<string> UpdateStatusAsync(DateTime commandDate, string serialNumber, string operatorId, long? groupId, long? locationId, string slotAisle, string setStatusTo, int useLuts, CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "UpdateStatus",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                    groupId?.ToString(),
                    locationId?.ToString(),
                    slotAisle,
                    setStatusTo
                }
            };

            return SendVoiceLinkRequest(command, useLuts, cancellationToken);
        }

        public Task<string> VerifyReplenishmentAsync(DateTime commandDate, string serialNumber, string operatorId, long locationId, string itemNumber, CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "VerifyReplenishment",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                    locationId.ToString(),
                    itemNumber
                }
            };

            return SendVoiceLinkRequest(command, cancellationToken: cancellationToken);
        }

        public Task<string> GetRequestWorkTypeAsync(DateTime commandDate, string serialNumber, string operatorId, string workId, int scanned, int assignmentType,
            CancellationToken cancellationToken = default)
        {
            var command = new Command
            {
                Method = "RequestWork",
                Params = new List<string>
                {
                    FormatDateTime(commandDate),
                    serialNumber,
                    operatorId,
                    workId,
                    scanned.ToString(),
                    assignmentType.ToString()
                }
            };

            return SendVoiceLinkRequest(command, cancellationToken: cancellationToken);
        }

        protected virtual async Task<string> SendVoiceLinkRequest(Command command, int useLuts = 2, 
            CancellationToken cancellationToken = default, Command logCommand = null)
        {
            string response;
            if (useLuts == (int)LUTFlag.USE_ODR)
            {
                _Log.Debug($"Sending VoiceLink command as ODR");

                // Send as ODR.  Put the request in the queue and continue without waiting for it to complete.
                command.Method = "prTaskODR" + command.Method;
                response = await SendRequest(command, true, cancellationToken, logCommand);
            }
            else if (useLuts == (int)LUTFlag.USE_LUT)
            {
                _Log.Debug($"Sending VoiceLink command as LUT");

                // Send as LUT.  Wait for pending ODRs to be sent then wait for the new request to complete.
                command.Method = "prTaskLUT" + command.Method;
                response = await SendRequest(command, false, cancellationToken, logCommand);
            }
            else
            {
                _Log.Debug($"Sending VoiceLink command as LUT if no pending ODRs");

                // When the value of useLuts is 1 the request should be sent as a LUT if there are not pending ODRs,
                // otherwise it should be sent as an ODR.
                if (RequestsPending() == 0)
                {
                    command.Method = "prTaskLUT" + command.Method;
                    response = await SendRequest(command, false, cancellationToken, logCommand);
                }
                else
                {
                    command.Method = "prTaskODR" + command.Method;
                    response = await SendRequest(command, true, cancellationToken, logCommand);
                }
            }

            return response;
        }

        protected abstract Task<string> SendRequest(Command command, bool asODR, CancellationToken cancellationToken, Command logCommand = null);
        protected abstract uint RequestsPending();
    }
}
