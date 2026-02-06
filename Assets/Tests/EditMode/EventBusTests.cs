using HackingProject.Infrastructure.Events;
using NUnit.Framework;

namespace HackingProject.Tests.EditMode
{
    public sealed class EventBusTests
    {
        private readonly struct TestEvent : IEvent
        {
            public TestEvent(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        [Test]
        public void Publish_NotifiesSubscriber()
        {
            var bus = new EventBus();
            var received = false;

            bus.Subscribe<TestEvent>(_ => received = true);

            bus.Publish(new TestEvent(1));

            Assert.IsTrue(received);
        }

        [Test]
        public void Unsubscribe_PreventsNotification()
        {
            var bus = new EventBus();
            var received = false;

            var subscription = bus.Subscribe<TestEvent>(_ => received = true);
            subscription.Dispose();
            subscription.Dispose();

            bus.Publish(new TestEvent(2));

            Assert.IsFalse(received);
        }
    }
}
