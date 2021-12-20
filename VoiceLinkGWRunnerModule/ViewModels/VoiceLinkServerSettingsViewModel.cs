//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLink
{
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.WorkflowEngine;
    using System.Collections.Generic;
    using System.Windows.Input;

    public class VoiceLinkServerSettingsViewModel : NavigatingMenuViewModel
    {
        public VoiceLinkServerSettingsViewModel(WorkflowViewModelDependencies dependencies)
            : base(dependencies)
        {
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
        /// Toggle that backs the switch for enabling/disabling secure connections (https)
        /// </summary>
        private bool _ServerSecureConnections;
        public bool ServerSecureConnections
        {
            get
            {
                return _ServerSecureConnections;
            }

            set
            {
                _ServerSecureConnections = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to show and hide view components associated with the "Legacy Server Settings" portion of the view
        /// </summary>
        private bool _LegacyServerSettingsVisible;
        public bool LegacyServerSettingsVisible
        {
            get
            {
                return _LegacyServerSettingsVisible;
            }

            set
            {
                _LegacyServerSettingsVisible = value;
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

        public string ODRPort { get; set; }

        public string SiteName { get; set; }

        public ICommand OnHostEntryLostFocus { get; set; }

        public ICommand OnPortEntryLostFocus { get; set; }

        public ICommand OnODRPortEntryLostFocus { get; set; }

        public ICommand OnSiteNameEntryLostFocus { get; set; }
    }
}
