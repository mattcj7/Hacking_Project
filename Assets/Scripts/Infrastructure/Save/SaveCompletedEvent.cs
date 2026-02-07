using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Save
{
    public readonly struct SaveCompletedEvent : IEvent
    {
        public SaveCompletedEvent(SaveGameData data)
        {
            Data = data;
        }

        public SaveGameData Data { get; }
    }
}
