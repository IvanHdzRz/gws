//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Xamarin.Forms;

    public partial class ReceivingView : CoreView
    {
        public ReceivingView(ReceivingEnterDigitsViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        public ReceivingView(ReceivingBooleanConfirmationViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;

            AddButton(0, "NegativeButtonText", "SecondaryButtonStyle", "NegativeCommand", "ReceivingViewNegativeButton");
            AddButton(1, "AffirmativeButtonText", "PrimaryButtonStyle", "AffirmativeCommand", "ReceivingViewAffirmativeButton");

            HiQuantityEntrySubview.SetBinding(UserEntrySubview.TextProperty, "HiQuantityPicked");
            TiQuantityEntrySubview.SetBinding(UserEntrySubview.TextProperty, "TiQuantityPicked");
            TotalQuantityEntrySubview.SetBinding(UserEntrySubview.TextProperty, "TotalQuantityPicked");
        }

        public ReceivingView(ReceivingConfirmConditionViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;

            AddButton(0, "NegativeButtonText", "SecondaryButtonStyle", "NegativeCommand", "ReceivingViewDamagedButton");
            AddButton(1, "AffirmativeButtonText", "PrimaryButtonStyle", "AffirmativeCommand", "ReceivingViewGoodButton");

            HiQuantityEntryStackLayout.IsVisible = false;
            TiQuantityEntryStackLayout.IsVisible = false;
            TotalQuantityEntryStackLayout.IsVisible = false;
        }

        /// <summary>
        /// Adds a button to ButtonGrid.
        /// </summary>
        /// <param name="column">Zero based column of grid to insert button.</param>
        /// <param name="textLocalizationKey">Localization key for text property</param>
        /// <param name="styleKey">Style resource Key</param>
        /// <param name="commandName">Name of command to bind</param>
        /// <param name="automationID">Automation ID for button</param>
        /// <param name="commandParameter">CommandParameter for Command (optional)</param>
        protected void AddButton(int column, string textLocalizationKey, string styleKey, string commandName, string automationID, string commandParameter = null)
        {
            var button = new Button()
            {
                Style = Application.Current.Resources[styleKey] as Style,
                Text = TranslateExtension.GetLocalizedTextForBaseKey(textLocalizationKey).ToUpper(),
                CommandParameter = commandParameter,
                AutomationId = automationID
            };
            button.SetBinding(Button.CommandProperty, commandName);
            ButtonGrid.Children.Add(button, column, 0);
        }

        /// <summary>
        /// Enables the entry for total quantity and sets corresponding bindings and style.
        /// </summary>
        protected void EnableTotalQuantityEntry()
        {
            TotalQuantityEntrySubview.SetBinding(UserEntrySubview.ErrorMessageProperty, "ErrorMessage");
            TotalQuantityEntrySubview.IsEnabled = true;
        }

        /// <summary>
        /// Enables the entry for hi quantity and sets corresponding bindings and style.
        /// </summary>
        protected void EnableHiQuantityEntry()
        {
            HiQuantityEntrySubview.Text = string.Empty;
            HiQuantityEntrySubview.SetBinding(UserEntrySubview.ErrorMessageProperty, "ErrorMessage");
            HiQuantityEntrySubview.IsEnabled = true;
        }

        /// <summary>
        /// Enables the entry for picked quantity and sets corresponding bindings and style.
        /// </summary>
        protected void EnableTiQuantityEntry()
        {
            HiQuantityEntrySubview.SetBinding(UserEntrySubview.TextProperty, "HiQuantityPicked");
            TiQuantityEntrySubview.SetBinding(UserEntrySubview.ErrorMessageProperty, "ErrorMessage");
            TiQuantityEntrySubview.IsEnabled = true;
        }

        protected void ResetHiQuantityEntry()
        {
            HiQuantityEntrySubview.Text = string.Empty;
        }

        protected void ResetTiQuantityEntry()
        {
            TiQuantityEntrySubview.Text = string.Empty;
        }
    }
}