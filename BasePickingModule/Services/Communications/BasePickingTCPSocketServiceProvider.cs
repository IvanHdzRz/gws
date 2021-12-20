//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Logging;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;

    public class BasePickingTCPSocketServiceProvider : IBasePickingTCPSocketServiceProvider
    {
        private static readonly ILog _Log = LogManager.GetLogger(nameof(BasePickingTCPSocketServiceProvider));

        private readonly IBasePickingTCPSocketService _TCPSocketService;
        private readonly IDeviceInfo _DeviceInfo;

        protected class Command
        {
            public string Method;

            public List<string> Params;

            public string TCPFormat()
            {
                return Method + "('" + string.Join("','", Params.ToArray()) + "')" + "\r\n\n";
            }

            public override string ToString()
            {
                return Method + "('" + string.Join("','", Params.ToArray()) + "')";
            }
        }

        public BasePickingTCPSocketServiceProvider(IBasePickingTCPSocketService TCPSocketService, IDeviceInfo deviceInfo)
        {
            _TCPSocketService = TCPSocketService;
            _DeviceInfo = deviceInfo;
        }

        private string FormatDateTime(DateTime timeToFormat)
        {
            var timeToTZ = TZEnvDateTime.ConvertLocalTimeToTZTime(timeToFormat);
            return timeToTZ.ToString("MM-dd-yy HH:mm:ss.fff");
        }

        protected uint RequestsPending()
        {
            return _TCPSocketService.PersistedRequestsPending;
        }

        protected Task<string> SendRequest(Command command, bool asODR, CancellationToken cancellationToken, Command logCommand = null)
        {
            string logData = command.ToString();
            if (logCommand != null)
            {
                logData = logCommand.ToString();
            }

            if (asODR)
            {
                _TCPSocketService.ExecuteTCPSocketDataAsync(command.TCPFormat(), true, asODR, cancellationToken, logData);
                return Task.FromResult("0");
            }
            return _TCPSocketService.ExecuteTCPSocketDataAsync(command.TCPFormat(), false, asODR, cancellationToken, logData);
        }

        public Task<string> SignOnAsync(string operatorIdenitifier, string password, CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new Command
            {
                Method = "GetSignOn",
                Params = new List<string>
                {
                    FormatDateTime(DateTime.Now),
                    _DeviceInfo.GetDeviceSerialNumber(),
                    operatorIdenitifier,
                    password
                }
            };

            //Differ command object masking the password so it doesn't end up in logs
            var logCommand = new Command
            {
                Method = "GetSignOn",
                Params = new List<string>
                {
                    FormatDateTime(DateTime.Now),
                    _DeviceInfo.GetDeviceSerialNumber(),
                    operatorIdenitifier,
                    "********"
                }
            };

            return SendRequest(command, false, cancellationToken, logCommand);
        }

        public Task<string> SignOffAsync(string operatorIdentifier, CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new Command
            {
                Method = "GetSignOff",
                Params = new List<string>
                {
                    FormatDateTime(DateTime.Now),
                    _DeviceInfo.GetDeviceSerialNumber(),
                    operatorIdentifier
                }
            };
            return SendRequest(command, false, cancellationToken);
        }

        public Task<string> GetPicksAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new Command
            {
                Method = "GetPicks",
                Params = new List<string>
                {
                    FormatDateTime(DateTime.Now),
                    _DeviceInfo.GetDeviceSerialNumber()
                }
            };
            return SendRequest(command, false, cancellationToken);
        }

        public Task<string> UpdatePickAsync(long pickId, int quantityPicked, CancellationToken cancellationToken = default(CancellationToken))
        {
            var command = new Command
            {
                Method = "PostPick",
                Params = new List<string>
                {
                    FormatDateTime(DateTime.Now),
                    _DeviceInfo.GetDeviceSerialNumber(),
                    pickId.ToString(),
                    quantityPicked.ToString()
                }
            };
            return SendRequest(command, true, cancellationToken);
        }
    }
}
