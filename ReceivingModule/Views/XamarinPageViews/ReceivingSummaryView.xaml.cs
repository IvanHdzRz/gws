//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;
    using GuidedWork;
    using Xamarin.Forms;

    public partial class ReceivingSummaryView : CoreView
    {
        public ReceivingSummaryView(ReceivingSummaryViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        public void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            ((ListView)sender).SelectedItem = null;
        }
    }
}
