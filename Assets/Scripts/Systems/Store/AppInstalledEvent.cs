using HackingProject.Infrastructure.Events;

namespace HackingProject.Systems.Store
{
    public readonly struct AppInstalledEvent : IEvent
    {
        public AppInstalledEvent(string appId)
        {
            AppId = appId;
        }

        public string AppId { get; }
    }
}
