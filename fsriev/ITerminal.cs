using System.Diagnostics;

namespace TehGM.Fsriev
{
    public interface ITerminal
    {
        /// <summary>Creates a terminal process for command <paramref name="command"/> and returns the process handle.</summary>
        /// <param name="command">Command to create.</param>
        /// <param name="asRoot">Should the terminal start as root user? If false, will execute as current user.</param>
        /// <remarks>Current implementation always runs as current user on Windows.</remarks>
        /// <returns>Started process.</returns>
        Process Create(string command, string workingDirectory, bool asRoot);
    }
}
