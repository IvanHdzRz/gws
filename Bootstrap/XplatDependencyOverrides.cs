//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2018 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace Bootstrap
{
    using Bootstrap.Services;
    using Honeywell.GuidedWork.Base;
    using TinyIoC;

    /// <summary>
    /// A set of methods to help an app set cross-platform dependency 
    /// overrides.
    /// </summary>
    public static class XplatDependencyOverrides
    {
        /// <summary>
        /// Call this method to set any overrides of the Guided Work system,
        /// prior to initializing it.
        /// </summary>
        public static void SetPreInitOverrides(TinyIoCContainer container)
        {
            // For example, provide your own branded Navigation bar by
            // pre-registering a replacement for the Honeywell-branded default.
            //container.Register<INavigationBarService, MyNavigationBarService>().AsMultiInstance();

            AppBuildInfo.Register(container);

            // Register cross-platform services
            container.Register<ILanguageAvailabilityService, LanguageAvailabilityService>();
        }
    }
}
