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

    public partial class OrderPickingStagingLocationView : CoreView
    {
        public OrderPickingStagingLocationView(OrderPickingEnterStagingLocationViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;

            StagingLocationEntrySubview.IsEnabled = true;
        }

        public OrderPickingStagingLocationView(OrderPickingConfirmStagingLocationViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;

            AddButton(0, "NegativeButtonText", "SecondaryButtonStyle", "NegativeCommand", "OrderPickingStagingLocationViewNegativeButton");
            AddButton(1, "AffirmativeButtonText", "PrimaryButtonStyle", "AffirmativeCommand", "OrderPickingStagingLocationViewAffirmativeButton");

            StagingLocationEntrySubview.IsAccepted = true;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            DisplaySequenceLayout();
            SetFormattedLabels();
        }

        protected void SetFormattedLabels()
        {
            if (ViewModel.GetType() == typeof(OrderPickingEnterStagingLocationViewModel))
            {
                SetFormattedHeaderLabel();
            }
        }

        protected virtual void SetFormattedHeaderLabel()
        {
            
            string placeholder1 = "placeholder1";
            string sequence = TranslateExtension.GetLocalizedTextForBaseKey("Header", placeholder1);
            var formattedHeader = new FormattedString();
            formattedHeader.Spans.Add(new Span
            {
                Text = sequence.Substring(0, sequence.IndexOf(placeholder1)),
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBook
            });
            formattedHeader.Spans.Add(new Span
            {
                Text = ((OrderPickingEnterStagingLocationViewModel)BindingContext).Container,
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBlack
            });
            formattedHeader.Spans.Add(new Span
            {
                Text = sequence.Substring(sequence.IndexOf(placeholder1) + placeholder1.Length),
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBook
            });
            HeaderLabel.FormattedText = formattedHeader;
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