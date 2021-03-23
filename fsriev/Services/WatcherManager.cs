using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TehGM.Fsriev.Services
{
    public class WatcherManager : IWatcherManager, IHostedService, IDisposable
    {
        private readonly ICollection<Watcher> _watchers;
        private readonly ITerminal _terminal;
        private readonly ILogger _log;
        private readonly ILogger _watcherLog;

        private readonly IDisposable _optionsChangeRegistration;
        private bool _started;
        private readonly object _lock = new object();

        public WatcherManager(IOptionsMonitor<ApplicationOptions> options, ITerminal terminal, 
            ILogger<WatcherManager> log, ILogger<Watcher> watcherLog)
        {
            this._watchers = new List<Watcher>();
            this._terminal = terminal;
            this._log = log;
            this._watcherLog = watcherLog;

            this._optionsChangeRegistration = options.OnChange(options =>
                this.InitializeWatchers(options.Watchers));

            this.InitializeWatchers(options.CurrentValue.Watchers);
        }

        private void InitializeWatchers(IEnumerable<WatcherOptions> options)
        {
            lock (_lock)
            {
                bool wasRunning = this._started;

                // kill old watchers in case it's options change
                if (this._watchers.Any())
                {
                    this._log.LogInformation("Options changed, killing old watchers");
                    this.KillWatchers();
                }

                if (options?.Any() != true)
                {
                    this._log.LogInformation("No watchers to initialize");
                    this._started = false;
                    return;
                }

                // get all enabled watchers, and start them
                IEnumerable<WatcherOptions> enabledWatchers = options.Where(w => w.Enabled);
                int enabledCount = enabledWatchers.Count();
                int disabledCount = options.Count() - enabledCount;
                this._log.LogInformation("Initializing {Count} watchers", enabledCount);
                if (disabledCount != 0)
                    this._log.LogDebug("Disabled watchers: {Count}", disabledCount);
                foreach (WatcherOptions opts in enabledWatchers)
                {
                    try
                    {
                        this._watchers.Add(new Watcher(opts, this._terminal, this._watcherLog));
                    }
                    catch (Exception ex) when (ex.LogAsError(this._log, "Error when creating watcher {Watcher}", opts.GetName())) { }
                }
                if (wasRunning)
                    this.StartWatchersInternal();
            }
        }

        public void StartWatchers()
        {
            lock (_lock)
            {
                if (this._started)
                    return;
                this.StartWatchersInternal();
            }
        }

        public void StopWatchers()
        {
            lock (_lock)
            {
                if (!this._started)
                    return;
                this.StopWatchersInternal();
            }
        }

        private void StartWatchersInternal()
        {
            this._started = true;
            foreach (Watcher watcher in this._watchers)
                watcher.Start();
        }

        private void StopWatchersInternal()
        {
            this._started = false;
            foreach (Watcher watcher in this._watchers)
                watcher.Stop();
        }

        private void KillWatchers()
        {
            if (this._started)
                this.StopWatchersInternal();
            foreach (Watcher watcher in this._watchers)
                watcher.Dispose();
            this._watchers.Clear();
        }

        public void Dispose()
        {
            try { this._optionsChangeRegistration.Dispose(); } catch { }
            lock (_lock)
                try { this.KillWatchers(); } catch { }
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            this.StartWatchers();
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            this.StopWatchers();
            return Task.CompletedTask;
        }
    }
}
