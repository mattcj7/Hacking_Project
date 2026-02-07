using System;

namespace HackingProject.Infrastructure.Terminal
{
    public readonly struct TerminalCommandResult
    {
        public TerminalCommandResult(string[] outputLines, bool clearOutput)
        {
            OutputLines = outputLines ?? Array.Empty<string>();
            ClearOutput = clearOutput;
        }

        public string[] OutputLines { get; }
        public bool ClearOutput { get; }

        public static TerminalCommandResult Empty => new TerminalCommandResult(Array.Empty<string>(), false);
    }
}
