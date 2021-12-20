//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Threading.Tasks;
    using TinyIoC;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using GuidedWork;

    public class WarehousePickingStateInternals
    {
        private readonly IWarehousePickingModel _Model;
        private readonly IWarehousePickingActivityTracker _WarehousePickingActivityTracker;
        private string _LastAisle = string.Empty;

        public WarehousePickingStateInternals(IWarehousePickingModel model, IWarehousePickingActivityTracker warehousePickingActivityTracker)
        {
            _Model = model;
            _WarehousePickingActivityTracker = warehousePickingActivityTracker;
        }

        public Trigger NextTrigger { get; set; }
        public State LastState { get; set; }

        public Task SignOutAsync()
        {
            NextTrigger = Trigger.DataRequestSucceeded;
            return Task.CompletedTask;
        }

        public Task StartSignInAsync()
        {
            _Model.CurrentUserMessage = string.Empty;
            NextTrigger = Trigger.Ready;
            return Task.CompletedTask;
        }

        public async Task SignInAsync()
        {
            NextTrigger = Trigger.DataRequestFailed;

            bool validUserCredentials = await _Model.ValidateUserCredentialsAsync();

            if (validUserCredentials)
            {
                TinyIoCContainer.Current.Resolve<IGuidedWorkTemplateService>().ResetWorker(_Model.User.Id, _Model.User.Id);
                NextTrigger = Trigger.ValidEntry;
            }
            else
            {
                _Model.CurrentUserMessage = "Authenication failed";
                NextTrigger = Trigger.InvalidEntry;
            }
        }

        public async Task RetrieveSubcentersAsync()
        {
            NextTrigger = Trigger.DataRequestFailed;

            await _Model.RetrieveSubcentersAsync();

            NextTrigger = Trigger.DataRequestSucceeded;
        }

        public Task HandleSubcenterAsync()
        {
            NextTrigger = Trigger.ValidEntry;

            if (_Model.SelectedSubcenter == "NavigateBack")
            {
                NextTrigger = Trigger.NavigateBack;
            }

            return Task.CompletedTask;
        }

        public Task VerifyLabelPrinterAsync()
        {
            NextTrigger = Trigger.ValidEntry;

            if (_Model.LabelPrinter == "NavigateBack")
            {
                NextTrigger = Trigger.NavigateBack;
            }

            return Task.CompletedTask;
        }

        public async Task RetrievePicksAsync()
        {
            NextTrigger = Trigger.DataRequestFailed;

            await _Model.RetrievePicksAsync();

            if (_Model.CurrentWarehousePickingWorkItem != null)
            {
                NextTrigger = Trigger.DataRequestSucceeded;
            }
        }

        public Task HandlePickTripInfoAsync()
        {
            if (_Model.AcknowledgePickTripInfo == "NavigateBack")
            {
                NextTrigger = Trigger.NavigateBack;
                return Task.CompletedTask; ;
            }

            _WarehousePickingActivityTracker.StartTrip();

            NextTrigger = Trigger.Ready;

            return Task.CompletedTask;
        }

        public Task SetSelectedProductAsync()
        {
            _Model.SetRequestedProduct();

            NextTrigger = Trigger.DataRequestSucceeded;

            return Task.CompletedTask;
        }

        public Task VerifyLocationAsync()
        {
            if (_Model.AcknowledgedLocation == "NavigateBack")
            {
                NextTrigger = Trigger.NavigateBack;
            }
            else if (_Model.AcknowledgedLocation == Translate.GetLocalizedTextForKey("VocabWord_SkipProduct"))
            {
                // set as skipped in model?
                LastState = State.DisplayAcknowledgeLocation;
                NextTrigger = Trigger.SkipProduct;
            }
            else if (_Model.AcknowledgedLocation == Translate.GetLocalizedTextForKey("accept_entry_word") ||
                     _Model.AcknowledgedLocation == Translate.GetLocalizedTextForKey("next_entry_word"))
            {
                _WarehousePickingActivityTracker.StartAisle(_Model.CurrentWarehousePickingWorkItem.Aisle);
                _Model.SetWorkItemInProgress();
                NextTrigger = Trigger.Ready;
            }
            else if (_Model.AcknowledgedLocation == Translate.GetLocalizedTextForKey("VocabWord_EndOrder") ||
                     _Model.AcknowledgedLocation == Translate.GetLocalizedTextForKey("VocabWord_NoMore"))
            {
                LastState = State.DisplayAcknowledgeLocation;
                NextTrigger = Trigger.Cancel;
            }
            else if (_Model.AcknowledgedLocation == Translate.GetLocalizedTextForKey("VocabWord_LastPick"))
            {
                LastState = State.DisplayAcknowledgeLocation;
                NextTrigger = Trigger.LastPick;
            }
            else if (_Model.AcknowledgedLocation == Translate.GetLocalizedTextForKey("VocabWord_OrderStatus"))
            {
                LastState = State.DisplayAcknowledgeLocation;
                NextTrigger = Trigger.OrderStatus;
            }

            return Task.CompletedTask;
        }

        public Task StartSlotTrackerAsync()
        {
            _WarehousePickingActivityTracker.StartSlot(_Model.CurrentWarehousePickingWorkItem.SlotID, _Model.CurrentWarehousePickingWorkItem.Aisle);
            NextTrigger = Trigger.Ready;

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
            else if (_Model.EnteredProduct == Translate.GetLocalizedTextForKey("VocabWord_LastPick"))
            {
                LastState = State.DisplayEnterProduct;
                NextTrigger = Trigger.LastPick;
            }
            else if (_Model.EnteredProduct == Translate.GetLocalizedTextForKey("VocabWord_OrderStatus"))
            {
                LastState = State.DisplayEnterProduct;
                NextTrigger = Trigger.OrderStatus;
            }
            else if (_Model.DataStore.ValidCheckDigit(_Model.EnteredProduct))
            {
                NextTrigger = Trigger.ValidEntry;
            }
            else
            {
                _Model.CurrentUserMessage = Translate.GetLocalizedTextForKey("WarehousePicking_WrongCheckdigit");
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

            _Model.GeneratePickedQuantityRecordAsync(_Model.EnteredQuantity);
            _Model.UpdateRemainingQuantity(_Model.EnteredQuantity);
            _WarehousePickingActivityTracker.EndSlot(_Model.CurrentWarehousePickingWorkItem.SlotID);

            _LastAisle = _Model.CurrentWarehousePickingWorkItem.Aisle;
            _Model.SetWorkItemCompleteAsync();

            NextTrigger = Trigger.DataRequestSucceeded;

            return Task.CompletedTask;
        }

        public Task CheckForMoreWorkAsync()
        {
            if (_Model.MoreWorkItems)
            {
                long productID = _Model.CurrentWarehousePickingWorkItem.ProductID;
                string currAisle = _Model.GetAisleForProductID(productID);

                if (_LastAisle == currAisle)
                {
                    _Model.SetRequestedProduct();
                    NextTrigger = Trigger.MoreWorkSameLocation;
                }
                else
                {
                    _WarehousePickingActivityTracker.EndAisle(_LastAisle);
                    NextTrigger = Trigger.MoreWork;
                }
            }
            else
            {
                _WarehousePickingActivityTracker.EndAisle(_LastAisle);
                _WarehousePickingActivityTracker.EndTrip();
                NextTrigger = Trigger.NoMoreWork;
            }

            return Task.CompletedTask;
        }

        public Task CheckShortProductConfirmationAsync()
        {
            NextTrigger = _Model.ShortProductConfirmation ? Trigger.AffirmativeConfirmation : Trigger.NegativeConfirmation;

            if (_Model.ShortProductConfirmation)
            {
                _Model.SetShortedIndicator();
            }

            return Task.CompletedTask;
        }

        public Task SetOrderCompleteAsync()
        {
            NextTrigger = Trigger.DataRequestFailed;

            //_Model.SetOrderComplete(_Model.CurrentOrder.OrderId);
            NextTrigger = Trigger.DataRequestSucceeded;

            return Task.CompletedTask;
        }

        public Task CheckNoMoreConfirmationAsync()
        {
            NextTrigger = _Model.NoMoreConfirmation ? Trigger.AffirmativeConfirmation : Trigger.NegativeConfirmation;

            return Task.CompletedTask;
        }

        public Task CheckSkipProductConfirmationAsync()
        {
            NextTrigger = _Model.SkipProductConfirmation ? Trigger.AffirmativeConfirmation : Trigger.NegativeConfirmation;

            return Task.CompletedTask;
        }

        public Task SkipProductAsync()
        {
            _WarehousePickingActivityTracker.EndSlot(_Model.CurrentWarehousePickingWorkItem.SlotID);

            _LastAisle = _Model.CurrentWarehousePickingWorkItem.Aisle;
            _Model.SetWorkItemSkipped();
            NextTrigger = Trigger.DataRequestSucceeded;

            return Task.CompletedTask;
        }

        public Task HandlePickOrderStatusAsync()
        {
            NextTrigger = Trigger.Ready;
            return Task.CompletedTask;
        }

        public Task HandlePickOrderSummaryAsync()
        {
            NextTrigger = Trigger.Ready;
            return Task.CompletedTask;
        }

        public Task HandlePickPerformanceAsync()
        {
            NextTrigger = Trigger.Ready;

            if (_Model.AcknowledgePickPerformance == "NavigateBack")
            {
                NextTrigger = Trigger.NavigateBack;
            }

            return Task.CompletedTask;
        }

        public Task HandleResetDataAsync()
        {
            NextTrigger = Trigger.Ready;

            _Model.ResetPicks();

            return Task.CompletedTask;
        }


        public Task HandleLastPickAsync()
        {
            NextTrigger = Trigger.Ready;

            if (_Model.AcknowledgeLastPick == "NavigateBack")
            {
                NextTrigger = Trigger.NavigateBack;
            }

            return Task.CompletedTask;
        }
    }
}
