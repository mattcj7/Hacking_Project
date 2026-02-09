using System;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Notifications;
using UnityEngine.UIElements;

namespace HackingProject.UI.Desktop
{
    public sealed class ToastController : IDisposable
    {
        private const string ToastClassName = "toast";
        private const string ToastTextClassName = "toast-text";
        private const int ToastDurationMs = 3500;

        private readonly VisualElement _container;
        private readonly EventBus _eventBus;
        private IDisposable _subscription;

        public ToastController(VisualElement container, EventBus eventBus)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _subscription = _eventBus.Subscribe<NotificationPostedEvent>(OnNotificationPosted);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
            _container.Clear();
        }

        private void OnNotificationPosted(NotificationPostedEvent evt)
        {
            if (string.IsNullOrWhiteSpace(evt.Message))
            {
                return;
            }

            var toast = new VisualElement();
            toast.AddToClassList(ToastClassName);
            var label = new Label(evt.Message);
            label.AddToClassList(ToastTextClassName);
            toast.Add(label);
            _container.Add(toast);

            _container.schedule.Execute(() => RemoveToast(toast)).ExecuteLater(ToastDurationMs);
        }

        private void RemoveToast(VisualElement toast)
        {
            if (toast == null)
            {
                return;
            }

            if (toast.parent == _container)
            {
                toast.RemoveFromHierarchy();
            }
        }
    }
}
