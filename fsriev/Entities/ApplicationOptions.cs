using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace TehGM.Fsriev
{
    public class ApplicationOptions
    {
        /// <summary>Output mode to use for command output.</summary>
        public CommandOutputMode CommandOutputMode { get; set; } = CommandOutputMode.Console;
        /// <summary>Log level used for output.</summary>
        /// <remarks><para>Only applicable when <see cref="CommandOutputMode"/> is set to <see cref="CommandOutputMode.Log"/>.</para>
        /// <para>This affects only normal output. STDERR output will always be logged as <see cref="LogLevel.Error"/>.</para></remarks>
        public LogLevel CommandOutputLevel { get; set; } = LogLevel.Information;

        /// <summary>Configs for all watchers.</summary>
        public IEnumerable<WatcherOptions> Watchers { get; set; }
    }
}
