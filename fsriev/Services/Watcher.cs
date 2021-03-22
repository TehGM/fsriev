using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace TehGM.Fsriev.Services
{
    public class Watcher : IDisposable
    {
        public string Name { get; }
        public bool IsBusy { get; private set; }

        private readonly FileSystemWatcher _watch;
        private readonly ITerminal _terminal;
        private readonly WatcherOptions _options;
        private readonly ILogger _log;
        private readonly IEnumerable<ExclusionFilter> _exclusions;
        private bool _disposed;
        private readonly object _lock = new object();

        public Watcher(WatcherOptions options, ITerminal terminal, ILogger log)
        {
            if (string.IsNullOrWhiteSpace(options.FolderPath))
                throw new ArgumentNullException(nameof(options.FolderPath));

            this._options = options;
            this._terminal = terminal;
            this._log = log;
            this.Name = options.GetName();

            this._watch = new FileSystemWatcher(options.FolderPath);
            this._watch.NotifyFilter = options.ActionFilters;
            foreach (string filter in options.FileFilters)
                this._watch.Filters.Add(filter);
            this._exclusions = options.Exclusions?.Select(f => ExclusionFilter.Build(f)) ?? Enumerable.Empty<ExclusionFilter>();
            this._watch.IncludeSubdirectories = options.Recursive;

            this._watch.Changed += OnFileChanged;
            this._watch.Created += OnFileChanged;
            this._watch.Renamed += OnFileChanged;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            using IDisposable logScope = this._log.BeginScope(new Dictionary<string, object>
            {
                { "Watcher", this.Name },
                { "File", e.FullPath }
            });

            // if busy, don't process again
            // this is cause file watch can raise multiple events at once
            if (this.IsBusy && this._options.SkipWhenBusy)
            {
                this._log.LogDebug("Watcher {Watcher}: already busy, skipping file {File}");
                return;
            }

            lock (_lock)
            {
                // check exclusion filters
                foreach (ExclusionFilter exclusion in this._exclusions)
                {
                    this._log.LogTrace("Watcher {Watcher}: Checking exclusion {Exclusion}", this.Name, exclusion);
                    if (exclusion.Excludes(e.FullPath))
                    {
                        this._log.LogDebug("Watcher {Watcher}: File {File} matched by exclusion filter {Exclusion}, skipping",
                            this.Name, e.FullPath, exclusion);
                        return;
                    }
                }

                this.IsBusy = true;
                try
                {
                    this._log.LogInformation("File {File} changed, watcher {Watcher} running", e.FullPath, this.Name);
                    foreach (string cmd in this._options.Commands)
                    {
                        string workingDir = string.IsNullOrWhiteSpace(this._options.WorkingDirectory)
                            ? this._options.FolderPath : this._options.WorkingDirectory;
                        int status = this._terminal.ExecuteAndWait(cmd, workingDir);
                        if (status != 0)
                            this._log.LogError("Watcher {Watcher} exited with error code {Code}", this.Name, status);
                    }
                }
                catch (Exception ex) when (ex.LogAsError(this._log, "An exception occured in watcher {Watcher}")) { }
                finally
                {
                    this.IsBusy = false;
                }
            }
        }

        public void Start()
        {
            this._log.LogDebug("Starting watcher {Watcher}", this.Name);
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().Name);
            this._watch.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            this._log.LogDebug("Stopping watcher {Watcher}", this.Name);
            if (this._disposed)
                throw new ObjectDisposedException(this.GetType().Name);
            this._watch.EnableRaisingEvents = false;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try { this._watch.Changed -= OnFileChanged; } catch { }
            try { this._watch.Created -= OnFileChanged; } catch { }
            try { this._watch.Renamed -= OnFileChanged; } catch { }
            try { this._watch.Dispose(); } catch { }
            this._disposed = true;
        }

        public override string ToString()
            => this.Name;
    }
}
