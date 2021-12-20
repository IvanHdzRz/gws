﻿using GuidedWork;
using System.Resources;

namespace BasePickingExample
{
    /// <summary>
    /// A class built from a pared-down auto-generated designer file for gaining
    /// access to the resources from BasePickingExampleResources.resx.  Since we aren't
    /// actually referencing any of the symbols generated by the designer, save
    /// some code and reduce the size of the app.
    /// </summary>
    public static class BasePickingExampleResources
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
                    _ResourceManager = EmbeddedResourceUtil.GetResourceByType(typeof(BasePickingExampleResources));
                }
                return _ResourceManager;
            }
        }
    }
}
