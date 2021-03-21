using System;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace TehGM.Fsriev.Logging
{
    public static class LoggingInitializationExtensions
    {
        public static IHostBuilder ConfigureSerilog(this IHostBuilder builder)
            => builder.UseSerilog(ConfigureSerilog, true);

        public static void ConfigureSerilog(HostBuilderContext context, LoggerConfiguration config)
        {
            config.ReadFrom.Configuration(context.Configuration, "Logging")
                .Enrich.FromLogContext();
        }

        public static void EnableUnhandledExceptionLogging()
        {
            if (Log.Logger != null)
                return;

            // add default logger for errors that happen before host runs
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File($"{dir}/TehGM/fsriev/logs/unhandled-.log",
                fileSizeLimitBytes: 1048576,        // 1MB
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 30,
                rollingInterval: RollingInterval.Day)
                .CreateLogger();
            // capture unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Log.Error((Exception)e.ExceptionObject, "An exception was unhandled");
                Log.CloseAndFlush();
            }
            catch { }
        }
    }
}
