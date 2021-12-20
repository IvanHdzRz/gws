//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking
{
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;

    public partial class BasePickingConfigurationSettingsView : CoreView
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:BasePicking.BasePickingConfiguraitonSettingsView"/> class.
        /// </summary>
        /// <param name="viewModel">View model.</param>
        /// <param name="logger">Logger.</param>
        public BasePickingConfigurationSettingsView(BasePickingConfigurationSettingsViewModel viewModel, ILog logger) : base(viewModel, logger)
        {
            InitializeComponent();

            BindingContext = viewModel;
        }
    }
}
