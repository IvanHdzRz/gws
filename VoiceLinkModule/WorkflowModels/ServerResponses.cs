//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using GuidedWork;
    using System.Collections.Generic;
    using System.Linq;

    #region LUT Definitions
    public enum LutType
    {
        BreakTypes,
        CompleteAssignment,
        Config,
        ExecutePick,
        GetAssignments,
        GetContainers,
        GetFunctions,
        GetPickingRegion,
        GetPicks,
        GetRegionPermissions,
        PassAssignment,
        Replenishment,
        RequestWork,
        SignOff,
        SignOn,
        UpdateStatus
    }

    public class GenericResponse
    {
        [LutField(Position = 0, IsErrorCode = true)]
        public int ErrorCode;
        [LutField(Position = 1, IsErrorMessage = true)]
        public string ErrorMessage;
    }

    public class GenericResponseLUT : LUTResponse<GenericResponse>
    {
        public static readonly int Warning = 99;
        public static readonly int Critical = 98;

        public GenericResponseLUT(string response) : base(response)
        {
        }

        public GenericResponseLUT(int errorCode, string message)
        {
            ErrorCode = errorCode;
            ErrorMessage = message;
        }
    }

    public class SignOnResponse
    {
        [LutField(Position = 0)]
        public int Interleave;
        [LutField(Position = 1, IsErrorCode = true)]
        public int ErrorCode;
        [LutField(Position = 2, IsErrorMessage = true)]
        public string ErrorMessage;
    }

    public class SignOnLUT : LUTResponse<SignOnResponse>
    {
        public SignOnLUT(string response) : base(response)
        {
        }
    }

    public class VerifyReplenishment
    {
        [LutField(Position = 0)]
        public bool Replenished;

        [LutField(Position = 1, IsErrorCode = true)]
        public int ErrorCode;
        [LutField(Position = 2, IsErrorMessage = true)]
        public string ErrorMessage;
    }

    public class VerifyReplenishmentLUT : LUTResponse<VerifyReplenishment>
    {
        public VerifyReplenishmentLUT(string response) : base(response)
        {
        }
    }


    public class VLConfig
    {
        [LutField(Position = 0)]
        public string CustomerName;
        [LutField(Position = 1)]
        public string OperatorId;
        [LutField(Position = 2)]
        public int ConfirmPassword;

        [LutField(Position = 3, IsErrorCode = true)]
        public int ErrorCode;
        [LutField(Position = 4, IsErrorMessage = true)]
        public string ErrorMessage;
    }

    public class VLConfigLUT : LUTResponse<VLConfig>
    {
        public VLConfigLUT(string response) : base(response)
        {
        }
    }

    public class BreakType
    {
        [LutField(Position = 0)]
        public int Number;
        [LutField(Position = 1)]
        public string Description;

        [LutField(Position = 2, IsErrorCode = true)]
        public int ErrorCode;
        [LutField(Position = 3, IsErrorMessage = true)]
        public string ErrorMessage;
    }

    public class BreakTypeLUT : LUTResponse<BreakType>
    {
        public BreakTypeLUT(string response) : base(response)
        {
        }
    }

    public class Function
    {
        [LutField(Position = 0)]
        public int Number;
        [LutField(Position = 1)]
        public string Description;

        [LutField(Position = 2, IsErrorCode = true)]
        public int ErrorCode;
        [LutField(Position = 3, IsErrorMessage = true)]
        public string ErrorMessage;
    }

    public class FunctionLUT : LUTResponse<Function>
    {
        public FunctionLUT(string response) : base(response)
        {
        }
    }

    public class Region
    {
        [LutField(Position = 0)]
        public int Number;
        [LutField(Position = 1)]
        public string Name;

        [LutField(Position = 2, IsErrorCode = true)]
        public int ErrorCode;
        [LutField(Position = 3, IsErrorMessage = true)]
        public string ErrorMessage;
    }

    public class RegionLUT : LUTResponse<Region>
    {
        public RegionLUT(string response) : base(response)
        {
        }
    }

    public class PickingRegion
    {
        [LutField(Position = 0)]
        public string Number;
        [LutField(Position = 1)]
        public string Name;
        [LutField(Position = 2)]
        public int AssignmentType;
        [LutField(Position = 3)]
        public bool AutoAssign;
        [LutField(Position = 4)]
        public int NumAssignsAllowed;
        [LutField(Position = 5)]
        public bool AllowSkipAisle;
        [LutField(Position = 6)]
        public bool AllowSkipSlot;
        [LutField(Position = 7)]
        public bool AllowRepickSkips;
        [LutField(Position = 8)]
        public int PrintContainerLabels;
        [LutField(Position = 9)]
        public bool PrintChaseLabels;
        [LutField(Position = 10)]
        public int MultiPickPrompt;
        [LutField(Position = 11)]
        public bool AllowSignOff;
        [LutField(Position = 12)]
        public int ContainerType;
        [LutField(Position = 13)]
        public bool DeliverContainerClosed;
        [LutField(Position = 14)]
        public bool AllowPass;
        [LutField(Position = 15)]
        public int DeliveryType;
        [LutField(Position = 16)]
        public bool QuantityVerification;
        [LutField(Position = 17)]
        public int WorkIdLength;
        [LutField(Position = 18)]
        public int GoBackForShorts;
        [LutField(Position = 19)]
        public bool AllowReverse;
        [LutField(Position = 20)]
        public int UseLut;
        [LutField(Position = 21)]
        public string CurrentPreAisle;
        [LutField(Position = 22)]
        public string CurrentAisle;
        [LutField(Position = 23)]
        public string CurrentPostAisle;
        [LutField(Position = 24)]
        public string CurrentSlot;
        [LutField(Position = 25)]
        public bool PrintNumberOfLabelsFlag;
        [LutField(Position = 26)]
        public bool PromptOpenContainer;
        [LutField(Position = 27)]
        public bool AllowMultipleOpenContainers;
        [LutField(Position = 28)]
        public int SpokenContainerValidLength;
        [LutField(Position = 29)]
        public bool PickByPick;

        [LutField(Position = 30, IsErrorCode = true)]
        public int ErrorCode;
        [LutField(Position = 31, IsErrorMessage = true)]
        public string ErrorMessage;
    }

    public class PickingRegionLUT : LUTResponse<PickingRegion>
    {
        public PickingRegionLUT(string response) : base(response)
        {
        }
    }

    public class RequestWork
    {
        [LutField(Position = 0)]
        public string WorkId;

        [LutField(Position = 1, IsErrorCode = true)]
        public int ErrorCode;
        [LutField(Position = 2, IsErrorMessage = true)]
        public string ErrorMessage;
    }

    public class RequestWorkLUT : LUTResponse<RequestWork>
    {
        public RequestWorkLUT(string response) : base(response)
        {
        }

        public List<string> GetDuplicates()
        {
            var result = new List<string>();
            foreach (var record in this)
            {
                result.Add(record.WorkId);
            }
            return result;
        }
    }

    public class Assignment
    {
        [LutField(Position = 0)]
        public long? GroupID;
        [LutField(Position = 1)]
        public bool IsChase;
        [LutField(Position = 2)]
        public long AssignmentID;
        [LutField(Position = 3)]
        public string IDDescription;
        [LutField(Position = 4)]
        public int Position;
        [LutField(Position = 5)]
        public double GoalTime;
        [LutField(Position = 6)]
        public string Route;
        [LutField(Position = 7)]
        public int? ActiveContainer;
        [LutField(Position = 8)]
        public int PassAssignment;
        [LutField(Position = 9)]
        public int SummaryPromptType;
        [LutField(Position = 10)]
        public string OverridePrompt;

        [LutField(Position = 11, IsErrorCode = true)]
        public int ErrorCode;
        [LutField(Position = 12, IsErrorMessage = true)]
        public string ErrorMessage;
    }

    public class AssignmentLUT : LUTResponse<Assignment>
    {
        public AssignmentLUT(string response) : base(response)
        {
        }
    }

    public class Container
    {
        [LutField(Position = 0)]
        public long ContainerID;
        [LutField(Position = 1)]
        public string ScannedContainerValidation;
        [LutField(Position = 2)]
        public string SpokenContainerValidation;
        [LutField(Position = 3)]
        public long AssignmentID;
        [LutField(Position = 4)]
        public long IDDescription;
        [LutField(Position = 5)]
        public int? TargetContainer;
        [LutField(Position = 6)]
        public string ContainerStatus;
        [LutField(Position = 7)]
        public int Printed;

        [LutField(Position = 8, IsErrorCode = true)]
        public int ErrorCode;
        [LutField(Position = 9, IsErrorMessage = true)]
        public string ErrorMessage;
    }

    public class ContainerLUT : LUTResponse<Container>
    {
        public ContainerLUT(string response) : base(response)
        {
        }
    }

    public class Pick
    {
        [LutField(Position = 0)]
        public string Status;
        [LutField(Position = 1)]
        public bool BaseItem;
        [LutField(Position = 2)]
        public long PickId;
        [LutField(Position = 3)]
        public long LocationId;
        [LutField(Position = 4)]
        public int RegionNumber;
        [LutField(Position = 5)]
        public string PreAisleDirection;
        [LutField(Position = 6)]
        public string Aisle;
        [LutField(Position = 7)]
        public string PostAisleDirection;
        [LutField(Position = 8)]
        public string Slot;
        [LutField(Position = 9)]
        public int QuantityToPick;
        [LutField(Position = 10)]
        public string UOM;
        [LutField(Position = 11)]
        public string ItemNumber;
        [LutField(Position = 12)]
        public bool VariableWeightItem;
        [LutField(Position = 13)]
        public double MinimumWeightAllowed;
        [LutField(Position = 14)]
        public double MaximumWeightAllowed;
        [LutField(Position = 15)]
        public int QuantityPicked;
        [LutField(Position = 16)]
        public string CheckDigits;
        [LutField(Position = 17)]
        public string ScanProductID;
        [LutField(Position = 18)]
        public string SpokenProductID;
        [LutField(Position = 19)]
        public string ItemDescription;
        [LutField(Position = 20)]
        public string ItemSize;
        [LutField(Position = 21)]
        public string ItemUPC;
        [LutField(Position = 22)]
        public long AssignmentID;
        [LutField(Position = 23)]
        public long? IDDescription;
        [LutField(Position = 24)]
        public string DeliveryLocation;
        [LutField(Position = 25)]
        public string DummyField;
        [LutField(Position = 26)]
        public string CustomerNumber;
        [LutField(Position = 27)]
        public string CaseLabelCheckDigit;
        [LutField(Position = 28)]
        public int? TargetContainer;
        [LutField(Position = 29)]
        public bool CaptureLotFlag;
        [LutField(Position = 30)]
        public string CaptureLotText;
        [LutField(Position = 31)]
        public string PromptMessage;
        [LutField(Position = 32)]
        public bool VerifyLocationFlag;
        [LutField(Position = 33)]
        public bool CycleCount;
        [LutField(Position = 34)]
        public bool SerialNumber;
        [LutField(Position = 35)]
        public bool MultipleItemLocation;

        [LutField(Position = 36, IsErrorCode = true)]
        public int ErrorCode;
        [LutField(Position = 37, IsErrorMessage = true)]
        public string ErrorMessage;

        public bool Matches(Pick pickToMatch, bool combineAssignments = true)
        {
            if (LocationId == pickToMatch.LocationId &&
                UOM == pickToMatch.UOM &&
                ItemNumber == pickToMatch.ItemNumber &&
                TargetContainer == pickToMatch.TargetContainer &&
                (combineAssignments || AssignmentID == pickToMatch.AssignmentID))
            {
                return true;
            }
            return false;
        }
    }

    public class PickLUT : LUTResponse<Pick>
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:VoiceLink.PickLUT"/> class.
        /// </summary>
        /// <param name="response">Response.</param>
        public PickLUT(string response) : base(response)
        {
        }
    }

    /// <summary> 
    /// Class to hold parameters to execute Lut. 
    /// </summary>
    public class LutParameter
    {
        /// <summary> A list of error codes to ignore generic error handling process. </summary>
        public List<int> IgnoreErrors { get; set; }
    }

    /// <summary> 
    /// Class to hold parameters to execute ExecutePickLut. 
    /// </summary>
    public class ExecutePickParam : LutParameter
    {
        public ExecutePickParam(Assignment currentAssigment, Pick pick, int quantity, bool complete, long? containerId, string lotNumber, double? weight, string serialNumber)
        {
            CurrentAssigment = currentAssigment;
            Pick = pick;
            Quantity = quantity;
            Complete = complete;
            ContainerId = containerId;
            LotNumber = lotNumber;
            Weight = weight;
            SerialNumber = serialNumber;
        }

        /// <summary> Parameter for ExecutePick Lut. </summary>
        public Assignment CurrentAssigment { get; set; }

        /// <summary> Parameter for ExecutePick Lut. </summary>
        public Pick Pick { get; set; }

        /// <summary> Parameter for ExecutePick Lut. </summary>
        public int Quantity { get; set; }

        /// <summary> Parameter for ExecutePick Lut. </summary>
        public bool Complete { get; set; }

        /// <summary> Parameter for ExecutePick Lut. </summary>
        public long? ContainerId { get; set; }

        /// <summary> Parameter for ExecutePick Lut. </summary>
        public string LotNumber { get; set; }

        /// <summary> Parameter for ExecutePick Lut. </summary>
        public double? Weight { get; set; }

        /// <summary> Parameter for ExecutePick Lut. </summary>
        public string SerialNumber { get; set; }
    }

    /// <summary> 
    /// Class to hold parameters to execute GetAssignmentsLut. 
    /// </summary>
    public class GetAssignmentsParam : LutParameter
    {
        public GetAssignmentsParam(int assignmentsToRequest)
        {
            AssignmentsToRequest = assignmentsToRequest;
        }

        /// <summary> Parameter for GetAssignments Lut. </summary>
        public int AssignmentsToRequest { get; set; }
    }

    /// <summary> 
    /// Class to hold parameters to execute GetContainersLut. 
    /// </summary>
    public class GetContainersParam : LutParameter
    {
        public GetContainersParam(Assignment assignment, string targetContainer, long? pickContainerId, string containerNumber, int operation, string labels)
        {
            Assignment = assignment;
            TargetContainer = targetContainer;
            PickContainerId = pickContainerId;
            ContainerNumber = containerNumber;
            Operation = operation;
            Labels = labels;
        }

        /// <summary> Parameter for GetContainers Lut. </summary>
        public Assignment Assignment { get; set; }

        /// <summary> Parameter for GetContainers Lut. </summary>
        public string TargetContainer { get; set; }

        /// <summary> Parameter for GetContainers Lut. </summary>
        public long? PickContainerId { get; set; }

        /// <summary> Parameter for GetContainers Lut. </summary>
        public string ContainerNumber { get; set; }

        /// <summary> Parameter for GetContainers Lut. </summary>
        public int Operation { get; set; }

        /// <summary> Parameter for GetContainers Lut. </summary>
        public string Labels { get; set; }
    }

    /// <summary> 
    /// Class to hold parameters to execute ReplenishmentLut. 
    /// </summary>
    public class ReplenishmentParam : LutParameter
    {
        public ReplenishmentParam(Pick pick)
        {
            Pick = pick;
        }

        /// <summary> Parameter for Replenishment Lut. </summary>
        public Pick Pick { get; set; }
    }

    /// <summary> 
    /// Class to hold parameters to execute UpdateStatusLut. 
    /// </summary>
    public class UpdateStatusParam : LutParameter
    {
        public UpdateStatusParam(long? location, string scope, string status)
        {
            Location = location;
            Scope = scope;
            Status = status;
        }

        /// <summary> Parameter for UpdateStatus Lut. </summary>
        public long? Location { get; set; }

        /// <summary> Parameter for UpdateStatus Lut. </summary>
        public string Scope { get; set; }

        /// <summary> Parameter for UpdateStatus Lut. </summary>
        public string Status { get; set; }
    }
    #endregion

    public abstract class ServerResponses<T>
    {
        public static T CurrentResponse { get; protected set; }

        public static void UpdateCurrentResponse(T response)
        {
            CurrentResponse = response;
        }
    }

    public class GenericLUTResponse : ServerResponses<GenericResponseLUT>
    {
        public static int ErrorCode
        {
            get
            {
                if (CurrentResponse != null && CurrentResponse.Count > 0)
                {
                    return CurrentResponse[0].ErrorCode;
                }
                return 0;
            }
        }
    }

    public class FunctionsResponse : ServerResponses<FunctionLUT>
    {
        public static int ValidFunctionCount
        {
            get
            {
                int result = 0;
                if (CurrentResponse != null && CurrentResponse.Count > 0)
                {
                    result = CurrentResponse.Where(f => f.Number > 0).Count();
                }
                return result;
            }
        }
    }

    public class ContainersResponse : ServerResponses<ContainerLUT>
    {

        public static IEnumerable<Container> GetOpenContainers(long assignmentId)
        {
            if (CurrentResponse != null)
            {
                return CurrentResponse.Where(c => c.AssignmentID == assignmentId && c.ContainerStatus == "O");
            }
            return new List<Container>();
        }

        public static IEnumerable<Container> GetClosedContainers(long assignmentId)
        {
            if (CurrentResponse != null)
            {
                return CurrentResponse.Where(c => c.AssignmentID == assignmentId && c.ContainerStatus == "C");
            }
            return new List<Container>();
        }

        public static bool MultipleOpenContainers(long assignmentId)
        {
            if (CurrentResponse != null)
            {
                return CurrentResponse.Where(c => c.AssignmentID == assignmentId && c.ContainerStatus == "O").Count() > 1;
            }
            return false;
        }

        public static Container GetMatchingOpenContainer(string idToMatch, long assignmentId)
        {
            return CurrentResponse.Where(c => c.AssignmentID == assignmentId && c.ContainerStatus == "O" && (c.ScannedContainerValidation == idToMatch || c.SpokenContainerValidation == idToMatch)).FirstOrDefault();
        }

        public static Container GetMatchingContainer(string idToMatch)
        {
            return CurrentResponse.Where(c => c.ScannedContainerValidation == idToMatch || c.SpokenContainerValidation == idToMatch).FirstOrDefault();
        }

        public static bool HasContainersWithStatus(long assignmentId, string status)
        {
            if (CurrentResponse != null)
            {
                return CurrentResponse.Where(c => c.AssignmentID == assignmentId && c.ContainerStatus == status).Count() > 0;
            }
            return false;
        }

        public static bool ValidContainerForAssignment(long assignmentId, string idToMatch)
        {
            if (CurrentResponse != null)
            {
                return CurrentResponse.Where(c => c.AssignmentID == assignmentId && (c.ScannedContainerValidation == idToMatch || c.SpokenContainerValidation == idToMatch)).FirstOrDefault() != null;
            }
            return false;
        }
    }

    public class RegionsResponse : ServerResponses<RegionLUT>
    {
    }

    public class PickingRegionsResponse : ServerResponses<PickingRegionLUT>
    {
        public static int CurrentPickingRegionIdx { get; private set; } = 0;

        public static void ResetCurrentPickingRegionIndex()
        {
            CurrentPickingRegionIdx = 0;
        }

        public static void IncrementCurrentPickingRegionIndex()
        {
            CurrentPickingRegionIdx++;
        }

        public static PickingRegion CurrentPickingRegion => CurrentResponse[CurrentPickingRegionIdx];
    }

    public class AssignmentsResponse : ServerResponses<AssignmentLUT>
    {
        public static int NumberOfAssignments()
        {
            return CurrentResponse.Where(a => a.GroupID != null).Count();
        }

        public static bool HasMultipleAssignments()
        {
            return NumberOfAssignments() > 1;
        }

        public static Assignment FirstAssignment()
        {
            return CurrentResponse[0];
        }
    }

    public class PicksResponse : ServerResponses<PickLUT>
    {
        public static bool HasAnyShorts()
        {
            if (CurrentResponse != null)
            {
                return CurrentResponse.Where(p => (p.QuantityToPick - p.QuantityPicked) > 0 && p.Status == "P").Count() > 0;
            }
            return false;
        }

        public static bool HasPicksWithStatus(string status)
        {
            if (CurrentResponse != null)
            {
                return CurrentResponse.Where(p => p.Status == status).Count() > 0;
            }
            return false;
        }

        public static void ChangeStatus(string toStatus, string fromStatus)
        {
            foreach (var pick in CurrentResponse)
            {
                if (pick.Status.Length > 0)
                {
                    if (fromStatus == null || pick.Status == fromStatus)
                    {
                        pick.Status = toStatus;
                    }
                }
            }
        }

        public static bool HasPicks(Assignment assignment, List<string> status)
        {
            foreach (var pick in CurrentResponse)
            {
                if ((assignment == null || assignment.AssignmentID == pick.AssignmentID) && status.Contains(pick.Status))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class VerifyReplenishmentResponse : ServerResponses<VerifyReplenishmentLUT>
    {
        public static bool Replenished
        {
            get
            {
                if (CurrentResponse != null && CurrentResponse.Count > 0)
                {
                    return CurrentResponse[0].ErrorCode == 0 && CurrentResponse[0].Replenished;
                }
                return true;
            }
        }
    }

    public class RequestWorkResponse : ServerResponses<RequestWorkLUT>
    {
        public static int ErrorCode
        {
            get
            {
                if (CurrentResponse != null && CurrentResponse.Count > 0)
                {
                    return CurrentResponse[0].ErrorCode;
                }
                return 0;
            }
        }
    }
}