using System;
using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Time
{
    public readonly struct TimeSecondTickedEvent : IEvent
    {
        public TimeSecondTickedEvent(DateTime currentTime)
        {
            CurrentTime = currentTime;
        }

        public DateTime CurrentTime { get; }
    }
}
