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

    public partial class WarehousePickingAcknowledgeLocationView : CoreView
    {
        public WarehousePickingAcknowledgeLocationView(WarehousePickingAcknowledgeLocationViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;

            AddButton(0, "SkipButtonText", "SecondaryButtonStyle", "ValidationModel.SubmitResponseCommand", "WarehousePickingAcknowledgeLocationViewSkipButton", TranslateExtension.GetLocalizedTextForBaseKey("VocabWord_SkipProduct"));
            AddButton(1, "ReadyButtonText", "PrimaryButtonStyle", "ValidationModel.SubmitResponseCommand", "WarehousePickingAcknowledgeLocationViewReadyButton", TranslateExtension.GetLocalizedTextForBaseKey("next_entry_word"));
        }

        public virtual void OnClick(object sender, EventArgs e)
        {
            var viewModel = ViewModel as WarehousePickingAcknowledgeLocationViewModel;
            ToolbarItem tbi = (ToolbarItem)sender;
            viewModel.ValidationModel.SubmitResponseCommand?.Execute(tbi.Text);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            BuildLocationLayout(((IWarehousePickingViewModel)BindingContext).LocationDescriptors);
            DisplaySequenceLayout();
            SetFormattedLabels();
            UpdateOverflowMenu();
        }

        private void UpdateOverflowMenu()
        {
            var existing = new Dictionary<string, ToolbarItem>();
            var viewModel = ViewModel as WarehousePickingAcknowledgeLocationViewModel;

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

                    tbi.Clicked += OnClick;
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
            SetFormattedSequenceLabel();
        }

        protected void SetFormattedProductIdentifierLabel()
        {
            string productIdentifier = ((IWarehousePickingViewModel)BindingContext).ProductIdentifier;
            var formattedProductIdentifier = new FormattedString();
            formattedProductIdentifier.Spans.Add(new Span
            {
                Text = productIdentifier.Substring(0, productIdentifier.Length - 3),
                FontSize = 14,
                ForegroundColor = ColorResources.HoneywellGray50,
                FontFamily = FontResources.HoneywellSansBook
            });
            formattedProductIdentifier.Spans.Add(new Span
            {
                Text = productIdentifier.Substring(productIdentifier.Length - 3),
                FontSize = 16,
                ForegroundColor = ColorResources.HoneywellMediumBlue,
                FontFamily = FontResources.HoneywellSansBold
            });
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