using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace TehGM.Fsriev
{
    /// <summary>Represents options for one watcher instance.</summary>
    public class WatcherOptions
    {
        /// <summary>Name of the watcher.</summary>
        public string Name { get; set; }

        /// <summary>Is this watcher enabled?</summary>
        /// <remarks>Defaults to true.</remarks>
        public bool Enabled { get; set; } = true;
        /// <summary>Skip processing of the file event when already busy.</summary>
        /// <remarks>Watcher might receive multiple events at once. This switch controls if watcher should ignore them while already processing one.
        /// Defaults to true.</remarks>
        public bool SkipWhenBusy { get; set; } = true;

        /// <summary>Folder to watch by the watcher.</summary>
        [Required]
        public string FolderPath { get; set; }
        /// <summary>File filters</summary>
        /// <remarks>Defaults to '*.scss' and '*.js'.</remarks>
        public IEnumerable<string> FileFilters { get; set; }
        /// <summary>Watch folder recursively.</summary>
        /// <remarks>Defaults to true.</remarks>
        public bool Recursive { get; set; } = true;
        /// <summary>Working directory to run commands in.</summary>
        /// <remarks>If not specified, will use <see cref="FolderPath"/>.</remarks>
        public string WorkingDirectory { get; set; } = null;
        /// <summary>Commands to run when matching files change.</summary>
        /// <remarks>Defaults to single command 'webcompiler -r .'.</remarks>
        public IEnumerable<string> Commands { get; set; }
        /// <summary>Filesystem actions that will trigger the watcher command.</summary>
        /// <remarks>Defaults to <see cref="NotifyFilters.LastWrite"/> | <see cref="NotifyFilters.FileName"/>.</remarks>
        public NotifyFilters ActionFilters { get; set; } = NotifyFilters.LastWrite | NotifyFilters.FileName;
        /// <summary>Patterns that will cause the change to be skipped.</summary>
        public IEnumerable<string> Exclusions { get; set; }
    }
}
