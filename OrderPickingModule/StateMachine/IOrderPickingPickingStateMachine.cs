//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Threading.Tasks;

    public enum State
    {
        Start,
        RequestData,
        BackgroundActvity,
        SignOut,
        RetrievePicks,
        DisplayGetContainers,
        HandleGetContainers,
        SetSelectedProduct,
        CheckIfSubstitution,
        DisplayAcknowledgeLocation,
        HandleAcknowledgeLocation,
        DisplayEnterProduct,
        VerifyProduct,
        DisplayEnterSubProduct,
        DisplayEnterQuantity,
        VerifyQuantity,
        UpdateQuantity,
        DisplayConfirmOverflow,
        HandleConfirmOverflow,
        CheckForMoreWork,
        SetOrderComplete,
        DisplayConfirmQuantity,
        CheckForSubstitution,
        DisplayConfirmNoMore,
        CheckNoMoreConfirmation,
        DisplayAllDone,
        DisplayConfirmSkipProduct,
        CheckSkipProductConfirmation,
        SkipProduct,
        DisplayPickOrderStatus,
        HandlePickOrderStatus,
        DisplayGoToStagingLocation,
        HandleGoToStaging,
        DisplayEnterStagingLocation,
        DisplayConfirmStagingLocation,
        HandleConfirmStagingLocation,
        HandleEnterStagingLocation,
        HandleRevertPick,
        HandleConfirmQuantity,
        HandleNoMoreWork
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
        Substitution,
        NoSubstitution,
        MoreWork,
        NoMoreWork,
        SkipProduct,
        OrderStatus,
        Overflow,
        NavigateBack,
        WaitForUserInput,
        ReturnUserInput,
        Staging,
        EndWorkflow,
        EnterProduct,
        EnterSubProduct,
        AcknowledgeLocation
    }

    public interface IOrderPickingStateMachine
    {
        State CurrentState { get; }
        void InitializeStateMachine();
        Task ExecuteStateAsync();
    }
}
