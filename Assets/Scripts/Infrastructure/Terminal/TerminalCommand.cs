using System;

namespace HackingProject.Infrastructure.Terminal
{
    public readonly struct TerminalCommand
    {
        public TerminalCommand(string name, string[] args)
        {
            Name = name ?? string.Empty;
            Args = args ?? Array.Empty<string>();
        }

        public string Name { get; }
        public string[] Args { get; }
    }
}
