//////////////////////////////////////////////////////////////////////////////
//    Copyright (C) 2019 Honeywell International Inc. All rights reserved.
//////////////////////////////////////////////////////////////////////////////

namespace BasePicking.Artisan
{
    using System.Threading.Tasks;
    using GuidedWork.Devices.NetCore;
    using GuidedWorkRunner;
    using Honeywell.Firebird;
    using TinyIoC;
    using BasePicking;

    /// <summary>
    /// Override of base application class to register application
    /// and an dependencies application may have
    /// </summary>
    class NetCoreApplication : BaseNetCoreApplication
    {
        /// <summary>
        /// Method contaiing app-specific registrations that need to be added before
        /// starting app.
        /// </summary>
        /// <param name="container">The TinyIoc container for the application.</param>
        protected override void RegisterDependencies(TinyIoCContainer container)
        {
            container.Register<IAppBootstrap, AppBootstrap>();
            ArtisanCommon.DependencyOverrides.Register(container);
        }

        /// <summary>
        /// Method contaiing app-specific registrations.
        /// </summary>
        /// <param name="container">The TinyIoc container for the application.</param>
        protected override void RegisterApplication(TinyIoCContainer container)
        {
            //==============================================================================================
            //Register Application modules
            Dependencies.RegisterAppModule<BasePickingModule, IGenericMobilDataExchange<IBasePickingModel>, IBasePickingModel, IBasePickingConfigRepository>(container);
        }
    }

    /// <summary>
    /// Run main application
    /// </summary>
    class Program
    {
        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        static Task<int> Main(string[] args)
        {
            ExternalLogWriter.ProcessLogMessage = ProcessLogMessage;
            var application = new NetCoreApplication();
            return application.HandleArgsAsync(args);
        }

        /// <summary>
        /// Method to alter or skip log message to hide/mask/encrypt sensitive
        /// information in the logs.
        /// </summary>
        /// <param name="message">Message to send to logs</param>
        /// <returns>altered message to actually log, Empty or null string to keep out of logs altogether</returns>
        private static string ProcessLogMessage(string message)
        {
            //Replace password field in sign in slots
            if (message.Contains("\"slots\""))
            {
                message = SlotContainer.FormatSlotsLogMessage(message);
            }
            return message;
        }
    }
}
