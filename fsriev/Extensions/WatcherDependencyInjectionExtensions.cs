using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TehGM.Fsriev;
using TehGM.Fsriev.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WatcherDependencyInjectionExtensions
    {
        public static IServiceCollection AddWatchers(this IServiceCollection services, Action<ApplicationOptions> configureOptions = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddTransient<ITerminal, Terminal>();
            services.AddSingleton<IWatcherManager, WatcherManager>();
            services.AddTransient<IHostedService>(provider => (IHostedService)provider.GetRequiredService<IWatcherManager>());

            if (configureOptions != null)
                services.Configure(configureOptions);
            services.AddSingleton<IPostConfigureOptions<ApplicationOptions>, ConfigureWatcherOptions>();

            return services;
        }

        public class ConfigureWatcherOptions : IPostConfigureOptions<ApplicationOptions>
        {
            private static readonly string[] _defaultFileFilters = new string[] { "*" };

            public void PostConfigure(string name, ApplicationOptions options)
            {
                foreach (WatcherOptions watcher in options.Watchers)
                {
                    // expand paths
                    watcher.FolderPath = ExpandPath(watcher.FolderPath);
                    watcher.WorkingDirectory = ExpandPath(watcher.WorkingDirectory);

                    // normalize paths
                    watcher.FolderPath = NormalizePath(watcher.FolderPath);
                    watcher.WorkingDirectory = NormalizePath(watcher.WorkingDirectory);

                    // verify used filters
                    if (watcher.FileFilters == null)
                        watcher.FileFilters = _defaultFileFilters;
                }
            }

            private static string NormalizePath(string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                    return null;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return path.Replace('/', '\\');
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return path.Replace('\\', '/');

                // fallback: just return unchanged
                return path;
            }

            private static string ExpandPath(string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                    return path;

                return Environment.ExpandEnvironmentVariables(path);
            }
        }
    }
}
