using System;
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
                    if (watcher.FileFilters == null)
                        watcher.FileFilters = _defaultFileFilters;
                }
            }
        }
    }
}
