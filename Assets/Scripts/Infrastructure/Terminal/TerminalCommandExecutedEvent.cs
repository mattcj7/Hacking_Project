using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Terminal
{
    public readonly struct TerminalCommandExecutedEvent : IEvent
    {
        public TerminalCommandExecutedEvent(string command, string[] args, string currentPath, string resolvedPath)
        {
            Command = command;
            Args = args;
            CurrentPath = currentPath;
            ResolvedPath = resolvedPath;
        }

        public string Command { get; }
        public string[] Args { get; }
        public string CurrentPath { get; }
        public string ResolvedPath { get; }
    }
}
