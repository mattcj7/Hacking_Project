using System;
using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Notifications
{
    public sealed class NotificationService
    {
        private readonly EventBus _eventBus;

        public NotificationService(EventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void Post(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            _eventBus.Publish(new NotificationPostedEvent(message));
        }
    }
}
