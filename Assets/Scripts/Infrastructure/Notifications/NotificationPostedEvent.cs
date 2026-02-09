using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Notifications
{
    public readonly struct NotificationPostedEvent : IEvent
    {
        public NotificationPostedEvent(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
