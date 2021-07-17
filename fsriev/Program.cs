using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TehGM.Fsriev.Logging;

namespace TehGM.Fsriev
{
    class Program
    {
        public const string Name = "fsriev";
        public const string Author = "TehGM";
        public const string CopyrightYear = "2021";
        public static readonly string Version = FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).ProductVersion;

        static async Task Main(string[] args)
        {
            Console.Title = $"{Name} v{Version} by {Author}";
            Program.WriteCopyright();
            LoggingInitializationExtensions.EnableUnhandledExceptionLogging();

            Console.WriteLine("Starting {0} host. Press Ctrl+C to stop and exit.", Name); ;
            Console.WriteLine();

            IHost host = new HostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureHostConfiguration(builder =>
                {
                    // add env variables
                    builder.AddEnvironmentVariables("DOTNET_");

                    // add args
                    builder.AddCommandLine(args);
                })
                .ConfigureAppConfiguration((context, builder) =>
                {
                    // add app settings
                    builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    // add log settings
                    builder.AddJsonFile("logsettings.json", optional: true, reloadOnChange: true);
                    builder.AddJsonFile($"logsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    // add env variables
                    builder.AddEnvironmentVariables();
                    builder.AddEnvironmentVariables("FSRIEV_");

                    // add args
                    builder.AddCommandLine(args);
                })
                .ConfigureSerilog()
                .ConfigureServices((context, services) =>
                {
                    services.Configure<ApplicationOptions>(context.Configuration);
                    services.AddWatchers();
                })
                .UseDefaultServiceProvider((context, options) =>
                {
                    bool isDevelopment = context.HostingEnvironment.IsDevelopment();
                    options.ValidateScopes = isDevelopment;
                    options.ValidateOnBuild = isDevelopment;
                })
                .Build();
            await host.RunAsync().ConfigureAwait(false);
        }

        private static void WriteCopyright()
        {
            Console.WriteLine("{0} v{1} by {2}", Name, Version, Author);
            Console.WriteLine("Copyright (c) {0}, {1}", CopyrightYear, Author);
            Console.WriteLine("Repository: https://github.com/{0}/{1}", Author, Name);
            Console.WriteLine();
        }
    }
}
