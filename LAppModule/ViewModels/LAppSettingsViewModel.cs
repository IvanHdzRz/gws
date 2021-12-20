//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace LApp
{
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using System.Collections.Generic;
    using System.Windows.Input;

    public class LAppSettingsViewModel : CoreViewModel
    {
        private readonly ILAppConfigRepository _LAppConfigRepository;

        public LAppSettingsViewModel(WorkflowViewModelDependencies dependencies,
                                     ILAppConfigRepository lappConfigRepository) : base(dependencies)
        {
            _LAppConfigRepository = lappConfigRepository;
        }

        /// <summary>
        /// Source for the WorkflowFilterPicker
        /// This isn't an ObservableCollection like the SiteChoices property since this list is static
        /// and will never need to be updated at run time
        /// </summary>
        public IList<string> WorkflowFilterChoices { get; set; }

        /// <summary>
        /// The currently selected workflow filter
        /// </summary>
        private string _SelectedWorkflowFilter;
        public string SelectedWorkflowFilter
        {
            get
            {
                return _SelectedWorkflowFilter;
            }

            set
            {
                _SelectedWorkflowFilter = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to show and hide view components associated with the "Server Settings" portion of the view
        /// </summary>
        private bool _ServerSettingsVisible;
        public bool ServerSettingsVisible
        {
            get
            {
                return _ServerSettingsVisible;
            }

            set
            {
                _ServerSettingsVisible = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to show and hide the "Workflow Filter Selection" portion of the view
        /// </summary>
        /// <value><c>true</c> if application mode selection visible; otherwise, <c>false</c>.</value>
        private bool _WorkflowFilterSettingsVisible;
        public bool WorkflowFilterSettingsVisible
        {
            get
            {
                return _WorkflowFilterSettingsVisible;
            }

            set
            {
                _WorkflowFilterSettingsVisible = value;
                NotifyPropertyChanged();
            }
        }

        public string Host { get; set; }

        public string Port { get; set; }

        public string CompanyDB { get; set; }

        public ICommand OnHostEntryLostFocus { get; set; }

        public ICommand OnPortEntryLostFocus { get; set; }

        public ICommand OnCompanyDBEntryLostFocus { get; set; }
    }
}
