//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2014 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace GWS.iOS
{
    using Honeywell.Firebird.CoreLibrary.iOS;
    using UIKit;

    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.

            var wrapper = new ExceptionWrapper();
            wrapper.WrapAction(() => UIApplication.Main(args, null, "AppDelegate"));
        }
    }
}
