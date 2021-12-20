//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System;
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Xamarin.Forms;
    using Retail;

    public partial class OrderPickingReadyView : CoreView
    {
        public OrderPickingReadyView(SingleResponseViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;

            AddButton(0, "OrderPickingWorkflowActivity_CancelButtonText", "SecondaryButtonStyle", "ValidationModel.SubmitResponseCommand", "OrderPickingReadyViewCancelButton", TranslateExtension.GetLocalizedTextForBaseKey("VocabWord_EndOrder"));
            AddButton(1, "NextButtonText", "PrimaryButtonStyle", "ValidationModel.SubmitResponseCommand", "OrderPickingReadyViewNextButton", TranslateExtension.GetLocalizedTextForBaseKey("next_entry_word"));
        }

        public OrderPickingReadyView(ReadyViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;

            AddButton(0, "NextButtonText", "PrimaryButtonStyle", "ReadyCommand", "OrderPickingReadyViewNextButton");
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            DisplaySequenceLayout();
        }

        /// <summary>
        /// Adds a button to ButtonGrid.
        /// </summary>
        /// <param name="column">Zero based column of grid to insert button.</param>
        /// <param name="textLocalizationKey">Localization key for text property</param>
        /// <param name="styleKey">Style resource Key</param>
        /// <param name="commandName">Name of command to bind</param>
        /// <param name="automationID">Automation ID</param>
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

        protected void OnInfoTapped(object sender, EventArgs e)
        {
            if (HeaderLayout.IsVisible)
            {
                DisplayInstructionsLayout();
            }
            else
            {
                DisplaySequenceLayout();
            }
        }

        protected void DisplayInstructionsLayout()
        {
            HeaderLayout.IsVisible = false;
            InstructionsLayout.IsVisible = true;
        }

        protected void DisplaySequenceLayout()
        {
            HeaderLayout.IsVisible = true;
            InstructionsLayout.IsVisible = false;
        }

    }
}