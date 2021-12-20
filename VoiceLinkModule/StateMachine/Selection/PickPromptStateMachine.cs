//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using GuidedWork;
    using GuidedWorkRunner;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public abstract class PickPromptStateMachine : SimplifiedBaseBusinessLogic<IVoiceLinkModel, VoiceLinkStateMachine, IVoiceLinkConfigRepository>
    {
        public static readonly CoreAppSMState InitPickPromptData = new CoreAppSMState(nameof(InitPickPromptData));
        public static readonly CoreAppSMState CheckTargetContainer = new CoreAppSMState(nameof(CheckTargetContainer));
        public static readonly CoreAppSMState DisplayReopenContainer = new CoreAppSMState(nameof(DisplayReopenContainer));
        public static readonly CoreAppSMState SlotVerification = new CoreAppSMState(nameof(SlotVerification));
        public static readonly CoreAppSMState VerifyProductSlot = new CoreAppSMState(nameof(VerifyProductSlot));
        public static readonly CoreAppSMState DisplayIdenticalShortProduct = new CoreAppSMState(nameof(DisplayIdenticalShortProduct));
        public static readonly CoreAppSMState DisplayShortProduct = new CoreAppSMState(nameof(DisplayShortProduct));
        public static readonly CoreAppSMState HandleIdenticalShortProductResponse = new CoreAppSMState(nameof(HandleIdenticalShortProductResponse));
        public static readonly CoreAppSMState HandleShortProductResponse = new CoreAppSMState(nameof(HandleShortProductResponse));
        public static readonly CoreAppSMState CaseLabelCheckDigit = new CoreAppSMState(nameof(CaseLabelCheckDigit));
        public static readonly CoreAppSMState DisplayCaseLabelCheckDigit = new CoreAppSMState(nameof(DisplayCaseLabelCheckDigit));
        public static readonly CoreAppSMState HandleCaseLabelCheckDigitResponse = new CoreAppSMState(nameof(HandleCaseLabelCheckDigitResponse));
        public static readonly CoreAppSMState EnterQuantity = new CoreAppSMState(nameof(EnterQuantity));
        public static readonly CoreAppSMState VerifyEnteredQuantity = new CoreAppSMState(nameof(VerifyEnteredQuantity));
        public static readonly CoreAppSMState DisplayConfirmQuantity = new CoreAppSMState(nameof(DisplayConfirmQuantity));
        public static readonly CoreAppSMState HandleConfirmQuantityResponse = new CoreAppSMState(nameof(HandleConfirmQuantityResponse));
        public static readonly CoreAppSMState DisplayConfirmShort = new CoreAppSMState(nameof(DisplayConfirmShort));
        public static readonly CoreAppSMState HandleConfirmShortResponse = new CoreAppSMState(nameof(HandleConfirmShortResponse));
        public static readonly CoreAppSMState LotTracking = new CoreAppSMState(nameof(LotTracking));
        public static readonly CoreAppSMState InitPuts = new CoreAppSMState(nameof(InitPuts));
        public static readonly CoreAppSMState InitPutPrompt = new CoreAppSMState(nameof(InitPutPrompt));
        public static readonly CoreAppSMState DisplayPutPrompt = new CoreAppSMState(nameof(DisplayPutPrompt));
        public static readonly CoreAppSMState HandlePutPromptResponse = new CoreAppSMState(nameof(HandlePutPromptResponse));
        public static readonly CoreAppSMState WeightAndSerial = new CoreAppSMState(nameof(WeightAndSerial));
        public static readonly CoreAppSMState StartTransmitPicks = new CoreAppSMState(nameof(StartTransmitPicks));
        public static readonly CoreAppSMState CommTransmitPicks = new CoreAppSMState(nameof(CommTransmitPicks));
        public static readonly CoreAppSMState CheckPartial = new CoreAppSMState(nameof(CheckPartial));
        public static readonly CoreAppSMState CheckCloseTargetContainer = new CoreAppSMState(nameof(CheckCloseTargetContainer));
        public static readonly CoreAppSMState NextStep = new CoreAppSMState(nameof(NextStep));
        public static readonly CoreAppSMState CycleCount = new CoreAppSMState(nameof(CycleCount));
        public static readonly CoreAppSMState DisplayCheckDigitEntry = new CoreAppSMState(nameof(DisplayCheckDigitEntry));
        public static readonly CoreAppSMState HandleCheckDigitResponse = new CoreAppSMState(nameof(HandleCheckDigitResponse));
        public static readonly CoreAppSMState DisplayEnterQuantity = new CoreAppSMState(nameof(DisplayEnterQuantity));
        public static readonly CoreAppSMState HandleEnterQuantityResponse = new CoreAppSMState(nameof(HandleEnterQuantityResponse));
        public static readonly CoreAppSMState AfterPickLutTransmitted = new CoreAppSMState(nameof(AfterPickLutTransmitted));

        private OpenContainerStateMachine _OpenContainerSM;
        private OpenContainerStateMachine OpenContainerSM { get { return Manager.CreateStateMachine(ref _OpenContainerSM); } }
        private NewContainerStateMachine _NewContainerSM;
        private NewContainerStateMachine NewContainerSM { get { return Manager.CreateStateMachine(ref _NewContainerSM); } }
        private CloseContainerStateMachine _CloseContainerSM;
        private CloseContainerStateMachine CloseContainerSM { get { return Manager.CreateStateMachine(ref _CloseContainerSM); } }

        protected int _PickedQuantity;
        protected bool _ShortProduct;
        protected string _IdDescription;
        protected string _Description;
        protected Pick _CurrentPick;
        protected string _CurrentCheckDigit;
        protected string _CurrentQuantityPicked;
        protected int _ExpectedQuantity;

        private string _LotNumber;
        private int _LotQuantity;
        private int _PutQuantity;
        private Assignment _CurrentAssigment;
        private Container _CurrentContainer;
        private List<Pick> _Puts;
        private List<float> _Weights;
        private List<string> _SerialNumbers;
        private Pick _Pick;
        private double? _Weight;
        private string _SerialNumber;
        private long? _ContainerId;
        private bool _Complete;
        private int _Quantity;
        private bool? _ShortProductResponse;
        private string _CurrentCaseLabelCheckDigit;
        private bool? _ConfirmSpokenQuantityResponse;
        private bool? _ConfirmShortResponse;
        private string _PutResponse;
        private string _CurrentContainerToReopen;

        public PickPromptStateMachine(SimplifiedStateMachineManager<VoiceLinkStateMachine, IVoiceLinkModel> manager, IVoiceLinkModel model) : base(manager, model)
        {
        }

        public override void ConfigureStates()
        {
            ConfigureLogicState(InitPickPromptData, () =>
            {
                foreach (var pick in PickAssignmentStateMachine.PickList)
                {
                    _ExpectedQuantity += pick.QuantityToPick - pick.QuantityPicked;
                }

                if (PickAssignmentStateMachine.AutoShort)
                {
                    _ExpectedQuantity = _PickedQuantity = _LotQuantity = _PutQuantity = 0;
                    NextState = InitPuts;
                    return;
                }

                _CurrentPick = PickAssignmentStateMachine.PickList.First();

                if (_CurrentPick.MultipleItemLocation)
                {
                    _Description = _CurrentPick.ItemDescription.ToLower();
                    if (!string.IsNullOrEmpty(_Description))
                    {
                        _Description += ",";
                    }
                }

                if (AssignmentsResponse.CurrentResponse.Count > 1 && PickingRegionsResponse.CurrentPickingRegion.ContainerType == 0)
                {
                    if (_CurrentPick.IDDescription != null)
                    {
                        _IdDescription = Translate.GetLocalizedTextForKey("PickPrompt_Pick_Prompt", _CurrentPick.IDDescription.ToString() + ",");
                    }
                }

                NextState = CheckTargetContainer;
            }, InitPuts, CheckTargetContainer);

            ConfigureLogicState(CheckTargetContainer, async () =>
            {
                // Check if picking to target containers and make sure specified target
                // container exists and is opne.If exists and open and not active then
                // prompt operator to reopen, if does not exist or not opened, then call open
                // container to open new target container
                // Note: assumes only 1 assignment

                NextState = SlotVerification;

                var target = PickAssignmentStateMachine.PickList[0].TargetContainer;
                if (target == null)
                {
                    // Not picking target containers
                    return;
                }
                else if (target != AssignmentsResponse.CurrentResponse.First().ActiveContainer)
                {
                    string spokenContainer = null;
                    if (ContainersResponse.CurrentResponse != null)
                    {
                        foreach (var container in ContainersResponse.CurrentResponse)
                        {
                            if (container.TargetContainer == target && container.ContainerStatus == "0")
                            {
                                spokenContainer = container.SpokenContainerValidation;
                            }
                        }
                    }

                    if (spokenContainer != null)
                    {
                        _CurrentContainerToReopen = spokenContainer;
                        NextState = DisplayReopenContainer;
                    }
                    else
                    {
                        OpenContainerSM.Reset();
                        OpenContainerSM.InitProperties(PickingRegionsResponse.CurrentPickingRegion, AssignmentsResponse.FirstAssignment(), PickAssignmentStateMachine.PickList, AssignmentsResponse.HasMultipleAssignments());
                        await OpenContainerSM.InitializeStateMachineAsync();
                    }
                }
            }, SlotVerification, DisplayReopenContainer);

            ConfigureDisplayState(DisplayReopenContainer, SlotVerification, encodeAction: EncodeReopenContainerPrompt);

            ConfigureLogicState(VerifyProductSlot, () =>
            {
                var currentPick = PickAssignmentStateMachine.PickList.First();
                if (_CurrentCheckDigit != currentPick.ScanProductID
                    && _CurrentCheckDigit != currentPick.SpokenProductID
                    && _CurrentCheckDigit != currentPick.CheckDigits)
                {
                    CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_CheckDigit_Wrong_Prompt", _CurrentCheckDigit);
                    NextState = SlotVerification;
                }
                else if (_CurrentCheckDigit == currentPick.CheckDigits && _CurrentCheckDigit == currentPick.SpokenProductID)
                {
                    NextState = DisplayIdenticalShortProduct;
                }
                else if (_CurrentCheckDigit == currentPick.CheckDigits && (!string.IsNullOrEmpty(currentPick.SpokenProductID) || !string.IsNullOrEmpty(currentPick.ScanProductID)))
                {
                    NextState = DisplayShortProduct;
                }
                else
                {
                    NextState = CaseLabelCheckDigit;
                }
            }, SlotVerification, DisplayIdenticalShortProduct, DisplayShortProduct, CaseLabelCheckDigit);

            ConfigureDisplayState(DisplayIdenticalShortProduct, HandleIdenticalShortProductResponse, encodeAction: EncodeIdenticalShortProduct, decodeAction: DecodeIdenticalShortProduct);

            ConfigureDisplayState(DisplayShortProduct, HandleShortProductResponse, encodeAction: EncodeShortProduct, decodeAction: DecodeShortProduct);

            ConfigureLogicState(HandleIdenticalShortProductResponse, () =>
            {
                if (_ShortProductResponse ?? false)
                {
                    _ExpectedQuantity = _PickedQuantity = _LotQuantity = _PutQuantity = 0;
                    NextState = LotTracking;
                    return;
                }

                NextState = CaseLabelCheckDigit;
            }, LotTracking, CaseLabelCheckDigit);


            //--------------------------------------------------------------------------
            //Case label check digit states
            ConfigureLogicState(CaseLabelCheckDigit, () =>
            {
                // Check if case label check digits exist
                NextState = DisplayCaseLabelCheckDigit;
                if (string.IsNullOrEmpty(PickAssignmentStateMachine.PickList.First().CaseLabelCheckDigit))
                {
                    NextState = EnterQuantity;
                }
            }, EnterQuantity, DisplayCaseLabelCheckDigit);

            ConfigureDisplayState(DisplayCaseLabelCheckDigit, HandleCaseLabelCheckDigitResponse, encodeAction: EncodeCaseLabelCheckDigit, decodeAction: DecodeCaseLabelCheckDigit);

            ConfigureLogicState(HandleCaseLabelCheckDigitResponse, () =>
            {
                NextState = EnterQuantity;

                var caseLabelCD = "";
                foreach (var pick in PickAssignmentStateMachine.PickList)
                {
                    if (pick.Status != "P")
                    {
                        caseLabelCD = pick.CaseLabelCheckDigit;
                    }
                }
                if (caseLabelCD != _CurrentCaseLabelCheckDigit)
                {
                    CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_Selection_CaseLabelCheckDigit_Wrong", _CurrentCaseLabelCheckDigit);
                    NextState = DisplayCaseLabelCheckDigit;
                }

                // TODO: Allow and handle short product, partial, and skip sot
            }, DisplayCaseLabelCheckDigit, EnterQuantity);

            //--------------------------------------------------------------------------
            //Enter Quantity
            ConfigureLogicState(VerifyEnteredQuantity, () =>
            {
                NextState = LotTracking;
                if (_PickedQuantity < 0)
                {
                    CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_Selection_Verify_Entered_Quantity_Negative");
                    NextState = EnterQuantity;
                }
                else if (_PickedQuantity > _ExpectedQuantity)
                {
                    CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_Selection_Verify_Entered_Quantity_Greater",
                        _PickedQuantity.ToString(), _ExpectedQuantity.ToString());
                    NextState = EnterQuantity;
                }
                else if (_PickedQuantity <= _ExpectedQuantity && Model.Partial != true)
                {
                    if (_ShortProduct)
                    {
                        // If already doing a short product, then confirm spoken quantity
                        NextState = DisplayConfirmQuantity;
                    }
                    else if (_PickedQuantity < _ExpectedQuantity)
                    {
                        // Not doing short, but quantity is less than expected, ask if short
                        NextState = DisplayConfirmShort;
                    }
                }
                else if (_PickedQuantity == 0 && Model.Partial == true)
                {
                    Model.Partial = false;
                    CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_Selection_Verify_Entered_Quantity_Zero_Partial");
                    NextState = EnterQuantity;
                }
            }, EnterQuantity, DisplayConfirmQuantity, DisplayConfirmShort, LotTracking);

            ConfigureDisplayState(DisplayConfirmQuantity, HandleConfirmQuantityResponse, encodeAction: EncodeConfirmQuantity, decodeAction: DecodeConfirmQuantity);

            ConfigureLogicState(HandleConfirmQuantityResponse, () =>
            {
                NextState = EnterQuantity;
                if (_ConfirmSpokenQuantityResponse ?? false)
                {
                    _ShortProduct = false;
                _ExpectedQuantity = _PickedQuantity = _LotQuantity = _PutQuantity = _PickedQuantity;
                    NextState = LotTracking;
                    return;
                }

            }, EnterQuantity, LotTracking);

            //--------------------------------------------------------------------------
            //Handle Shorts
            ConfigureDisplayState(DisplayConfirmShort, HandleConfirmShortResponse, encodeAction: EncodeConfirmShort, decodeAction: DecodeConfirmShort);

            ConfigureLogicState(HandleConfirmShortResponse, () =>
            {
                NextState = EnterQuantity;
                _ShortProduct = false;
                if (_ConfirmShortResponse ?? false)
                {
                _ExpectedQuantity = _PickedQuantity = _LotQuantity = _PutQuantity = _PickedQuantity;
                    NextState = LotTracking;
                }
            }, EnterQuantity, LotTracking);

            //--------------------------------------------------------------------------
            //Lot tracking states
            ConfigureLogicState(LotTracking, async () =>
            {
                if (_PickedQuantity > 0 && PickAssignmentStateMachine.PickList.First().CaptureLotFlag)
                {
                    _LotQuantity = 0;
                    //TODO: go to lot tracking state machine
                    await Task.CompletedTask;
                }
                else
                {
                    _LotQuantity = _PickedQuantity;
                }

                NextState = InitPuts;
            }, InitPuts);

            //--------------------------------------------------------------------------
            //Put states
            ConfigureLogicState(InitPuts, () =>
            {
                long? assignmentId = null;
                _CurrentContainer = null;
                _Puts = new List<Pick>();
                _PutQuantity = 0;

                foreach (var pick in PickAssignmentStateMachine.PickList)
                {
                    if (pick.Status != "P")
                    {
                        // Check if first pick, or matches first pick
                        if (assignmentId == null || assignmentId == pick.AssignmentID)
                        {
                            if (assignmentId == null)
                            {
                                assignmentId = pick.AssignmentID;
                            }
                            _Puts.Add(pick);
                            _PutQuantity += (pick.QuantityToPick - pick.QuantityPicked);
                        }
                    }
                }

                // Adjust put quantity if greater than lot quantity
                if (_PutQuantity > _LotQuantity)
                {
                    _PutQuantity = _LotQuantity;
                }

                // Get current assignment record for puts
                _CurrentAssigment = null;
                foreach (var assignment in AssignmentsResponse.CurrentResponse)
                {
                    if (assignment.AssignmentID == assignmentId)
                    {
                        _CurrentAssigment = assignment;
                        break;
                    }
                }

                NextState = InitPutPrompt;

                // Not picking to containers, no put prompt needed.
                if (PickingRegionsResponse.CurrentPickingRegion.ContainerType == 0)
                {
                    NextState = WeightAndSerial;
                }

                // No quantity to put
                if (_PutQuantity <= 0)
                {
                    NextState = WeightAndSerial;
                }
            }, WeightAndSerial, InitPutPrompt);

            ConfigureLogicState(InitPutPrompt, () =>
            {
                List<string> expectedValue = new List<string>();
                var scanValue = "";

                // Do put prompt if multiple assignment or multiple open containers
                var openContainers = ContainersResponse.GetOpenContainers(_CurrentAssigment.AssignmentID);
                if (AssignmentsResponse.HasMultipleAssignments() ||
                    openContainers.Count() > 1)
                {

                    // Determine what the expected container is
                    if (_Puts[0].TargetContainer != 0)
                    {
                        if (_CurrentAssigment.ActiveContainer == _Puts[0].TargetContainer ||
                            openContainers.Count() == 1)
                        {
                            expectedValue.Add(openContainers.First().SpokenContainerValidation);
                            scanValue = openContainers.First().ScannedContainerValidation;
                        }
                    }
                    else if (openContainers.Count() == 1)
                    {
                        expectedValue.Add(openContainers.First().SpokenContainerValidation);
                        scanValue = openContainers.First().ScannedContainerValidation;
                    }

                    // TODO: Determine if "partial" is value response and add it to vocab/menu
                    NextState = DisplayPutPrompt;
                }

                NextState = HandlePutPromptResponse;
            }, HandlePutPromptResponse, DisplayPutPrompt);


            ConfigureDisplayState(DisplayPutPrompt, HandlePutPromptResponse, encodeAction: EncodePutPrompt, decodeAction: DecodePutPrompt);

            ConfigureLogicState(HandlePutPromptResponse, () =>
            {
                NextState = WeightAndSerial;
                var only1ContainerOpen = false;
                var openContainers = ContainersResponse.GetOpenContainers(_CurrentAssigment.AssignmentID);
                if (!AssignmentsResponse.HasMultipleAssignments() &&
                    openContainers.Count() <= 1)
                {
                    // No prompt was required since only one container should be open
                    only1ContainerOpen = true;
                }
                else
                {
                    // TODO: Handle partial response
                }

                if (only1ContainerOpen)
                {
                    if (openContainers.Count() > 0)
                    {
                        _CurrentContainer = openContainers.First();
                    }
                }
                else
                {
                    var matchingOpenContainer = ContainersResponse.GetMatchingOpenContainer(_PutResponse, _CurrentAssigment.AssignmentID);
                    if (_Puts[0].TargetContainer == 0)
                    {
                        if (matchingOpenContainer == null)
                        {
                            CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_Selection_Put_Prompt_Wrong_Container", _PutResponse);
                            NextState = InitPutPrompt;
                        }
                        else
                        {
                            _CurrentContainer = matchingOpenContainer;
                        }
                    }
                    else if (_Puts[0].TargetContainer > 0)
                    {
                        if (matchingOpenContainer == null ||
                            matchingOpenContainer.TargetContainer != _CurrentAssigment.ActiveContainer)
                        {
                            CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_Selection_Put_Prompt_Wrong_Container", _PutResponse);
                            NextState = InitPutPrompt;
                        }
                    }
                }
            }, InitPutPrompt, WeightAndSerial);


            //--------------------------------------------------------------------------
            //Catch Weight Serial Number states
            ConfigureLogicState(WeightAndSerial, async () =>
            {
                if (_PutQuantity > 0)
                {
                    var currentPick = PickAssignmentStateMachine.PickList.First();
                    if (currentPick.VariableWeightItem || currentPick.SerialNumber)
                    {
                        // TODO: implement weights and serial numbers
                        await Task.CompletedTask;
                    }
                }

                NextState = StartTransmitPicks;
            }, StartTransmitPicks);

            //--------------------------------------------------------------------------
            //Send pick information
            ConfigureLogicState(StartTransmitPicks, () =>
            {
                NextState = CoreAppStates.BackgroundActvity;
                backgroundActivityNextState = CommTransmitPicks;
                CurrentBackGroundActivityHeader = Translate.GetLocalizedTextForKey("VoiceLink_Selection_TransmitPicks_Header");
            }, CoreAppStates.BackgroundActvity);

            ConfigureLogicState(CommTransmitPicks, async () =>
            {
                _Pick = null;
                _Weight = null;
                _SerialNumber = null;
                _ContainerId = null;
                _Complete = true;

                // Find a pick to transmit
                foreach (var p in _Puts)
                {
                    if (p.Status != "P")
                    {
                        _Pick = p;
                        break;
                    }
                }

                if (_Pick == null)
                {
                    CurrentUserMessage = Translate.GetLocalizedTextForKey("VoiceLink_Selection_TransmitPick_Failed_Unknown");
                    NextState = DisplayPutPrompt;
                    return;
                }

                // Determine quantity to apply, weight, serial number, and container
                _Quantity = _Pick.QuantityToPick - _Pick.QuantityPicked;
                if (_Quantity > _PutQuantity)
                {
                    _Quantity = _PutQuantity;
                }

                if (_Quantity > 0 && (_Weights.Count() > 0 || _SerialNumbers.Count() > 0))
                {
                    _Quantity = 1;
                    if (_Weights.Count() > 0)
                    {
                        _Weight = _Weights[0];
                    }
                    if (_SerialNumbers.Count() > 0)
                    {
                        _SerialNumber = _SerialNumbers.First();
                    }
                }

                if (_CurrentContainer != null)
                {
                    _ContainerId = _CurrentContainer.ContainerID;
                }

                // Check if pick is complete (still more quantity to apply, but not applying all now)
                if (_PickedQuantity > _Quantity && _Quantity < (_Pick.QuantityToPick - _Pick.QuantityPicked))
                {
                    _Complete = false;
                }
                else if (Model.Partial == true && _Quantity < (_Pick.QuantityToPick - _Pick.QuantityPicked))
                {
                    _Complete = false;
                }

                // If pick isn't complete and there is no quantity then simply return and continue to next state.
                // There is no reason to send picked information
                if (!_Complete && _Quantity <= 0)
                {
                    NextState = CheckPartial;
                }
                else
                {
                    // Transmit pick record
                    NextState = AfterPickLutTransmitted;
                    await Model.LUTtransmit(LutType.ExecutePick, "VoiceLink_BackgroundActivity_Header_Transmitting_Picks",
                        parameters: new ExecutePickParam(_CurrentAssigment, _Pick, _Quantity, _Complete, _ContainerId, _LotNumber, _Weight, _SerialNumber),
                        goToStateIfFail: DisplayPutPrompt
                    );
                }
            }, DisplayPutPrompt, CheckPartial, AfterPickLutTransmitted);

            ConfigureLogicState(AfterPickLutTransmitted, () =>
            {
                // Update pick information
                _Pick.QuantityPicked = _Pick.QuantityPicked + _Quantity;
                if (_Complete)
                {
                    _Pick.Status = "P";
                }

                // Update pick quantities
                _PutQuantity -= _Quantity;
                _LotQuantity -= _Quantity;
                _PickedQuantity -= _Quantity;
                _ExpectedQuantity -= _Quantity;
                if (_Weights.Count() > 0)
                {
                    _Weights.RemoveAt(0);
                }
                if (_SerialNumbers.Count() > 0)
                {
                    _SerialNumbers.RemoveAt(0);
                }

                NextState = CheckPartial;
            }, CheckPartial);

            //--------------------------------------------------------------------------
            //final pick checks
            ConfigureLogicState(CheckPartial, async () =>
            {
                if (Model.Partial == true && _PutQuantity <= 0)
                {
                    Model.Partial = false;
                    if (ContainersResponse.MultipleOpenContainers(_CurrentAssigment.AssignmentID))
                    {
                        _LotQuantity = 0;
                        NewContainerSM.Reset();
                        NewContainerSM.InitProperties(PickingRegionsResponse.CurrentPickingRegion, _CurrentAssigment, PickAssignmentStateMachine.PickList, _CurrentContainer, AssignmentsResponse.HasMultipleAssignments());
                        await NewContainerSM.InitializeStateMachineAsync();
                    }
                }

                NextState = CheckCloseTargetContainer;
            }, CheckCloseTargetContainer);

            ConfigureLogicState(CheckCloseTargetContainer, async () =>
            {
                NextState = NextStep;

                var target = _Puts[0].TargetContainer;

                // Check if target containers.  Continue if not
                if (target == null) return;

                // Check if all picks picked and shorts/go back for shorts
                foreach (var pick in PicksResponse.CurrentResponse)
                {
                    // If pick in same container and not picked (or shorted with go back for short) then return without closing.
                    if (pick.TargetContainer == target)
                    {
                        if (pick.Status != "P")
                        {
                            return;
                        }
                        else if ((pick.QuantityToPick - pick.QuantityPicked) > 0 && PickingRegionsResponse.CurrentPickingRegion.GoBackForShorts != 0)
                        {
                            return;
                        }
                    }
                }

                CloseContainerSM.Reset();
                CloseContainerSM.InitProperties(PickingRegionsResponse.CurrentPickingRegion, _CurrentAssigment, PickAssignmentStateMachine.PickList, _CurrentContainer, AssignmentsResponse.HasMultipleAssignments(), false);
                await CloseContainerSM.InitializeStateMachineAsync();
            }, NextStep);

            ConfigureLogicState(NextStep, () =>
            {
                NextState = CycleCount;

                if (_PutQuantity > 0)
                {
                    // Still more to transmit
                    NextState = CommTransmitPicks;
                }
                else if (_LotQuantity > 0)
                {
                    // Still more to put
                    NextState = InitPuts;
                }
                else if (_PickedQuantity > 0 || _ExpectedQuantity > 0)
                {
                    _LotNumber = null;

                    // Re-prompt for remaining picks
                    if (PickingRegionsResponse.CurrentPickingRegion.MultiPickPrompt == 2)
                    {
                        NextState = EnterQuantity;
                    }
                    else
                    {
                        NextState = SlotVerification;
                    }
                }
                else
                {
                    _LotNumber = null;
                    foreach (var pick in PickAssignmentStateMachine.PickList)
                    {
                        if (pick.Status != "P")
                        {
                            NextState = InitPuts;
                            break;
                        }
                    }
                }

            }, CommTransmitPicks, InitPuts, EnterQuantity, SlotVerification, CycleCount);

            ConfigureReturnLogicState(CycleCount, async () =>
            {
                if (PicksResponse.CurrentResponse[0].CycleCount)
                {
                    // TODO: implement cycle count state machine
                    await Task.CompletedTask;
                }
            });
        }

        #region EncodersDecoders
        private WorkflowObjectContainer EncodeReopenContainerPrompt(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateReadyIntent(Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_ReopenContainer_Header", _CurrentContainerToReopen),
                                                              "readyNone",
                                                              model.CurrentUserMessage,
                                                              null,
                                                              initialPrompt: model.CurrentUserMessage);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private WorkflowObjectContainer EncodeIdenticalShortProduct(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateBooleanIntent(Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_CheckDigit_IdenticalShortProduct_Header"),
                                                                "IdenticalShortProductPrompt",
                                                                Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_CheckDigit_IdenticalShortProduct_Prompt"),
                                                                model.CurrentUserMessage,
                                                                initialPrompt: model.CurrentUserMessage,
                                                                affirmativeVocab: VoiceLinkModuleVocab.Yes,
                                                                negativeVocab: VoiceLinkModuleVocab.No);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeIdenticalShortProduct(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            _ShortProductResponse = GenericBaseEncoder<IVoiceLinkModel>.DecodeBooleanPrompt(slotContainer);
        }

        private WorkflowObjectContainer EncodeShortProduct(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateBooleanIntent(Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_CheckDigit_ShortProduct_Header"),
                                                                "ShortProductPrompt",
                                                                Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_CheckDigit_ShortProduct_Prompt"),
                                                                model.CurrentUserMessage,
                                                                initialPrompt: model.CurrentUserMessage,
                                                                affirmativeVocab: VoiceLinkModuleVocab.Yes,
                                                                negativeVocab: VoiceLinkModuleVocab.No);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeShortProduct(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            _ShortProductResponse = GenericBaseEncoder<IVoiceLinkModel>.DecodeBooleanPrompt(slotContainer);
        }

        private WorkflowObjectContainer EncodeCaseLabelCheckDigit(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_CaseLabel_Header"),
                                                                  "caseLabelID",
                                                                  Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_CaseLabel_Label"),
                                                                  Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_CaseLabel_Prompt"),
                                                                  null,
                                                                  null,
                                                                  model.CurrentUserMessage,
                                                                  initialPrompt: model.CurrentUserMessage);

            wfo.ValueProperties.AllowedCharacters = CharacterSet.AlphaNumeric;
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeCaseLabelCheckDigit(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            _CurrentCaseLabelCheckDigit = GenericBaseEncoder<IVoiceLinkModel>.DecodeValueEntry(slotContainer);
        }

        private WorkflowObjectContainer EncodeConfirmQuantity(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateBooleanIntent(Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_ConfirmQuantity_Header"),
                                                                "ConfirmQuantity",
                                                                Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_ConfirmQuantity_Prompt", _CurrentQuantityPicked),
                                                                model.CurrentUserMessage,
                                                                initialPrompt: model.CurrentUserMessage,
                                                                affirmativeVocab: VoiceLinkModuleVocab.Yes,
                                                                negativeVocab: VoiceLinkModuleVocab.No);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeConfirmQuantity(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            _ConfirmSpokenQuantityResponse = GenericBaseEncoder<IVoiceLinkModel>.DecodeBooleanPrompt(slotContainer);
        }

        private WorkflowObjectContainer EncodeConfirmShort(IVoiceLinkModel model)
        {
            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateBooleanIntent(Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_ConfirmShort_Header"),
                                                                "ConfirmShortProduct",
                                                                Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_ConfirmShort_Prompt", 
                                                                _CurrentQuantityPicked, _ExpectedQuantity.ToString()),
                                                                model.CurrentUserMessage,
                                                                initialPrompt: model.CurrentUserMessage,
                                                                affirmativeVocab: VoiceLinkModuleVocab.Yes,
                                                                negativeVocab: VoiceLinkModuleVocab.No);
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodeConfirmShort(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            _ConfirmShortResponse = GenericBaseEncoder<IVoiceLinkModel>.DecodeBooleanPrompt(slotContainer);
        }

        private WorkflowObjectContainer EncodePutPrompt(IVoiceLinkModel model)
        {
            // Determine prompts
            var prompt = Translate.GetLocalizedTextForKey("VoiceLink_Selection_Put_Prompt_For_Container_Multiple", _PutQuantity.ToString(), _CurrentAssigment.Position.ToString());
            if (ContainersResponse.GetOpenContainers(_CurrentAssigment.AssignmentID).Count() > 0 || model.Partial == true)
            {
                prompt = Translate.GetLocalizedTextForKey("VoiceLink_Selection_Put_Prompt_For_Container");
            }

            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_Put_Header"),
                                                                  "PutPrompt",
                                                                  Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_Put_Label"),
                                                                  prompt,
                                                                  null,
                                                                  null,
                                                                  model.CurrentUserMessage,
                                                                  initialPrompt: model.CurrentUserMessage);

            wfo.ValueProperties.AllowedCharacters = CharacterSet.AlphaNumeric;
            wfo.MessageType = model.MessageType;
            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        private void DecodePutPrompt(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            _PutResponse = GenericBaseEncoder<IVoiceLinkModel>.DecodeValueEntry(slotContainer);
        }

        //======================================================================================
        //Shared Encoders/Decoders for Single and Multiple state machines, can be overridden if needed
        //in those state machines
        protected virtual WorkflowObjectContainer EncodeCheckDigitEntry(IVoiceLinkModel model)
        {
            var currentPick = PickAssignmentStateMachine.PickList.First();
            string prompt;
            if (PickingRegionsResponse.CurrentPickingRegion.MultiPickPrompt == 2)
            {
                prompt = Translate.GetLocalizedTextForKey("PickPrompt_Multiple_Slot_Only", currentPick.Slot);
            }
            else if (PickingRegionsResponse.CurrentPickingRegion.MultiPickPrompt == 1 && _ExpectedQuantity == 1)
            {
                prompt = Translate.GetLocalizedTextForKey("PickPrompt_Single_Slot_Only",
                    _CurrentPick.Slot, _CurrentPick.UOM + ",", _Description, _IdDescription, _CurrentPick.PromptMessage);
            }
            else
            {
                prompt = Translate.GetLocalizedTextForKey("PickPrompt_Single_Pick_Quantity",
                    _CurrentPick.Slot, _ExpectedQuantity.ToString(), _CurrentPick.UOM + ",", _Description, _IdDescription, _CurrentPick.PromptMessage);
            }

            var uiElements = GeneratePickDisplayElements(model.CurrentPick);

            var wfoContainer = new WorkflowObjectContainer();
            var wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_CheckDigit_Header"),
                                                                   "CheckDigit",
                                                                   Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_CheckDigit_Label"),
                                                                   prompt,
                                                                   null,
                                                                   uiElements,
                                                                   model.CurrentUserMessage,
                                                                   initialPrompt: model.CurrentUserMessage);

            wfo.MessageType = model.MessageType;

            int cdLength = !string.IsNullOrEmpty(currentPick.CheckDigits) ? currentPick.CheckDigits.Length : 0;
            int prodIdLength = !string.IsNullOrEmpty(currentPick.SpokenProductID) ? currentPick.SpokenProductID.Length : 0;
            if (cdLength > 0 && prodIdLength > 0)
            {
                wfo.ValueProperties.MaxAllowedLength = Math.Max(cdLength, prodIdLength);
                wfo.ValueProperties.MinRequiredLength = Math.Min(cdLength, prodIdLength);
                wfo.ValueProperties.ResponseExpressions.AddRange(new[] { currentPick.CheckDigits, currentPick.SpokenProductID });
                wfo.ValueProperties.ExpectedSpokenOrTypedValues.AddRange(new[] { currentPick.CheckDigits, currentPick.SpokenProductID });
            }
            else if (cdLength > 0)
            {
                wfo.ValueProperties.MaxAllowedLength = cdLength;
                wfo.ValueProperties.MinRequiredLength = cdLength;
                wfo.ValueProperties.ResponseExpressions.Add(currentPick.CheckDigits);
                wfo.ValueProperties.ExpectedSpokenOrTypedValues.Add(currentPick.CheckDigits);
            }
            else if (prodIdLength > 0)
            {
                wfo.ValueProperties.MaxAllowedLength = prodIdLength;
                wfo.ValueProperties.MinRequiredLength = prodIdLength;
                wfo.ValueProperties.ResponseExpressions.Add(currentPick.SpokenProductID);
                wfo.ValueProperties.ExpectedSpokenOrTypedValues.Add(currentPick.SpokenProductID);
            }

            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(VoiceLinkModuleVocab.SkipSlot));
            if (PickingRegionsResponse.CurrentPickingRegion.MultiPickPrompt != 2) //Single pick prompt gets additional commands. 
            {
                wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(VoiceLinkModuleVocab.ShortProduct));
                wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(VoiceLinkModuleVocab.Partial));
            }

            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        protected virtual void DecodeCheckDigitEntry(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            _CurrentCheckDigit = GenericBaseEncoder<IVoiceLinkModel>.DecodeValueEntry(slotContainer);
        }

        protected virtual WorkflowObjectContainer EncodeEnterQuantity(IVoiceLinkModel model)
        {
            var prompt = Translate.GetLocalizedTextForKey("PickPrompt_Single_EnterQuantity_Quantity");
            if (_ShortProduct)
            {
                prompt = Translate.GetLocalizedTextForKey("PickPrompt_EnterQuantity_Short");
            }
            else if (model.Partial == true)
            {
                prompt = Translate.GetLocalizedTextForKey("PickPrompt_EnterQuantity_Partial");
            }

            var uiElements = GeneratePickDisplayElements(model.CurrentPick);

            var wfoContainer = new WorkflowObjectContainer();
            WorkflowObject wfo = null;
            wfo = WorkflowObjectFactory.CreateGetValueIntent(Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_EnterQuantity_Header"),
                                                                "quantity",
                                                                Translate.GetLocalizedTextForKey("VoiceLink_PickPrompt_EnterQuantity_Label"),
                                                                prompt,
                                                                null,
                                                                uiElements,
                                                                model.CurrentUserMessage,
                                                                initialPrompt: model.CurrentUserMessage);
            wfo.ValueProperties.MaxAllowedLength = _ExpectedQuantity.ToString().Length;

            wfo.MessageType = model.MessageType;

            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(VoiceLinkModuleVocab.SkipSlot));
            wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(VoiceLinkModuleVocab.Partial));
            if (model.Partial == true)
            {
                wfo.AdditionalOverflowMenuOptions.Add(new InteractiveItem(VoiceLinkModuleVocab.ShortProduct));
            }

            wfoContainer.Add(wfo);
            return wfoContainer;
        }

        protected virtual void DecodeEnterQuantity(SlotContainer slotContainer, IVoiceLinkModel model)
        {
            _CurrentQuantityPicked = GenericBaseEncoder<IVoiceLinkModel>.DecodeValueEntry(slotContainer);
        }

        protected virtual List<UIElement> GeneratePickDisplayElements(Pick pick)
        {
            return new List<UIElement>
            {
                new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("VoiceLink_Selection_ProductDetails_Location_Label")
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = pick.Aisle,
                    LabelInfo = Translate.GetLocalizedTextForKey("VoiceLink_Selection_ProductDetails_Aisle_Label"),
                    LabelInfoVertical = true,
                    InlineWithNext = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = pick.Slot,
                    LabelInfo = Translate.GetLocalizedTextForKey("VoiceLink_Selection_ProductDetails_Slot_Label"),
                    LabelInfoVertical = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Banner,
                    Label = Translate.GetLocalizedTextForKey("VoiceLink_Selection_ProductDetails_Details_Label")
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = pick.ItemDescription,
                    Bold = true,
                    Centered = true
                },
                new UIElement
                {
                    ElementType = UIElementType.Detail,
                    Label = Translate.GetLocalizedTextForKey("VoiceLink_Selection_ProductDetails_SerialNumber_Label"),
                    Value = pick.ItemUPC
                }
            };
        }

        #endregion

        public override void Reset()
        {
            Model.Partial = false;
            _ExpectedQuantity = 0;
            _PickedQuantity = 0;
            _ShortProduct = false;
            _LotNumber = null;
            _LotQuantity = 0;
            _PutQuantity = 0;
            _CurrentAssigment = null;
            _CurrentContainer = null;
            _Puts = new List<Pick>();
            _Weights = new List<float>();
            _SerialNumbers = new List<string>();
            _IdDescription = "";
            _Description = "";
            _CurrentPick = null;

            OpenContainerSM.Reset();
            NewContainerSM.Reset();
            CloseContainerSM.Reset();
        }
    }
}
