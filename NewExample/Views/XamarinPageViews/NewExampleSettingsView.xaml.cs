using Common.Logging;
using Honeywell.Firebird.CoreLibrary;
using System;

namespace NewExample
{
    public partial class NewExampleSettingsView : CoreView
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:NewExample.NewExampleSettingsView"/> class.
        /// </summary>
        /// <param name="viewModel">View model.</param>
        /// <param name="logger">Logger.</param>
        public NewExampleSettingsView(NewExampleSettingsViewModel viewModel, ILog logger) : base(viewModel, logger)
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

        private void HostEntryLostFocus(object sender, EventArgs e)
        {
            var viewModel = CoreViewModel as NewExampleSettingsViewModel;
            viewModel.OnHostEntryLostFocus?.Execute(null);
        }

        private void PortEntryLostFocus(object sender, EventArgs e)
        {
            var viewModel = CoreViewModel as NewExampleSettingsViewModel;
            viewModel.OnPortEntryLostFocus?.Execute(null);
        }
    }
}
