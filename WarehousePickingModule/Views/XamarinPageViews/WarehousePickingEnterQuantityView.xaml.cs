//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using Common.Logging;

    public partial class WarehousePickingEnterQuantityView : WarehousePickingView
    {
        public WarehousePickingEnterQuantityView(WarehousePickingEnterDigitsViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;
            EnableQuantityEntry();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            ResetQuantityEntry();
        }
    }
}
