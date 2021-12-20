using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace BasePickingExample
{
    /// <summary>
    /// BasePickingExample REST service provider
    /// </summary>
    public interface IBasePickingExampleRESTServiceProvider
    {
        Task<SignOnResponse> SignOnAsync(string operatorIdenitifier, string password, CancellationToken cancellationToken = default(CancellationToken));
        Task<GetPicksResponse> GetPicksAsync(string operatorIdentifier, CancellationToken cancellationToken = default(CancellationToken));
        Task<UpdatePickResponse> UpdatePickAsync(long pickId, int quantityPicked, CancellationToken cancellationToken = default(CancellationToken));
        Task<SignOffResponse> SignOffAsync(string operatorIdentifier, CancellationToken cancellationToken = default(CancellationToken));
        Task<GetContainerResponce> GetContainersById(string operatorId, string containerId, CancellationToken cancellationToken = default(CancellationToken));
        Task<OpenContainerResponce> OpenContainer(string operatorId, string containerId, CancellationToken cancellationToken = default(CancellationToken));
    }

    public class SignOnRequest
    {
        public string OperatorId;
        public string Password;
    }

    public class SignOnResponse
    {
        public string Status;
    }

    public class GetPicksRequest
    {
        public string OperatorId;
    }

    public class GetPicksResponse : List<Pick>
    {
    }

    public class Pick
    {
        public long PickId;
        public string CheckDigits;
        public string Aisle;
        public string Slot;
        public int QuantityToPick;
        public int ContainerPosition;
        public string ProductName;
        public string ProductScannedVerification;
        public string ProductSpokenVerification;
        public string ContainerSpokenVerification;
        public string ContainerScannedVerification;
        public int QuantityPicked;
        public bool Picked;
    }
    public class UpdatePickRequest
    {
        public long PickId;
        public int QuantityPicked;
    }

    public class UpdatePickResponse
    {
        public string status;
    }

    public class SignOffRequest
    {
        public string OperatorId;
    }

    public class SignOffResponse
    {
        public string Status;
    }
    //models for Containers
    public class Container { 
        public string ContainerId { get; set; }
        public string Status { get; set; }
        public string OperatorId { get; set; }
        public string AssignmentId { get; set; }
    }
    public class GetContainerResponce : List<Container> { }
    public class GetContainerRequest { 
        public string OperatorId { get; set; }
        public string ContainerId { get; set; }
    }
    public class OpenContainerRequest
    {
        public string assignmentId { get; set; }
        public string containerId { get; set; }
    }
    public class OpenContainerResponce { 
        public string status { get; set; }
    }
}
