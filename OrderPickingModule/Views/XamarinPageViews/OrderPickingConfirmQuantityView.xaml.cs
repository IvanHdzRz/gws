//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Xamarin.Forms;

    public partial class OrderPickingConfirmQuantityView : OrderPickingView
    {
        public OrderPickingConfirmQuantityView(OrderPickingBooleanConfirmationViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override FormattedString GetFormattedHeaderLabel()
        {
            string placeholder1 = "placeholder1";
            string placeholder2 = "placeholder2";
            string sequence = TranslateExtension.GetLocalizedTextForBaseKey("Header", placeholder1, placeholder2);
            var formattedHeader = new FormattedString();
            formattedHeader.Spans.Add(new Span
            {
                Text = sequence.Substring(0, sequence.IndexOf(placeholder1)),
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBook
            });
            formattedHeader.Spans.Add(new Span
            {
                Text = ((OrderPickingBooleanConfirmationViewModel)BindingContext).RemainingQuantity,
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBlack
            });
            formattedHeader.Spans.Add(new Span
            {
                Text = sequence.Substring(sequence.IndexOf(placeholder1) + placeholder1.Length, sequence.IndexOf(placeholder2) - (sequence.IndexOf(placeholder1) + placeholder1.Length)),
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBook
            });
            formattedHeader.Spans.Add(new Span
            {
                Text = ((OrderPickingBooleanConfirmationViewModel)BindingContext).Container,
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBlack
            });
            formattedHeader.Spans.Add(new Span
            {
                Text = sequence.Substring(sequence.IndexOf(placeholder2) + placeholder2.Length),
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBook
            });
            return formattedHeader;
        }

    }
}
