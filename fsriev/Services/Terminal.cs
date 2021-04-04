using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace TehGM.Fsriev.Services
{
    class Terminal : ITerminal
    {
        private readonly ILogger _log;

        public Terminal(ILogger<Terminal> logger)
        {
            this._log = logger;
        }

        /// <inheritdoc/>
        public Process Create(string command, string workingDirectory, bool asRoot)
        {
            // create process, start it and immediately return
            _log.LogDebug("Creating process: {Command}", command);
            Process prc = new Process();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string escapedCommand = command.Replace("\"", "\\\"");
                // if as root, just sudo it
                if (asRoot)
                    prc.StartInfo = new ProcessStartInfo("/bin/bash", $"-c \"sudo {escapedCommand}\"");
                // if not as root, we need to open it as current user
                // this may still run as root if the process is running as root
                else
                    prc.StartInfo = new ProcessStartInfo("/bin/bash", $"-c \"{escapedCommand}\"");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                prc.StartInfo = new ProcessStartInfo("CMD.exe", $"/C {command}");
                if (asRoot)
                    prc.StartInfo.Verb = "runas";
            }
            else
                throw new PlatformNotSupportedException($"{nameof(Terminal)} is only supported on Windows and Linux platforms.");

            prc.StartInfo.WorkingDirectory = string.IsNullOrWhiteSpace(workingDirectory) ? Environment.CurrentDirectory : workingDirectory;
            prc.StartInfo.UseShellExecute = false;
            prc.StartInfo.CreateNoWindow = true;
            return prc;
        }
    }
}
