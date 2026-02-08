using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Vfs
{
    public readonly struct FileManagerOpenedFileEvent : IEvent
    {
        public FileManagerOpenedFileEvent(string fullPath)
        {
            FullPath = fullPath;
        }

        public string FullPath { get; }
    }
}
