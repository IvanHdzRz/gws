//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Threading.Tasks;
    using TinyIoC;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using GuidedWork;

    public class ReceivingStateInternals
    {
        private readonly IReceivingModel _Model;

        public ReceivingStateInternals(IReceivingModel model)
        {
            _Model = model;
        }

        public Trigger NextTrigger { get; set; }

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

        public async Task RetrieveOrdersAsync()
        {
            NextTrigger = Trigger.DataRequestFailed;

            await _Model.GetReceivingWorkItemsAsync();

            NextTrigger = Trigger.DataRequestSucceeded;
        }

        public Task VerifyOrderAsync()
        {
            NextTrigger = Trigger.InvalidEntry;

            if (_Model.SelectedOrder == "NavigateBack")
            {
                NextTrigger = Trigger.NavigateBack;
            }
            else if (_Model.SelectedOrder == Translate.GetLocalizedTextForKey("VocabWord_NoMore"))
            {
                NextTrigger = Trigger.Cancel;
            }
            else
            {
                var matchingOrders = _Model.RetrieveMatchingWorkItems(_Model.SelectedOrder);
                _Model.LastValidWorkItem = matchingOrders[0];

                NextTrigger = Trigger.ValidEntry;
            }

            return Task.CompletedTask;
        }

        public Task VerifyHiQuantityAsync()
        {
            NextTrigger = Trigger.InvalidEntry;

            if (_Model.EnteredHiQuantity == "NavigateBack")
            {
                NextTrigger = Trigger.NavigateBack;
            }
            else
            {
                _Model.HiQuantityLastReceived = int.Parse(_Model.EnteredHiQuantity);

                NextTrigger = Trigger.ValidEntry;
            }

            return Task.CompletedTask;
        }

        public Task VerifyTiQuantityAsync()
        {
            NextTrigger = Trigger.InvalidEntry;

            if (_Model.EnteredTiQuantity == "NavigateBack")
            {
                NextTrigger = Trigger.NavigateBack;
            }
            else
            {
                _Model.TiQuantityLastReceived = int.Parse(_Model.EnteredTiQuantity);
                _Model.QuantityLastReceived = _Model.HiQuantityLastReceived * _Model.TiQuantityLastReceived;

                NextTrigger = Trigger.ValidEntry;
            }

            return Task.CompletedTask;
        }

        public Task CheckQuantityConfirmationAsync()
        {
            NextTrigger = _Model.QuantityConfirmation ? Trigger.AffirmativeConfirmation : Trigger.NegativeConfirmation;

            if (_Model.QuantityConfirmation)
            {
                _Model.UpdateRemainingWorkItemQuantity(_Model.QuantityLastReceived);
            }

            return Task.CompletedTask;
        }

        public Task HandleApplyLabelAsync()
        {
            NextTrigger = Trigger.Ready;

            return Task.CompletedTask;
        }

        public Task CheckPalletConditionAsync()
        {
            NextTrigger = _Model.PalletCondition ? Trigger.GoodCondition : Trigger.DamagedCondition;

            return Task.CompletedTask;
        }

        public Task HandleDamagedReasonAsync()
        {
            NextTrigger = Trigger.Ready;

            if (_Model.DamageReason == "NavigateBack")
            {
                NextTrigger = Trigger.NavigateBack;
            }
            else
            {
                _Model.UpdateDamaged(true);
            }

            return Task.CompletedTask;
        }

        public Task HandleInvoiceSummaryAsync()
        {
            NextTrigger = Trigger.Ready;

            return Task.CompletedTask;
        }

    }
}
