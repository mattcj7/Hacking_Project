using System;
using HackingProject.Infrastructure.Save;
using HackingProject.Infrastructure.Vfs;

namespace HackingProject.Infrastructure.Terminal
{
    public sealed class TerminalCommandProcessor
    {
        private readonly VirtualFileSystem _vfs;
        private readonly TerminalSession _session;
        private readonly OsSessionData _sessionData;

        public TerminalCommandProcessor(VirtualFileSystem vfs, TerminalSession session, OsSessionData sessionData)
        {
            _vfs = vfs ?? throw new ArgumentNullException(nameof(vfs));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _sessionData = sessionData;
        }

        public TerminalCommandResult Execute(string input)
        {
            if (!TerminalCommandParser.TryParse(input, out var command))
            {
                return TerminalCommandResult.Empty;
            }

            var commandName = command.Name.ToLowerInvariant();
            switch (commandName)
            {
                case "help":
                    return new TerminalCommandResult(new[]
                    {
                        "Commands: help, pwd, ls, cd, cat, clear"
                    }, false);
                case "pwd":
                    return new TerminalCommandResult(new[] { _session.CurrentPath }, false);
                case "ls":
                    return ExecuteList();
                case "cd":
                    return ExecuteChangeDirectory(command.Args);
                case "cat":
                    return ExecuteCat(command.Args);
                case "clear":
                    return new TerminalCommandResult(Array.Empty<string>(), true);
                default:
                    return new TerminalCommandResult(new[] { $"Unknown command: {command.Name}" }, false);
            }
        }

        private TerminalCommandResult ExecuteList()
        {
            var children = _session.CurrentDirectory.Children;
            if (children.Count == 0)
            {
                return new TerminalCommandResult(new[] { "(empty)" }, false);
            }

            var lines = new string[children.Count];
            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                lines[i] = child is VfsDirectory ? $"{child.Name}/" : child.Name;
            }

            return new TerminalCommandResult(lines, false);
        }

        private TerminalCommandResult ExecuteChangeDirectory(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return new TerminalCommandResult(new[] { "cd: path required" }, false);
            }

            var target = args[0];
            var node = ResolvePath(target);
            if (node is VfsDirectory directory)
            {
                _session.SetCurrentDirectory(directory);
                if (_sessionData != null)
                {
                    _sessionData.TerminalCwdPath = _session.CurrentPath;
                }
                return TerminalCommandResult.Empty;
            }

            return new TerminalCommandResult(new[] { $"cd: no such directory: {target}" }, false);
        }

        private TerminalCommandResult ExecuteCat(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return new TerminalCommandResult(new[] { "cat: path required" }, false);
            }

            var target = args[0];
            var node = ResolvePath(target);
            if (node is VfsFile file)
            {
                return new TerminalCommandResult(new[] { file.Content }, false);
            }

            return new TerminalCommandResult(new[] { $"cat: not a file: {target}" }, false);
        }

        private VfsNode ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return _session.CurrentDirectory;
            }

            var combined = path.StartsWith("/", StringComparison.Ordinal)
                ? path
                : _session.CurrentPath == "/"
                    ? $"/{path}"
                    : $"{_session.CurrentPath}/{path}";

            return _vfs.Resolve(combined);
        }
    }
}
