using System;

namespace HackingProject.Infrastructure.Vfs
{
    public sealed class VfsFile : VfsNode
    {
        public VfsFile(string name, string content) : base(name)
        {
            Content = content ?? string.Empty;
        }

        public string Content { get; }
    }
}
