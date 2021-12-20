﻿//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;
    using System;
    using Xamarin.Forms;

    public partial class BasePickingServerSettingsView : CoreView
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:BasePicking.BasePickingServerSettingsView"/> class.
        /// </summary>
        /// <param name="viewModel">View model.</param>
        /// <param name="logger">Logger.</param>
        public BasePickingServerSettingsView(BasePickingServerSettingsViewModel viewModel, ILog logger) : base(viewModel, logger)
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
            base.OnDisappearing();
        }

        public void HostEntryLostFocus(object sender, EventArgs e)
        {
            var viewModel = CoreViewModel as BasePickingServerSettingsViewModel;
            viewModel.OnHostEntryLostFocus?.Execute(null);
        }

        public void PortEntryLostFocus(object sender, EventArgs e)
        {
            var viewModel = CoreViewModel as BasePickingServerSettingsViewModel;
            viewModel.OnPortEntryLostFocus?.Execute(null);
        }

        public void ODRPortEntryLostFocus(object sender, EventArgs e)
        {
            var viewModel = CoreViewModel as BasePickingServerSettingsViewModel;
            viewModel.OnODRPortEntryLostFocus?.Execute(null);
        }
    }
}
