﻿//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace VoiceLinkArtisanModule
{
    using GuidedWork;
    using System.Resources;

    /// <summary>
    /// A class built from a pared-down auto-generated designer file for gaining
    /// access to the resources from Dynamics365Resources.resx.  Since we aren't
    /// actually referencing any of the symbols generated by the designer, save
    /// some code and reduce the size of the app.
    /// </summary>
    public static class VoiceLinkArtisanResources
    {
        private static ResourceManager _ResourceManager;

        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        public static ResourceManager ResourceManager
        {
            get
            {
                if (ReferenceEquals(_ResourceManager, null))
                {
                    _ResourceManager = EmbeddedResourceUtil.GetResourceByType(typeof(VoiceLinkArtisanResources));
                }
                return _ResourceManager;
            }
        }
    }
}
