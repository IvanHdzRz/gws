//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2016 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace WarehousePicking
{
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using GuidedWork;

    public class WarehousePickingLastPickViewModel : ReadyViewModel
    {
        public string TotalCases { get; set; }
        public string RemainingCases { get; set; }
        public string LastPick { get; set; }

        public WarehousePickingLastPickViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        /// Gets or sets a bool representing whether the skip item button should be displayed.
        /// </summary>
        private bool _LastPickLabelVisible;
        public bool LastPickLabelVisible
        {
            get { return _LastPickLabelVisible; }
            set
            {
                _LastPickLabelVisible = value;
                NotifyPropertyChanged();
            }
        }
    }
}
