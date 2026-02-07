using System;
using System.Collections.Generic;

namespace HackingProject.Infrastructure.Vfs
{
    public sealed class VirtualFileSystem
    {
        private static readonly IReadOnlyList<VfsNode> Empty = Array.Empty<VfsNode>();

        public VirtualFileSystem(VfsDirectory root)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }

        public VfsDirectory Root { get; }

        public VfsNode Resolve(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == "/")
            {
                return Root;
            }

            if (!path.StartsWith("/", StringComparison.Ordinal))
            {
                return null;
            }

            var current = Root as VfsNode;
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];
                if (segment == ".")
                {
                    continue;
                }

                if (segment == "..")
                {
                    if (current is VfsDirectory dir && dir.Parent != null)
                    {
                        current = dir.Parent;
                    }

                    continue;
                }

                if (current is VfsDirectory currentDirectory)
                {
                    var child = currentDirectory.GetChild(segment);
                    if (child == null)
                    {
                        return null;
                    }

                    current = child;
                    continue;
                }

                return null;
            }

            return current;
        }

        public bool TryResolve(string path, out VfsNode node)
        {
            node = Resolve(path);
            return node != null;
        }

        public IReadOnlyList<VfsNode> ListChildren(string path)
        {
            var node = Resolve(path) as VfsDirectory;
            return node?.Children ?? Empty;
        }
    }
}
