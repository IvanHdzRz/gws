//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using Honeywell.Firebird.WorkflowEngine;

    public class ReceivingConfirmConditionViewModel : ReceivingBooleanConfirmationViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivingConfirmConditionViewModel"/> class.
        /// </summary>
        /// <param name="dependencies">The depenendencies.</param>
        public ReceivingConfirmConditionViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {
        }
    }
}
