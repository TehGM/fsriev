using System.Diagnostics;

namespace TehGM.Fsriev
{
    public static class TerminalExtensions
    {
        public static Process Execute(this ITerminal terminal, string command, string workingDirectory)
            => terminal.Execute(command, workingDirectory, false);
        public static Process Execute(this ITerminal terminal, string command)
            => Execute(terminal, command, null);

        public static int ExecuteAndWait(this ITerminal terminal, string command, string workingDirectory, bool asRoot)
        {
            using Process prc = terminal.Execute(command, workingDirectory, asRoot);
            prc.WaitForExit();
            return prc.ExitCode;
        }
        public static int ExecuteAndWait(this ITerminal terminal, string command, string workingDirectory)
            => ExecuteAndWait(terminal, command, workingDirectory, false);
        public static int ExecuteAndWait(this ITerminal terminal, string command)
            => ExecuteAndWait(terminal, command, null);
    }
}
