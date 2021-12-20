//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Threading.Tasks;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Retail;
    using TinyIoC;

    public class OrderPickingStateInternals
    {
        private readonly IOrderPickingModel _Model;
        private readonly IProductRequestModel _ProductRequestModel;
        private readonly IRetailAcuityEventService _RetailAcuityEventService;

        public OrderPickingStateInternals(IOrderPickingModel model,
            IRetailAcuityEventService retailAcuityEventService,
            IProductRequestModel productRequestModel)
        {
            _Model = model;
            _ProductRequestModel = productRequestModel;
            _RetailAcuityEventService = retailAcuityEventService;
        }

        public Trigger NextTrigger { get; set; }
        public State LastState { get; set; }

        public Task SignOutAsync()
        {
            NextTrigger = Trigger.DataRequestSucceeded;
            return Task.CompletedTask;
        }

        public async Task RetrievePicksAsync()
        {
            NextTrigger = Trigger.DataRequestFailed;

            await _Model.GetPicksAndAssociateContainersAsync();

            if (_Model.CurrentOrderPickingWorkItem != null)
            {
                NextTrigger = Trigger.DataRequestSucceeded;
            }
        }

        public Task HandleGetContainers()
        {
            string response = _Model.GetContainersResponse;
            LastState = State.DisplayGetContainers;

            if (response == "NavigateBack")
            {
                NextTrigger = Trigger.NavigateBack;
                return Task.CompletedTask;
            }
            else if (response == Translate.GetLocalizedTextForKey("accept_entry_word") ||
                     response == Translate.GetLocalizedTextForKey("next_entry_word"))
            {
                NextTrigger = Trigger.Ready;
            }
            else if (response == Translate.GetLocalizedTextForKey("VocabWord_EndOrder"))
            {
                NextTrigger = Trigger.Cancel;
            }
            return Task.CompletedTask;
        }

        public Task SetSelectedProductAsync()
        {
            _Model.SetRequestedProduct();
            NextTrigger = Trigger.NoSubstitution;

            _RetailAcuityEventService.SendTravelStartEventAsync(_Model.CurrentProduct.ID);
            var orderPickingEventsService = TinyIoCContainer.Current.Resolve<IOrderPickingEventsService>();
            orderPickingEventsService.StartAssignmentEvent();

            return Task.CompletedTask;
        }

        public Task HandleAcknowledgeLocationAsync()
        {
            LastState = State.DisplayAcknowledgeLocation;

            if (_Model.AcknowledgeLocationResponse == "NavigateBack")
            {
                NextTrigger = Trigger.NavigateBack;
            }
            else if (_Model.AcknowledgeLocationResponse == Translate.GetLocalizedTextForKey("VocabWord_SkipProduct"))
            {
                // set as skipped in model?
                NextTrigger = Trigger.SkipProduct;
            }
            else if (_Model.AcknowledgeLocationResponse == Translate.GetLocalizedTextForKey("accept_entry_word") ||
                     _Model.AcknowledgeLocationResponse == Translate.GetLocalizedTextForKey("next_entry_word"))
            {
                _Model.SetWorkItemInProgress();
                _RetailAcuityEventService.SendTravelStopEventAsync(_Model.CurrentProduct.ID);
                NextTrigger = Trigger.Ready;
            }
            else if (_Model.AcknowledgeLocationResponse == Translate.GetLocalizedTextForKey("VocabWord_EndOrder") ||
                     _Model.AcknowledgeLocationResponse == Translate.GetLocalizedTextForKey("VocabWord_NoMore"))
            {
                NextTrigger = Trigger.Cancel;
            }
            else if (_Model.AcknowledgeLocationResponse == Translate.GetLocalizedTextForKey("VocabWord_OrderStatus"))
            {
                NextTrigger = Trigger.OrderStatus;
            }

            return Task.CompletedTask;
        }

        public Task VerifyProductAsync()
        {
            if (_Model.EnteredProduct == "NavigateBack")
            {
                NextTrigger = Trigger.NavigateBack;
            }
            else if (_Model.EnteredProduct == Translate.GetLocalizedTextForKey("VocabWord_SkipProduct"))
            {
                LastState = State.DisplayEnterProduct;
                NextTrigger = Trigger.SkipProduct;
            }
            else if (_Model.EnteredProduct == Translate.GetLocalizedTextForKey("VocabWord_NoMore"))
            {
                LastState = State.DisplayEnterProduct;
                NextTrigger = Trigger.Cancel;
            }
            else if (_Model.EnteredProduct == Translate.GetLocalizedTextForKey("VocabWord_OrderStatus"))
            {
                LastState = State.DisplayEnterProduct;
                NextTrigger = Trigger.OrderStatus;
            }
            else if (_Model.DataStore.IsValidIdentifier(_Model.EnteredProduct))
            {
                _Model.StockCodeResponse = _Model.EnteredProduct;
                NextTrigger = Trigger.ValidEntry;
            }
            else
            {
                NextTrigger = Trigger.InvalidEntry;
            }

            return Task.CompletedTask;
        }

        public Task VerifyQuantityAsync()
        {
            if (_Model.EnteredQuantityString == "NavigateBack")
            {
                NextTrigger = Trigger.NavigateBack;
                return Task.CompletedTask;
            }

            int expectedQuantity = _Model.DataStore.RemainingQuantity;

            if (_Model.EnteredQuantityString == Translate.GetLocalizedTextForKey("VocabWord_Overflow"))
            {
                LastState = State.DisplayEnterQuantity;
                NextTrigger = Trigger.Overflow;
                return Task.CompletedTask;
            }
            else if (_Model.EnteredQuantityString == Translate.GetLocalizedTextForKey("VocabWord_OrderStatus"))
            {
                LastState = State.DisplayEnterQuantity;
                NextTrigger = Trigger.OrderStatus;
                return Task.CompletedTask;
            }

            _Model.EnteredQuantity = int.Parse(_Model.EnteredQuantityString);

            if (_Model.EnteredQuantity > expectedQuantity)
            {
                NextTrigger = Trigger.QuantityGreater;
            }
            else if (_Model.EnteredQuantity == expectedQuantity)
            {
                NextTrigger = Trigger.QuantityMatches;
            }
            else
            {                
                NextTrigger = Trigger.QuantityLess;
            }

            return Task.CompletedTask;
        }

        public Task UpdateQuantityAsync()
        {
            NextTrigger = Trigger.DataRequestFailed;

            var orderPickingEventsService = TinyIoCContainer.Current.Resolve<IOrderPickingEventsService>();
            orderPickingEventsService.StopAssignmentEvent();

            if (_Model.EnteredQuantity > 0)
            {
                _Model.CurrentPickingContainer.ContainsProduct = true;
            }
            
            _Model.GeneratePickedQuantityRecordAsync(_Model.QuantityLastPicked);
            _Model.GeneratePickedQuantityRecordAsync(_Model.EnteredQuantity);
            _Model.UpdateRemainingQuantity(_Model.EnteredQuantity);
            _Model.QuantityLastPicked = _Model.EnteredQuantity;             

            if (!(_Model.RemainingQuantity > 0 && _Model.SubstitutionsAvailable))
            {
                _Model.SetWorkItemCompleteAsync();
            }

            if (_Model.RequestFillOnShort && _Model.RemainingQuantity > 0)
            {
                _ProductRequestModel.CreateAndStoreProductRequestAsync(_Model.CurrentProduct.ProductIdentifier, ProductRequestResolutionReason.Fill);
            }  

            NextTrigger = Trigger.DataRequestSucceeded;

            return Task.CompletedTask;
        }

        public Task HandleConfirmQuantityAsync() {

            bool response = _Model.ProductQuantityConfirmation;

            if (response)
            {
                NextTrigger = Trigger.AffirmativeConfirmation;
            }
            else
            {
                NextTrigger = Trigger.NegativeConfirmation;
            }

            return Task.CompletedTask;
        }

        public Task CheckForSubstitution()
        {
            if (_Model.RemainingQuantity > 0 && _Model.SubstitutionsAvailable)
            {
                _Model.SetSubstitutedProduct();
                NextTrigger = Trigger.Substitution;
            }
            else
            {
                NextTrigger = Trigger.NoSubstitution;
            }

            return Task.CompletedTask;
        }

        public Task HandleConfirmOverflow()
        {
            if (_Model.OverflowConfirmation)
            {
                var orderPickingEventsService = TinyIoCContainer.Current.Resolve<IOrderPickingEventsService>();
                orderPickingEventsService.SendOverflowExceptionEvent();
                _Model.HandleOverflow();
            }

            NextTrigger = Trigger.DataRequestSucceeded;
            
            return Task.CompletedTask;
        }

        public Task CheckForMoreWorkAsync()
        {
            if (_Model.RemainingQuantity > 0 && _Model.SubstitutionsAvailable || _Model.MoreWorkItems)
            {
                NextTrigger = Trigger.MoreWork;
            }
            else
            {
                NextTrigger = Trigger.NoMoreWork;
            }

            return Task.CompletedTask;
        }

        public Task CheckNoMoreConfirmationAsync()
        {
            if (_Model.NoMoreConfirmation)
            {
                if (_Model.CurrentStagingContainer != null)
                {
                    NextTrigger = Trigger.Staging;
                }
                else
                {
                    NextTrigger = Trigger.EndWorkflow;
                }
            }
            else
            {
                NextTrigger = Trigger.NegativeConfirmation;
            }

            return Task.CompletedTask;
        }

        public Task CheckSkipProductConfirmationAsync()
        {
            NextTrigger = _Model.SkipProductConfirmation ? Trigger.AffirmativeConfirmation : Trigger.NegativeConfirmation;

            return Task.CompletedTask;
        }

        public Task SkipProductAsync()
        {
            _Model.SetWorkItemSkipped();

            var orderPickingEventsService = TinyIoCContainer.Current.Resolve<IOrderPickingEventsService>();
            orderPickingEventsService.SendSkipItemEvent();

            NextTrigger = Trigger.DataRequestSucceeded;

            return Task.CompletedTask;
        }

        public void HandleNoMoreWork()
        {
            if (_Model.CurrentStagingContainer != null)
            {
                NextTrigger = Trigger.Staging;
            }
            else
            {
                NextTrigger = Trigger.EndWorkflow;
            }
        }

        public Task HandlePickOrderStatusAsync()
        {
            NextTrigger = Trigger.Ready;
            return Task.CompletedTask;
        }

        public Task HandleGoToStagingAsync()
        {
            string response = _Model.GoToStagingResponse;

            if (response == Translate.GetLocalizedTextForKey("accept_entry_word") ||
                        response == Translate.GetLocalizedTextForKey("next_entry_word"))
            {
                NextTrigger = Trigger.Ready;
            }

            return Task.CompletedTask;
        }

        public Task HandleEnterStagingLocationAsync()
        {
            if (_Model.StagingLocationResponse == "NavigateBack")
            {
                NextTrigger = Trigger.NavigateBack;
                return Task.CompletedTask;
            }

            _Model.StagingLocation = _Model.StagingLocationResponse;

            LastState = State.DisplayEnterStagingLocation;
            NextTrigger = Trigger.ValidEntry;

            return Task.CompletedTask;
        }

        public Task HandleConfirmStagingLocationAsync()
        {
            bool response = _Model.StagingLocationConfirmation;

            if (response)
            {
                var stagedContainer = _Model.CurrentStagingContainer;
                stagedContainer.StagingLocation = _Model.StagingLocation;
                _Model.StoreStagingLocationAsync(stagedContainer.OrderId, stagedContainer.StagingLocation);

                if (_Model.CurrentStagingContainer != null)
                {
                    NextTrigger = Trigger.Staging;
                }
                else
                {
                    NextTrigger = Trigger.EndWorkflow;
                }
            }
            else
            {
                NextTrigger = Trigger.NavigateBack;
            }

            return Task.CompletedTask;
        }

        public Task HandleRevertPickingActions()
        {
            if (!_Model.ProcessingSubstitution)
            {
                LastState = State.DisplayGetContainers;
                NextTrigger = Trigger.AcknowledgeLocation;
            }
            else if (_Model.RevertPickingActions())
            {
                NextTrigger = Trigger.EnterSubProduct;
            }
            else
            {
                LastState = State.DisplayAcknowledgeLocation;
                NextTrigger = Trigger.EnterProduct;
            }
            
            return Task.CompletedTask;
        }
    }
}
