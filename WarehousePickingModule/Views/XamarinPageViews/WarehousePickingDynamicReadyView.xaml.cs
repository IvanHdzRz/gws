// <copyright company="HONEYWELL INTERNATIONAL INC">
//---------------------------------------------------------------------
//   COPYRIGHT (C) 2014-2015 HONEYWELL INTERNATIONAL INC. and/or one of
//   its wholly-owned subsidiaries, including Hand Held Products, Inc.,
//   Intermec, Inc., and/or Vocollect, Inc.
//   UNPUBLISHED - ALL RIGHTS RESERVED UNDER THE COPYRIGHT LAWS.
//   PROPRIETARY AND CONFIDENTIAL INFORMATION. DISTRIBUTION, USE
//   AND DISCLOSURE RESTRICTED BY HONEYWELL INTERNATIONAL INC.
//---------------------------------------------------------------------
// </copyright>

namespace WarehousePicking
{
    using Common.Logging;
    using Honeywell.Firebird.CoreLibrary;
    using GuidedWork;

    public partial class WarehousePickingDynamicReadyView : CoreView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadyView"/> class.
        /// </summary>
        /// <param name="viewModel">View model.</param>
        /// <param name="logger">A logger.</param>
        public WarehousePickingDynamicReadyView(ReadyViewModel viewModel, ILog logger)
            : base(viewModel, logger)
        {
            InitializeComponent();

            BindingContext = viewModel;
        }
    }
}
