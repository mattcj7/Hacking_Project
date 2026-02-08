using System;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Save;
using HackingProject.Infrastructure.Vfs;

namespace HackingProject.Infrastructure.Terminal
{
    public sealed class TerminalCommandProcessor
    {
        private readonly VirtualFileSystem _vfs;
        private readonly TerminalSession _session;
        private readonly OsSessionData _sessionData;
        private readonly EventBus _eventBus;

        public TerminalCommandProcessor(VirtualFileSystem vfs, TerminalSession session, OsSessionData sessionData, EventBus eventBus)
        {
            _vfs = vfs ?? throw new ArgumentNullException(nameof(vfs));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _sessionData = sessionData;
            _eventBus = eventBus;
        }

        public TerminalCommandResult Execute(string input)
        {
            if (!TerminalCommandParser.TryParse(input, out var command))
            {
                return TerminalCommandResult.Empty;
            }

            var cwd = _session.CurrentPath;
            var resolvedPath = ResolvePathForArgs(command.Args, cwd);
            var commandName = command.Name.ToLowerInvariant();
            TerminalCommandResult result;
            switch (commandName)
            {
                case "help":
                    result = new TerminalCommandResult(new[]
                    {
                        "Commands: help, pwd, ls, cd, cat, clear"
                    }, false);
                    break;
                case "pwd":
                    result = new TerminalCommandResult(new[] { _session.CurrentPath }, false);
                    break;
                case "ls":
                    result = ExecuteList();
                    break;
                case "cd":
                    result = ExecuteChangeDirectory(command.Args);
                    break;
                case "cat":
                    result = ExecuteCat(command.Args);
                    break;
                case "clear":
                    result = new TerminalCommandResult(Array.Empty<string>(), true);
                    break;
                default:
                    result = new TerminalCommandResult(new[] { $"Unknown command: {command.Name}" }, false);
                    break;
            }

            _eventBus?.Publish(new TerminalCommandExecutedEvent(command.Name, command.Args, cwd, resolvedPath));
            return result;
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
            return ResolvePath(_session.CurrentPath, path);
        }

        private VfsNode ResolvePath(string basePath, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return _session.CurrentDirectory;
            }

            var combined = path.StartsWith("/", StringComparison.Ordinal)
                ? path
                : basePath == "/"
                    ? $"/{path}"
                    : $"{basePath}/{path}";

            return _vfs.Resolve(combined);
        }

        private string ResolvePathForArgs(string[] args, string cwd)
        {
            if (args == null || args.Length == 0)
            {
                return null;
            }

            var node = ResolvePath(cwd, args[0]);
            return node?.Path;
        }
    }
}
