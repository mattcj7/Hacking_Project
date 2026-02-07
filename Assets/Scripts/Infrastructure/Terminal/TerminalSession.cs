using System;
using HackingProject.Infrastructure.Vfs;

namespace HackingProject.Infrastructure.Terminal
{
    public sealed class TerminalSession
    {
        private readonly VirtualFileSystem _vfs;

        public TerminalSession(VirtualFileSystem vfs, string startPath)
        {
            _vfs = vfs ?? throw new ArgumentNullException(nameof(vfs));
            CurrentDirectory = _vfs.Resolve(startPath) as VfsDirectory ?? _vfs.Root;
        }

        public VfsDirectory CurrentDirectory { get; private set; }
        public string CurrentPath => CurrentDirectory.Path;

        public void SetCurrentDirectory(VfsDirectory directory)
        {
            CurrentDirectory = directory ?? throw new ArgumentNullException(nameof(directory));
        }
    }
}
