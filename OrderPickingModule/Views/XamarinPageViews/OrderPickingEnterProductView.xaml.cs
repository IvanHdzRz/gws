//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary.Localization;

    public partial class OrderPickingEnterProductView : OrderPickingView
    {
        public OrderPickingEnterProductView(OrderPickingEnterDigitsViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;
            EnableStockCodeEntry();

            if (viewModel.SkipProductVisible)
            {
                AddButton(0, "SkipButtonText", "PrimaryButtonStyle", "ValidationModel.SubmitResponseCommand", "OrderPickingEnterProductViewSkipButton", TranslateExtension.GetLocalizedTextForBaseKey("VocabWord_SkipProduct"));
            }
        }
    }
}
