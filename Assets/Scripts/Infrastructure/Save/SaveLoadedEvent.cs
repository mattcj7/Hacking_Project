using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Save
{
    public readonly struct SaveLoadedEvent : IEvent
    {
        public SaveLoadedEvent(SaveGameData data, SaveLoadSource source)
        {
            Data = data;
            Source = source;
        }

        public SaveGameData Data { get; }
        public SaveLoadSource Source { get; }
        public bool LoadedFromBackup => Source == SaveLoadSource.Backup;
    }
}
