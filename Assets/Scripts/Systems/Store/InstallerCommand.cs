using System;
using HackingProject.Infrastructure.Terminal;
using HackingProject.Infrastructure.Vfs;

namespace HackingProject.Systems.Store
{
    public static class InstallerCommand
    {
        private const string InstallerExtension = ".installer";

        public static TerminalCommandResult Execute(VirtualFileSystem vfs, InstallService installService, string cwd, string pathArg, out string resolvedPath)
        {
            resolvedPath = null;
            if (string.IsNullOrWhiteSpace(pathArg))
            {
                return new TerminalCommandResult(new[] { "install: path required" }, false);
            }

            if (vfs == null)
            {
                return new TerminalCommandResult(new[] { "install: filesystem unavailable" }, false);
            }

            var node = ResolvePath(vfs, cwd, pathArg);
            resolvedPath = node?.Path;
            if (node == null)
            {
                return new TerminalCommandResult(new[] { $"install: no such file: {pathArg}" }, false);
            }

            if (node is not VfsFile file)
            {
                return new TerminalCommandResult(new[] { $"install: not a file: {pathArg}" }, false);
            }

            if (!file.Name.EndsWith(InstallerExtension, StringComparison.OrdinalIgnoreCase))
            {
                return new TerminalCommandResult(new[] { "install: not an installer file" }, false);
            }

            if (!InstallerPackage.TryParse(file.Content, out var package))
            {
                return new TerminalCommandResult(new[] { "install: invalid installer" }, false);
            }

            if (installService == null)
            {
                return new TerminalCommandResult(new[] { "install: install service unavailable" }, false);
            }

            var displayName = string.IsNullOrWhiteSpace(package.displayName) ? package.appId : package.displayName;
            var installed = installService.Install(package.appId);
            if (installed)
            {
                return new TerminalCommandResult(new[] { $"Installed {displayName}" }, false);
            }

            if (installService.IsInstalled(package.appId))
            {
                return new TerminalCommandResult(new[] { $"Already installed: {displayName}" }, false);
            }

            return new TerminalCommandResult(new[] { $"Install failed: {displayName}" }, false);
        }

        private static VfsNode ResolvePath(VirtualFileSystem vfs, string cwd, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var basePath = string.IsNullOrWhiteSpace(cwd) ? "/" : cwd;
            var combined = path.StartsWith("/", StringComparison.Ordinal)
                ? path
                : basePath == "/"
                    ? $"/{path}"
                    : $"{basePath}/{path}";

            return vfs.Resolve(combined);
        }
    }
}
