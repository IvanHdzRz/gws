//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2014 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System;
    using Common.Logging;
    using GuidedWork;
    using Honeywell.Firebird.CoreLibrary;

    public partial class WarehousePickingEnterLabelPrinterView : CoreView
    {
        public WarehousePickingEnterLabelPrinterView(WarehousePickingEnterDigitsViewModel viewModel, ILog logger)
            : base(viewModel, logger)
        {
            InitializeComponent();

            BindingContext = viewModel;
        }

        public void OnDone(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LabelPrinterEntry.Text))
            {
                var viewModel = (WarehousePickingEnterDigitsViewModel)ViewModel;
                viewModel.ValidationModel.SubmitResponseCommand.Execute(LabelPrinterEntry.Text);
            }
        }
    }
}
