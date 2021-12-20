//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using Honeywell.Firebird.CoreLibrary;

    /// <summary>
    /// Controller for WFA that notifies a user work is completed.
    /// </summary>
    public class OrderPickingAllDoneController : ReadyController
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderPickingAllDoneController"/> class.
        /// </summary>
        public OrderPickingAllDoneController(CoreViewControllerDependencies dependencies) : base(dependencies)
        {
        }


        public override void Ready()
        {
            // pop back to workflow selection
            PublishWorkflowActivityEvent("NavigateBackEvent");
        }
    }
}
