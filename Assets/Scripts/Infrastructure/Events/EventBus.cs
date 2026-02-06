using System;
using System.Collections.Generic;

namespace HackingProject.Infrastructure.Events
{
    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public IDisposable Subscribe<T>(Action<T> handler) where T : IEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var eventType = typeof(T);
            if (!_handlers.TryGetValue(eventType, out var subscribers))
            {
                subscribers = new List<Delegate>();
                _handlers.Add(eventType, subscribers);
            }

            subscribers.Add(handler);
            return new Subscription<T>(this, handler);
        }

        public void Publish<T>(T evt) where T : IEvent
        {
            if (ReferenceEquals(evt, null))
            {
                throw new ArgumentNullException(nameof(evt));
            }

            var eventType = typeof(T);
            if (!_handlers.TryGetValue(eventType, out var subscribers))
            {
                return;
            }

            var snapshot = subscribers.ToArray();
            foreach (var subscriber in snapshot)
            {
                if (subscriber is Action<T> typedSubscriber)
                {
                    typedSubscriber(evt);
                }
            }
        }

        private void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            if (handler == null)
            {
                return;
            }

            var eventType = typeof(T);
            if (!_handlers.TryGetValue(eventType, out var subscribers))
            {
                return;
            }

            subscribers.Remove(handler);
            if (subscribers.Count == 0)
            {
                _handlers.Remove(eventType);
            }
        }

        private sealed class Subscription<T> : IDisposable where T : IEvent
        {
            private readonly EventBus _bus;
            private Action<T> _handler;

            public Subscription(EventBus bus, Action<T> handler)
            {
                _bus = bus ?? throw new ArgumentNullException(nameof(bus));
                _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            }

            public void Dispose()
            {
                var handler = _handler;
                if (handler == null)
                {
                    return;
                }

                _handler = null;
                _bus.Unsubscribe(handler);
            }
        }
    }
}
