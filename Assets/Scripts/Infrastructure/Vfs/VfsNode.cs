using System;

namespace HackingProject.Infrastructure.Vfs
{
    public abstract class VfsNode
    {
        protected VfsNode(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name is required.", nameof(name));
            }

            Name = name;
        }

        public string Name { get; }
        public VfsDirectory Parent { get; internal set; }

        public string Path
        {
            get
            {
                if (Parent == null)
                {
                    return "/";
                }

                var parentPath = Parent.Path;
                return parentPath == "/" ? $"/{Name}" : $"{parentPath}/{Name}";
            }
        }
    }
}
