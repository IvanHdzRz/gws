//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System.Threading.Tasks;

    public enum State
    {
        Start,
        BackgroundActvity,
        ResetData,
        SignOut,
        StartSignIn,
        DisplaySignIn,
        SignIn,
        StartOperPrep,
        RetrieveSubcenters,
        DisplaySubcenters,
        HandleSubcenter,
        DisplayLabelPrinter,
        VerifyLabelPrinter,
        RetrievePicks,
        DisplayPickTripInfo,
        HandlePickTripInfo,
        SetSelectedProduct,
        DisplayAcknowledgeLocation,
        VerifyLocation,
        StartSlotTracker,
        DisplayEnterProduct,
        VerifyProduct,
        DisplayEnterQuantity,
        VerifyQuantity,
        UpdateQuantity,
        CheckForMoreWork,
        SetOrderComplete,
        DisplayConfirmQuantity,
        CheckShortProductConfirmation,
        DisplayConfirmNoMore,
        CheckNoMoreConfirmation,
        DisplayConfirmSkipProduct,
        CheckSkipProductConfirmation,
        SkipProduct,
        DisplayPickOrderStatus,
        HandlePickOrderStatus,
        DisplayPickOrderSummary,
        HandlePickOrderSummary,
        DisplayPickPerformance,
        HandlePickPerformance,
        DisplayLastPick,
        HandleLastPick
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
        QuantityLess,
        QuantityMatches,
        QuantityGreater,
        AffirmativeConfirmation,
        NegativeConfirmation,
        MoreWork,
        MoreWorkSameLocation,
        NoMoreWork,
        SkipProduct,
        LastPick,
        OrderStatus,
        NavigateBack,
        WaitForUserInput,
        ReturnUserInput
    }

    public interface IWarehousePickingStateMachine
    {
        State CurrentState { get; }
        void InitializeStateMachine();
        Task ExecuteStateAsync();
    }
}
