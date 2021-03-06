//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System.Threading.Tasks;
    using Honeywell.DialogueRunner;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWorkRunner;

    /// <summary>
    /// Controller for a WFA to set events based on the retrieval status.
    /// </summary>
    public class OrderPickingGuidedWorkController : GuidedWorkWorkflowController
    {
        public OrderPickingGuidedWorkController(WorkflowViewControllerDependencies dependencies,
            IGuidedWorkRunner guidedWorkRunner, IOrderPickingMobileDataExchange mobileDataExchange, IDialogueRunner dialogueRunner, IModal modal, IGuidedWorkStore guidedWorkStore)
            : base(dependencies, guidedWorkRunner, mobileDataExchange, dialogueRunner, modal, guidedWorkStore)
        {
        }

        protected override async Task HandleReturnFromOperPrep()
        {
            // When returning from oper prep the state machine is in an intermediate state 
            // similar to waiting for user input. Respond needs to be called to kick the state
            // machine into the next state.
            await GuidedWorkRunner.RespondAsync();
        }
    }
}
