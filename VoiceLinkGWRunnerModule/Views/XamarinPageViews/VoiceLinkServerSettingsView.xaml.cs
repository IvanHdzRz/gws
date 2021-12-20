//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;
    using System;

    /// <summary>
    /// Voice link server settings view.
    /// </summary>
    public partial class VoiceLinkServerSettingsView : CoreView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:VoiceLink.VoiceLinkServerSettingsView"/> class.
        /// </summary>
        /// <param name="viewModel">View model.</param>
        /// <param name="logger">Logger.</param>
        public VoiceLinkServerSettingsView(VoiceLinkServerSettingsViewModel viewModel, ILog logger) : base(viewModel, logger)
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
            ODRPortEntry.UserEntrySubviewUnfocused += ODRPortEntryLostFocus;
            SiteNameEntry.UserEntrySubviewUnfocused += SiteNameEntryLostFocus;
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
            ODRPortEntry.UserEntrySubviewUnfocused -= ODRPortEntryLostFocus;
            SiteNameEntry.UserEntrySubviewUnfocused -= SiteNameEntryLostFocus;
            base.OnDisappearing();
        }

        public void HostEntryLostFocus(object sender, EventArgs e)
        {
            var viewModel = CoreViewModel as VoiceLinkServerSettingsViewModel;
            viewModel.OnHostEntryLostFocus?.Execute(null);
        }

        public void PortEntryLostFocus(object sender, EventArgs e)
        {
            var viewModel = CoreViewModel as VoiceLinkServerSettingsViewModel;
            viewModel.OnPortEntryLostFocus?.Execute(null);
        }

        public void ODRPortEntryLostFocus(object sender, EventArgs e)
        {
            var viewModel = CoreViewModel as VoiceLinkServerSettingsViewModel;
            viewModel.OnODRPortEntryLostFocus?.Execute(null);
        }

        public void SiteNameEntryLostFocus(object sender, EventArgs e)
        {
            var viewModel = CoreViewModel as VoiceLinkServerSettingsViewModel;
            viewModel.OnSiteNameEntryLostFocus?.Execute(null);
        }
    }
}
