using System.Diagnostics;

namespace TehGM.Fsriev
{
    public static class TerminalExtensions
    {
        public static Process Execute(this ITerminal terminal, string command, string workingDirectory)
            => terminal.Execute(command, workingDirectory, false);
        public static Process Execute(this ITerminal terminal, string command)
            => Execute(terminal, command, null);
    }
}
