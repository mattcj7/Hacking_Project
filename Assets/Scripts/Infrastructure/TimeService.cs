using System;
using System.Globalization;
using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Time
{
    public sealed class TimeService
    {
        private readonly EventBus _eventBus;
        private float _accumulator;

        public TimeService(EventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            CurrentTime = DateTime.Now;
        }

        public DateTime CurrentTime { get; private set; }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            _accumulator += deltaTime;
            while (_accumulator >= 1f)
            {
                _accumulator -= 1f;
                CurrentTime = DateTime.Now;
                _eventBus.Publish(new TimeSecondTickedEvent(CurrentTime));
            }
        }

        public static string FormatTime(DateTime time)
        {
            return time.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}
