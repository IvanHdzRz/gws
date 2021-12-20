//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;
    using System;

    public partial class LAppSettingsView : CoreView
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:LAppModule.LAppSettingsView"/> class.
        /// </summary>
        /// <param name="viewModel">View model.</param>
        /// <param name="logger">Logger.</param>
        public LAppSettingsView(LAppSettingsViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();

            BindingContext = viewModel;
        }

        /// <summary>
        /// When overridden, allows application developers to customize behavior
        /// immediately prior to the Page becoming visible.
        /// </summary>
        protected override void OnAppearing()
        {
            //add our subscriptions to the host/port entries losing focus
            HostEntry.UserEntrySubviewUnfocused += HostEntryLostFocus;
            PortEntry.UserEntrySubviewUnfocused += PortEntryLostFocus;
            CompanyDBEntry.UserEntrySubviewUnfocused += CompanyDBEntryLostFocus;

            base.OnAppearing();
        }

        /// <summary>
        /// When overridden, allows the application developer to customize
        /// behavior as the Page disappears.
        /// </summary>
        protected override void OnDisappearing()
        {
            //remove our subscriptions to the host/port entries losing focus
            HostEntry.UserEntrySubviewUnfocused -= HostEntryLostFocus;
            PortEntry.UserEntrySubviewUnfocused -= PortEntryLostFocus;
            CompanyDBEntry.UserEntrySubviewUnfocused -= CompanyDBEntryLostFocus;

            base.OnDisappearing();
        }

        private void HostEntryLostFocus(object sender, EventArgs e)
        {
            var viewModel = CoreViewModel as LAppSettingsViewModel;
            viewModel.OnHostEntryLostFocus?.Execute(null);
        }

        private void PortEntryLostFocus(object sender, EventArgs e)
        {
            var viewModel = CoreViewModel as LAppSettingsViewModel;
            viewModel.OnPortEntryLostFocus?.Execute(null);
        }

        private void CompanyDBEntryLostFocus(object sender, EventArgs e)
        {
            var viewModel = CoreViewModel as LAppSettingsViewModel;
            viewModel.OnCompanyDBEntryLostFocus?.Execute(null);
        }
    }
}
