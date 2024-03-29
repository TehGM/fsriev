﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TehGM.Fsriev.Services
{
    public class Watcher : IDisposable
    {
        public string Name { get; }
        public bool IsBusy { get; private set; }

        // options and services
        private readonly WatcherOptions _options;
        private readonly ApplicationOptions _applicationOptions;
        private readonly FileSystemWatcher _watch;
        private readonly IEnumerable<string> _commands;
        private readonly ITerminal _terminal;
        private readonly ILogger _log;
        // calculated options cache
        private readonly IEnumerable<ExclusionFilter> _exclusions;
        private readonly string _workingDirectory;
        // flow control
        private bool _disposed;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly object _outputLock = new object();

        public Watcher(WatcherOptions options, ApplicationOptions applicationOptions, ITerminal terminal, ILogger log)
        {
            if (string.IsNullOrWhiteSpace(options.FolderPath))
                throw new ArgumentNullException(nameof(options.FolderPath));

            this._log = log;
            this._options = options;
            this._applicationOptions = applicationOptions;
            this._terminal = terminal;
            this.Name = options.GetName();

            this._log.LogTrace("Watcher {Watcher}: checking commands", this.Name);
            this._commands = options.Commands?.Where(c => !string.IsNullOrWhiteSpace(c)) ?? Enumerable.Empty<string>();
            if (!this._commands.Any())
                this._log.LogWarning("Watcher {Watcher}: no commands specified", this.Name);

            this._log.LogTrace("Watcher {Watcher}: building exclusion filters", this.Name);
            this._exclusions = options.Exclusions?.Select(f => ExclusionFilter.Build(f)) ?? Enumerable.Empty<ExclusionFilter>();

            this._log.LogTrace("Watcher {Watcher}: determining working directory", this.Name);
            this._workingDirectory = string.IsNullOrWhiteSpace(this._options.WorkingDirectory) ? this._options.FolderPath : this._options.WorkingDirectory;

            this._log.LogTrace("Watcher {Watcher}: creating {Type}", this.Name, typeof(FileSystemWatcher).Name);
            this._watch = new FileSystemWatcher(options.FolderPath);
            this._watch.NotifyFilter = options.NotifyFilters;
            foreach (string filter in options.FileFilters)
                this._watch.Filters.Add(filter);
            this._watch.IncludeSubdirectories = options.Recursive;
            this._watch.Changed += OnFileChanged;
            this._watch.Created += OnFileChanged;
            this._watch.Renamed += OnFileChanged;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
            => OnFileChangedAsync(sender, e).GetAwaiter().GetResult();

        private async Task OnFileChangedAsync(object sender, FileSystemEventArgs e)
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
                this._log.LogDebug("Watcher {Watcher}: Already busy, skipping file {File}");
                return;
            }

            CancellationToken cancellationToken = this._cts.Token;
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // check exclusion filters
                // do it before marking as busy, to prevent skipping trigger when at least one change wasn't on exclusion list
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

                // flag as busy to prevent multi-file changes spam
                this.IsBusy = true;
                try
                {
                    if (this._commands.Any())
                    {
                        this._log.LogInformation("Watcher {Watcher}: File {File} changed, running commands");
                        foreach (string cmd in this._options.Commands)
                            await this.RunProcessAsync(cmd, cancellationToken);
                        this._log.LogInformation("Watcher {Watcher}: Done executing commands");
                    }
                    else
                        this._log.LogInformation("Watcher {Watcher}: File {File} changed");
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) when (ex.LogAsError(this._log, "An exception occured in watcher {Watcher}")) { }
                finally
                {
                    this.IsBusy = false;
                }
            }
            finally
            {
                this._lock.Release();
            }
        }

        private async Task RunProcessAsync(string command, CancellationToken cancellationToken)
        {
            // execute and wait
            using Process prc = this._terminal.Create(command, this._workingDirectory);

            if (this._options.ShowCommandOutput)
            {
                prc.StartInfo.RedirectStandardOutput = true;
                prc.StartInfo.RedirectStandardError = true;
                prc.OutputDataReceived += (sender, e) => HandleProcessOutput(e.Data, false);
                prc.ErrorDataReceived += (sender, e) => HandleProcessOutput(e.Data, true);
            }

            try
            {
                prc.Start();
                if (this._options.ShowCommandOutput)
                {
                    prc.BeginOutputReadLine();
                    prc.BeginErrorReadLine();
                }
                await prc.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
                if (prc.ExitCode != 0)
                    this._log.LogError("Watcher {Watcher}: Process exited with error code {Code}", this.Name, prc.ExitCode);
                else
                    this._log.LogTrace("Watcher {Watcher}: Done executing process");
            }
            catch (OperationCanceledException)
            {
                // if didn't exit, but watcher is disposing, then force kill the process
                if (!prc.HasExited)
                {
                    this._log.LogDebug("Watcher {Watcher}: Force killing process {Process}", this.Name, command);
                    prc.Kill(true);
                }
                throw;
            }
        }

        private void HandleProcessOutput(string output, bool isError)
        {
            if (output == null)
                return;

            lock (_outputLock)
            {
                CommandOutputMode mode = this._applicationOptions.CommandOutputMode;
                if ((mode & CommandOutputMode.Console) == CommandOutputMode.Console)
                {
                    ConsoleColor previousColor = Console.ForegroundColor;
                    if (isError)
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(output);
                    if (isError)
                        Console.ForegroundColor = previousColor;
                }
                if ((mode & CommandOutputMode.Log) == CommandOutputMode.Log)
                {
                    LogLevel level = isError ? LogLevel.Error : this._applicationOptions.CommandOutputLevel;
                    this._log.Log(level, output);
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

            // kill the watcher
            try { this._watch.Changed -= OnFileChanged; } catch { }
            try { this._watch.Created -= OnFileChanged; } catch { }
            try { this._watch.Renamed -= OnFileChanged; } catch { }
            try { this._watch.Dispose(); } catch { }
            // cancel any running operation
            try { this._cts.Cancel(); } catch { }
            try { this._cts.Dispose(); } catch { }
            // dispose locks
            try { this._lock.Dispose(); } catch { }
            this._disposed = true;
        }

        public override string ToString()
            => this.Name;
    }
}
