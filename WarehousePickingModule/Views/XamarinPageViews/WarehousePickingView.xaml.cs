//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using System;
    using Common.Logging;
    using System.Collections.Generic;
    using GuidedWork;
    using Xamarin.Forms;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;

    public partial class WarehousePickingView : CoreView
    {
        public WarehousePickingView(WarehousePickingEnterDigitsViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        public WarehousePickingView(WarehousePickingAcknowledgeLocationViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;

            AddButton(0, "CancelButtonText", "SecondaryButtonStyle", "ValidationModel.SubmitResponseCommand", "WarehousePickingEnterProductViewCancelButton", TranslateExtension.GetLocalizedTextForBaseKey("VocabWord_EndOrder"));
            AddButton(1, "SkipButtonText", "SecondaryButtonStyle", "ValidationModel.SubmitResponseCommand", "WarehousePickingEnterProductViewSkipButton", TranslateExtension.GetLocalizedTextForBaseKey("VocabWord_SkipProduct"));
            AddButton(2, "ReadyButtonText", "PrimaryButtonStyle", "ValidationModel.SubmitResponseCommand", "WarehousePickingEnterProductViewReadyButton", TranslateExtension.GetLocalizedTextForBaseKey("accept_entry_word"));
        }

        public WarehousePickingView(WarehousePickingBooleanConfirmationViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;

            AddButton(0, "NegativeButtonText", "SecondaryButtonStyle", "NegativeCommand", "WarehousePickingEnterProductViewNegativeButton");
            AddButton(1, "AffirmativeButtonText", "PrimaryButtonStyle", "AffirmativeCommand", "WarehousePickingEnterProductViewAffirmativeButton");

            StockCodeEntrySubview.SetBinding(UserEntrySubview.TextProperty, "StockCodeResponse");
            QuantityEntrySubview.SetBinding(UserEntrySubview.TextProperty, "QuantityPicked");

            if (!string.IsNullOrEmpty(viewModel.StockCodeResponse))
            {
                StockCodeEntrySubview.IsAccepted = true;
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            BuildLocationLayout(((IWarehousePickingViewModel)BindingContext).LocationDescriptors);
            DisplaySequenceLayout();
            SetFormattedLabels();
        }

        protected void ResetStockCodeEntry()
        {
            StockCodeEntrySubview.Text = string.Empty;
        }

        protected void ResetQuantityEntry()
        {
            QuantityEntrySubview.Text = string.Empty;
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
        protected void AddButton(int column, string textLocalizationKey, string styleKey, string commandName,string automationID, string commandParameter = null)
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
        /// Constructs the location descriptors layout.
        /// </summary>
        /// <param name="locationDescriptors">List of <see cref="LocationDescriptor"/> objects</param>
        protected void BuildLocationLayout(List<LocationDescriptor> locationDescriptors)
        {
            LocationDescriptorsLayout.Children.Clear();
            foreach (var locationDescriptor in locationDescriptors)
            {
                var locationDescriptorLayout = new StackLayout()
                {
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Spacing = 0
                };
                var descriptorNameLabel = new Label()
                {
                    Text = "(" + locationDescriptor.Name + ")",
                    Style = Application.Current.Resources["LocationDescriptorLabelStyle"] as Style,
                    AutomationId = "WarehousePickingEnterProductView"+ locationDescriptor.Name
                };
                var descriptorValueLabel = new Label()
                {
                    Text = locationDescriptor.Value,
                    Style = Application.Current.Resources["LocationLabelStyle"] as Style,
                    AutomationId = "WarehousePickingEnterProductView" + locationDescriptor.Value
                };

                locationDescriptorLayout.Children.Add(descriptorValueLabel);
                locationDescriptorLayout.Children.Add(descriptorNameLabel);

                LocationDescriptorsLayout.Children.Add(locationDescriptorLayout);
            }
        }

        protected void SetFormattedLabels()
        {
            SetFormattedSequenceLabel();
        }

        protected void SetFormattedSequenceLabel()
        {
            string placeholder1 = "placeholder1";
            string placeholder2 = "placeholder2";
            string sequence = TranslateExtension.GetLocalizedTextForBaseKey("WarehousePicking_Sequence", placeholder1, placeholder2);
            var formattedSequence = new FormattedString();
            formattedSequence.Spans.Add(new Span
            {
                Text = sequence.Substring(0, sequence.IndexOf(placeholder1)),
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBook
            });
            formattedSequence.Spans.Add(new Span
            {
                Text = ((IWarehousePickingViewModel)BindingContext).CurrentProductIndex,
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBlack
            });
            formattedSequence.Spans.Add(new Span
            {
                Text = sequence.Substring(sequence.IndexOf(placeholder1) + placeholder1.Length, sequence.IndexOf(placeholder2) - (sequence.IndexOf(placeholder1) + placeholder1.Length)),
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBook
            });
            formattedSequence.Spans.Add(new Span
            {
                Text = ((IWarehousePickingViewModel)BindingContext).TotalProducts,
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBlack
            });
            formattedSequence.Spans.Add(new Span
            {
                Text = sequence.Substring(sequence.IndexOf(placeholder2) + placeholder2.Length),
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBook
            });
            SequenceLabel.FormattedText = formattedSequence;
        }

        /// <summary>
        /// Enables the entry for stock code and sets corresponding bindings and style.
        /// </summary>
        protected void EnableStockCodeEntry()
        {
            StockCodeEntrySubview.SetBinding(UserEntrySubview.TextProperty, "Response");
            StockCodeEntrySubview.SetBinding(UserEntrySubview.ErrorMessageProperty, "ErrorMessage");
            StockCodeEntrySubview.IsEnabled = true;
            StockCodeEntrySubview.IsAccepted = false;
        }

        /// <summary>
        /// Enables the entry for picked quantity and sets corresponding bindings and style.
        /// </summary>
        protected void EnableQuantityEntry()
        {
            QuantityEntrySubview.SetBinding(UserEntrySubview.TextProperty, "Response");
            QuantityEntrySubview.SetBinding(UserEntrySubview.ErrorMessageProperty, "ErrorMessage");
            QuantityEntrySubview.IsEnabled = true;
            StockCodeEntrySubview.SetBinding(UserEntrySubview.TextProperty, "StockCodeResponse");
            StockCodeEntrySubview.IsAccepted = true;
        }

        protected void OnInfoTapped(object sender, EventArgs e)
        {
            if (SequenceLayout.IsVisible)
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
            SequenceLayout.IsVisible = false;
            InstructionsLayout.IsVisible = true;
        }

        protected void DisplaySequenceLayout()
        {
            SequenceLayout.IsVisible = true;
            InstructionsLayout.IsVisible = false;
        }
 
    }
}