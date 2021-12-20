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

namespace GWS
{
    using Honeywell.Firebird;
    using Honeywell.Firebird.CoreLibrary;
    using Honeywell.Firebird.Module.XamarinForms;

    /// <summary>
    /// Forms app, copied from Firebird.  We really don't want it doing this for us, but with an attached
    /// Resource Dictionary.
    /// </summary>
    public partial class App : MasterDetailApplication
    {
        public App(IApplicationStateChangeService s, XamarinMasterDetailPage m) : base(s, m)
        {
            InitializeComponent();
        }
    }
}
