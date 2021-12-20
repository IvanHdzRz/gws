//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;

    public partial class OrderPickingOrderStatusView : CoreView
    {
        public OrderPickingOrderStatusView(OrderPickingOrderStatusViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
