//////////////////////////////////////////////////////////////////
//    Copyright (C) 2020 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace ArtisanCommon
{
    using Bootstrap;
    using Honeywell.GuidedWork.Core;
    using TinyIoC;

    /// <summary>
    /// A set of methods to help an Artisan app set common dependency 
    /// overrides. Note - this file is not distributed in nugets.  It is for
    /// the SMB Artisan applications only.  Devkit and connectors that
    /// support Artisan should supply their own implemenation of this
    /// class with appropriate overrides.
    /// </summary>
    public static class DependencyOverrides
    {
        /// <summary>
        /// Register shared dependencies for the application in the specified container.
        /// </summary>
        /// <param name="container">Container.</param>
        public static void Register(TinyIoCContainer container)
        {
            AppBuildInfo.Register(container);
        }
    }
}
