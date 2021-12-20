//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace OrderPicking
{
    using System;
    using System.Collections.Generic;
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.CoreLibrary.Localization;
    using Retail;
    using Xamarin.Forms;

    public partial class OrderPickingView : CoreView
    {
        private EventHandler _OnClickEvent;

        public OrderPickingView(OrderPickingEnterDigitsViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        public OrderPickingView(OrderPickingAcknowledgeLocationViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;

            AddButton(0, "OrderPickingWorkflowActivity_CancelButtonText", "SecondaryButtonStyle", "ValidationModel.SubmitResponseCommand", "OrderPickingViewCancelButton", TranslateExtension.GetLocalizedTextForBaseKey("VocabWord_EndOrder"));
            AddButton(1, "SkipButtonText", "SecondaryButtonStyle", "ValidationModel.SubmitResponseCommand", "OrderPickingViewSkipButton", TranslateExtension.GetLocalizedTextForBaseKey("VocabWord_SkipProduct"));
            AddButton(2, "NextButtonText", "PrimaryButtonStyle", "ValidationModel.SubmitResponseCommand", "OrderPickingViewNextButton", TranslateExtension.GetLocalizedTextForBaseKey("next_entry_word"));
        }

        public OrderPickingView(OrderPickingBooleanConfirmationViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;

            AddButton(0, "NegativeButtonText", "SecondaryButtonStyle", "NegativeCommand", "OrderPickingViewNegativeButton");
            AddButton(1, "AffirmativeButtonText", "PrimaryButtonStyle", "AffirmativeCommand", "OrderPickingViewAffirmativeButton");

            StockCodeEntrySubview.SetBinding(UserEntrySubview.TextProperty, "StockCodeResponse");
            QuantityEntrySubview.SetBinding(UserEntrySubview.TextProperty, "QuantityPicked");

            if (!string.IsNullOrEmpty(viewModel.StockCodeResponse))
            {
                StockCodeEntrySubview.IsAccepted = true;
            }
            foreach (var toolbarItem in ToolbarItems)
            {
                if (!string.IsNullOrEmpty(toolbarItem.Text) && toolbarItem.Order == ToolbarItemOrder.Secondary)
                {
                    System.Diagnostics.Debug.WriteLine($"### - toolbar item {toolbarItem.Text}");
                }
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            BuildLocationLayout(((IOrderPickingViewModel)BindingContext).LocationDescriptors);
            DisplaySequenceLayout();
            SetFormattedLabels();

            if (BindingContext is OrderPickingEnterDigitsViewModel)
            {
                _OnClickEvent = OnClickDigitEntry;
                UpdateOverflowMenu();
            }
            else if (BindingContext is OrderPickingAcknowledgeLocationViewModel)
            {
                _OnClickEvent = OnClickSingleResponse;
                UpdateOverflowMenu();
            }
        }

        public virtual void OnClickDigitEntry(object sender, EventArgs e)
        {
            var viewModel = BindingContext as OrderPickingEnterDigitsViewModel;
            ToolbarItem tbi = (ToolbarItem)sender;
            viewModel.ValidationModel.SubmitResponseCommand?.Execute(tbi.Text);
        }

        public virtual void OnClickSingleResponse(object sender, EventArgs e)
        {
            var viewModel = BindingContext as OrderPickingAcknowledgeLocationViewModel;
            ToolbarItem tbi = (ToolbarItem)sender;
            viewModel.ValidationModel.SubmitResponseCommand?.Execute(tbi.Text);
        }

        private void UpdateOverflowMenu()
        {
            var existing = new Dictionary<string, ToolbarItem>();
            var viewModel = ViewModel as IOrderPickingViewModel;

            // Build list of existing items
            foreach (ToolbarItem tbi in ToolbarItems)
            {
                if (!string.IsNullOrEmpty(tbi.Text) && tbi.Order == ToolbarItemOrder.Secondary)
                {
                    existing.Add(tbi.Text, tbi);
                }
            }

            foreach (var viewModelOverflowMenuItem in viewModel.OverflowMenuItems)
            {
                if (!existing.ContainsKey(viewModelOverflowMenuItem))
                {
                    ToolbarItem tbi = new ToolbarItem
                    {
                        Text = viewModelOverflowMenuItem,
                        Priority = 0,
                        Order = ToolbarItemOrder.Secondary,
                        AutomationId = $"CMD_{viewModelOverflowMenuItem}"
                    };

                    tbi.Clicked += _OnClickEvent;
                    ToolbarItems.Add(tbi);
                }
            }
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
                    AutomationId = locationDescriptor.Name + "Label"
                };
                var descriptorValueLabel = new Label()
                {
                    Text = locationDescriptor.Value,
                    Style = Application.Current.Resources["LocationLabelStyle"] as Style,
                    AutomationId = locationDescriptor.Name + locationDescriptor.Value + "Label"
                };

                locationDescriptorLayout.Children.Add(descriptorValueLabel);
                locationDescriptorLayout.Children.Add(descriptorNameLabel);

                LocationDescriptorsLayout.Children.Add(locationDescriptorLayout);
            }
        }

        protected void SetFormattedLabels()
        {
            SetFormattedProductIdentifierLabel();
            HeaderLabel.FormattedText = GetFormattedHeaderLabel();
        }

        protected void SetFormattedProductIdentifierLabel()
        {
            var viewModel = ((IOrderPickingViewModel)BindingContext);
            string productIdentifier = viewModel.ProductIdentifier;
            int highlightLength = (int)viewModel.ExpectedStockCodeResponseLength;
            var formattedProductIdentifier = new FormattedString();

            string nonhighlightedText = string.Empty;
            if (productIdentifier.Length >= highlightLength)
            {
                nonhighlightedText = productIdentifier.Substring(0, productIdentifier.Length - highlightLength);
            }

            string highlightedText = productIdentifier;
            if (productIdentifier.Length >= highlightLength)
            {
                highlightedText = productIdentifier.Substring(productIdentifier.Length - highlightLength);
            }

            formattedProductIdentifier.Spans.Add(new Span
            {
                Text = nonhighlightedText,
                FontSize = 14,
                ForegroundColor = ColorResources.HoneywellGray50,
                FontFamily = FontResources.HoneywellSansBook
            });
            formattedProductIdentifier.Spans.Add(new Span
            {
                Text = highlightedText,
                FontSize = 16,
                ForegroundColor = ColorResources.HoneywellMediumBlue,
                FontFamily = FontResources.HoneywellSansBold
            });
            ProductIdentifierLabel.FormattedText = formattedProductIdentifier;
        }

        protected virtual FormattedString GetFormattedHeaderLabel()
        {
            string placeholder1 = "placeholder1";
            string placeholder2 = "placeholder2";
            string sequence = TranslateExtension.GetLocalizedTextForBaseKey("OrderPickingWorkflowActivity_Sequence", placeholder1, placeholder2);
            var formattedHeader = new FormattedString();
            formattedHeader.Spans.Add(new Span
            {
                Text = sequence.Substring(0, sequence.IndexOf(placeholder1)),
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBook
            });
            formattedHeader.Spans.Add(new Span
            {
                Text = ((IOrderPickingViewModel)BindingContext).CurrentProductIndex,
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBlack
            });
            formattedHeader.Spans.Add(new Span
            {
                Text = sequence.Substring(sequence.IndexOf(placeholder1) + placeholder1.Length, sequence.IndexOf(placeholder2) - (sequence.IndexOf(placeholder1) + placeholder1.Length)),
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBook
            });
            formattedHeader.Spans.Add(new Span
            {
                Text = ((IOrderPickingViewModel)BindingContext).TotalProducts,
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBlack
            });
            formattedHeader.Spans.Add(new Span
            {
                Text = sequence.Substring(sequence.IndexOf(placeholder2) + placeholder2.Length),
                FontSize = 20,
                FontFamily = FontResources.HoneywellSansBook
            });
            return formattedHeader;
        }

        /// <summary>
        /// Enables the entry for stock code and sets corresponding bindings and style.
        /// </summary>
        protected void EnableStockCodeEntry()
        {
            StockCodeEntrySubview.SetBinding(UserEntrySubview.ErrorMessageProperty, "ErrorMessage");
            StockCodeEntrySubview.IsEnabled = true;
            StockCodeEntrySubview.IsAccepted = false;
        }

        /// <summary>
        /// Enables the entry for picked quantity and sets corresponding bindings and style.
        /// </summary>
        protected void EnableQuantityEntry()
        {
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