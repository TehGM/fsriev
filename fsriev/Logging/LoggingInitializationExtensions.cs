using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace TehGM.Fsriev.Logging
{
    public static class LoggingInitializationExtensions
    {
        private const string _sectionName = "Logging";

        public static IHostBuilder ConfigureSerilog(this IHostBuilder builder)
            => builder.UseSerilog(ConfigureSerilog, true);

        public static void ConfigureSerilog(HostBuilderContext context, LoggerConfiguration config)
        {
            if (context.Configuration.GetSection(_sectionName).Exists())
                config.ReadFrom.Configuration(context.Configuration, _sectionName);
            else
                config
                    .AddSharedConfiguration()
                    .AddFileDefaults("log");
        }

        private static LoggerConfiguration AddSharedConfiguration(this LoggerConfiguration config)
        {
            return config
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Warning)
                .WriteTo.Console();
        }

        private static LoggerConfiguration AddFileDefaults(this LoggerConfiguration config, string logFileName)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrWhiteSpace(logFileName))
                throw new ArgumentNullException(nameof(logFileName));
            // get file path
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string path = Path.Combine(dir, "TehGM/fsriev/logs/", $"{logFileName}-.log");
            // build config
            return config.WriteTo.Async(sink =>
                sink.File(path,
                    fileSizeLimitBytes: 1048576,        // 1MB
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 30,
                    rollingInterval: RollingInterval.Day));
        }

        public static void EnableUnhandledExceptionLogging()
        {
            // add default logger for errors that happen before host runs
            Log.Logger = new LoggerConfiguration()
                .AddSharedConfiguration()
                .AddFileDefaults("unhandled")
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
