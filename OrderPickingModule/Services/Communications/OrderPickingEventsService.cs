//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using Honeywell.Firebird.CoreLibrary;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using RetailEvents;
    using System.Linq;
    using Retail;

    public class OrderPickingEventsService : RetailAppEventsService, IOrderPickingEventsService
    {
        IOrderPickingDataService _OrderPickingDataService;
        private readonly IOrderPickingModel _OrderPickingModel;
        private bool _AssignmentInProgress = false;
        private bool _SubstitutionInProgress = false;

        public OrderPickingEventsService(IRetailEventsRESTServiceProvider restServiceProvider,
                                          GuidedWork.IDeviceInfo deviceInfo,
                                          IServerConfigRepository serverConfigRepo,
                                          IRetailConfigRepository retailConfigRepo,
                                          IOrderPickingModel orderPickingModel,
                                          IEventsServiceModel eventsServiceModel,
                                          IOrderPickingDataService orderPickingDataService)
            : base(restServiceProvider, deviceInfo, serverConfigRepo, retailConfigRepo, eventsServiceModel)
        {
            _OrderPickingDataService = orderPickingDataService;
            _OrderPickingModel = orderPickingModel;
        }

        public Task StartAssignmentEvent()
        {
            WorkflowType workflowType;

            if (_OrderPickingModel.ProcessingSubstitution)
            {
                if (_SubstitutionInProgress)
                {
                    return Task.CompletedTask;
                }

                _SubstitutionInProgress = true;
                workflowType = WorkflowType.OrderPickSub;
            }
            else
            {
                if (_AssignmentInProgress)
                {
                    return Task.CompletedTask;
                }

                _AssignmentInProgress = true;
                workflowType = WorkflowType.OrderPick;
            }
            
            return StartAssignmentEvent(workflowType, _OrderPickingModel.CurrentOrderPickingWorkItem?.ID.ToString(), _OrderAndProductProperties);
        }

        public Task StopAssignmentEvent()
        {
            WorkflowType workflowType;

            if (_OrderPickingModel.ProcessingSubstitution)
            {
                _SubstitutionInProgress = false;
                workflowType = WorkflowType.OrderPickSub;
            }
            else
            {
                _AssignmentInProgress = false;
                workflowType = WorkflowType.OrderPick;
            }

            return StopAssignmentEvent(workflowType, _OrderPickingModel.CurrentOrderPickingWorkItem?.ID.ToString(), _OrderAndProductProperties);
        }

        public Task SendSkipItemEvent()
        {
            return SendEventAsync(EventType.Assignment, EventStatus.SkipItem, _OrderAndProductProperties);
        }

        public Task SendOverflowExceptionEvent()
        {
            return SendEventAsync(EventType.Assignment, EventStatus.Overflow, _OrderAndProductProperties);
        }

        /// <summary>
        /// Sends an event asynchronously.
        /// </summary>
        /// <returns>The associated Task.</returns>
        /// <param name="type">The event type.</param>
        /// <param name="status">The event status.</param>
        /// <param name="additionalProperties">Additional properties.</param>
        private Task SendEventAsync(EventType type, EventStatus status, IReadOnlyDictionary<string, string> additionalProperties)
        {
            return SendEventAsync(type, status, WorkflowType.OrderPick, _OrderPickingModel.CurrentOrderPickingWorkItem?.ID.ToString(), additionalProperties);
        }

        private Task SendSubstitutionEventAsync(EventType type, EventStatus status, IReadOnlyDictionary<string, string> additionalProperties)
        {
            return SendEventAsync(type, status, WorkflowType.OrderPickSub, _OrderPickingModel.CurrentOrderPickingWorkItem?.ID.ToString(), additionalProperties);
        }

        private IReadOnlyDictionary<string, string> _OrderAndProductProperties
        {
            get
            {
                var currentLocation = _OrderPickingModel.GetLocationDescriptorsForProduct();
                return new Dictionary<string, string>
                {
                    [RetailEventKeys.OrderId] = _OrderPickingModel.OrderPickingOrderInfo?.ID.ToString(),
                    [RetailEventKeys.OrderIdentifier] = _OrderPickingModel.OrderPickingOrderInfo?.OrderIdentifier?.ToString(),
                    [RetailEventKeys.LocationIdKey] = currentLocation.FirstOrDefault()?.LocationID.ToString(),
                    [RetailEventKeys.LocationKey] = GetCurrentLocationString(),
                    [RetailEventKeys.ProductIdKey] = _OrderPickingModel.CurrentProduct.ID.ToString(),
                    [RetailEventKeys.ProductIdentifierKey] = _OrderPickingModel.CurrentProduct.ProductIdentifier,
                    [RetailEventKeys.RequestedQuantity] = _OrderPickingModel.CurrentOrderPickingWorkItem.Quantity.ToString(),
                    [RetailEventKeys.PickedQuantity] = (_OrderPickingModel.CurrentOrderPickingWorkItem.Quantity - _OrderPickingModel.RemainingQuantity).ToString(),
                    [RetailEventKeys.OrderedProductId] = _OrderPickingModel.CurrentOrderPickingWorkItem.ProductID.ToString(),
                    [RetailEventKeys.OrderedProductIdentifier] = _OrderPickingDataService.GetProduct(_OrderPickingModel.CurrentOrderPickingWorkItem.ProductID).ProductIdentifier
                };
            }
        }


        private string GetCurrentLocationString()
        {
            var locationDescriptors = new List<string>();
            foreach (var locationDescriptor in _OrderPickingModel.GetLocationDescriptorsForProduct())
            {
                locationDescriptors.Add($"{locationDescriptor.Name} {locationDescriptor.Value}");
            }
            return string.Join(", ", locationDescriptors);
        }
    }
}
