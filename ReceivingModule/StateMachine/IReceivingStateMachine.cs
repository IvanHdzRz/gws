//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System.Threading.Tasks;

    public enum State
    {
        Start,
        BackgroundActvity,
        SignOut,
        StartSignIn,
        DisplaySignIn,
        SignIn,
        StartOperPrep,
        RetrieveOrders,
        DisplayOrders,
        VerifyOrder,
        DisplayHiQuantity,
        VerifyHiQuantity,
        DisplayTiQuantity,
        VerifyTiQuantity,
        DisplayConfirmQuantity,
        CheckQuantityConfirmation,
        DisplayPrintingLabel,
        HandleLabelPrinter,
        DisplayApplyLabel,
        HandleApplyLabel,
        DisplayPalletCondition,
        CheckPalletCondition,
        DisplayDamagedReason,
        HandleDamagedReason,
        DisplayInvoiceSummary,
        HandleInvoiceSummary
    }

    public enum Trigger
    {
        ExecuteBackgroundActivity,
        DataRequestFailed,
        DataRequestSucceeded,
        ValidEntry,
        InvalidEntry,
        Ready,
        Cancel,
        AffirmativeConfirmation,
        NegativeConfirmation,
        GoodCondition,
        DamagedCondition,
        NavigateBack,
        WaitForUserInput,
        ReturnUserInput

    }

    public interface IReceivingStateMachine
    {
        State CurrentState { get; }
        void InitializeStateMachine();
        Task ExecuteStateAsync();
    }
}
