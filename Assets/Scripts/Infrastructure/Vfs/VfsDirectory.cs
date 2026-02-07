using System;
using System.Collections.Generic;

namespace HackingProject.Infrastructure.Vfs
{
    public sealed class VfsDirectory : VfsNode
    {
        private readonly List<VfsNode> _children = new List<VfsNode>();

        public VfsDirectory(string name) : base(name)
        {
        }

        public IReadOnlyList<VfsNode> Children => _children;

        public VfsDirectory AddDirectory(string name)
        {
            var directory = new VfsDirectory(name) { Parent = this };
            _children.Add(directory);
            return directory;
        }

        public VfsFile AddFile(string name, string content)
        {
            var file = new VfsFile(name, content) { Parent = this };
            _children.Add(file);
            return file;
        }

        public VfsNode GetChild(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            for (var i = 0; i < _children.Count; i++)
            {
                if (string.Equals(_children[i].Name, name, StringComparison.Ordinal))
                {
                    return _children[i];
                }
            }

            return null;
        }
    }
}
