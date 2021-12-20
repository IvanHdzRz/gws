//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace SimpleApp
{
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;
    using System;

    /// <summary>
    /// SimpleApp server settings view.
    /// </summary>
    public partial class SimpleAppSettingsView : CoreView
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SimpleApp.SimpleAppSettingsView"/> class.
        /// </summary>
        /// <param name="viewModel">View model.</param>
        /// <param name="logger">Logger.</param>
        public SimpleAppSettingsView(SimpleAppSettingsViewModel viewModel, ILog logger) : base(viewModel, logger)
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
            base.OnDisappearing();
        }

        public void HostEntryLostFocus(object sender, EventArgs e)
        {
            var viewModel = CoreViewModel as SimpleAppSettingsViewModel;
            viewModel.OnHostEntryLostFocus?.Execute(null);
        }

        public void PortEntryLostFocus(object sender, EventArgs e)
        {
            var viewModel = CoreViewModel as SimpleAppSettingsViewModel;
            viewModel.OnPortEntryLostFocus?.Execute(null);
        }
    }
}
