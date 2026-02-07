using System;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Time;
using NUnit.Framework;

namespace HackingProject.Tests.EditMode
{
    public sealed class TimeServiceTests
    {
        [Test]
        public void Tick_PublishesSecondTick()
        {
            var bus = new EventBus();
            var service = new TimeService(bus);
            var ticks = 0;

            using (bus.Subscribe<TimeSecondTickedEvent>(_ => ticks++))
            {
                service.Tick(0.5f);
                Assert.AreEqual(0, ticks);

                service.Tick(0.5f);
                Assert.AreEqual(1, ticks);
            }
        }

        [Test]
        public void FormatTime_ReturnsExpectedHhMmSs()
        {
            var time = new DateTime(2026, 2, 7, 9, 5, 30);

            var formatted = TimeService.FormatTime(time);

            Assert.AreEqual("09:05:30", formatted);
        }
    }
}
