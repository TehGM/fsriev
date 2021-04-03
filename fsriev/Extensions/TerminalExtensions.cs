using System.Diagnostics;

namespace TehGM.Fsriev
{
    public static class TerminalExtensions
    {
        public static Process Create(this ITerminal terminal, string command, string workingDirectory)
            => terminal.Create(command, workingDirectory, false);
        public static Process Create(this ITerminal terminal, string command)
            => Create(terminal, command, null);

        public static Process Execute(this ITerminal terminal, string command, string workingDirectory, bool asRoot)
        {
            Process prc = terminal.Create(command, workingDirectory, asRoot);
            prc.Start();
            return prc;
        }
        public static Process Execute(this ITerminal terminal, string command, string workingDirectory)
            => terminal.Execute(command, workingDirectory, false);
        public static Process Execute(this ITerminal terminal, string command)
            => Execute(terminal, command, null);
    }
}
