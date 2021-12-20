//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2017 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Receiving
{
    using System;
    using System.Collections.Generic;
    using Common.Logging;
    using Xamarin.Forms;

    public partial class ReceivingEnterHiQuantityView : ReceivingView
    {
        public ReceivingEnterHiQuantityView(ReceivingEnterDigitsViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();
            BindingContext = viewModel;
            EnableHiQuantityEntry();
        }

        public virtual void OnClick(object sender, EventArgs e)
        {
            var viewModel = ViewModel as ReceivingEnterDigitsViewModel;
            ToolbarItem tbi = (ToolbarItem)sender;
            viewModel.ValidationModel.SubmitResponseCommand?.Execute(tbi.Text);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            ResetHiQuantityEntry();
            UpdateOverflowMenu();
        }

        private void UpdateOverflowMenu()
        {
            var existing = new Dictionary<string, ToolbarItem>();
            var viewModel = ViewModel as ReceivingEnterDigitsViewModel;

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
    }
}
