using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Time
{
    public interface ITimeServiceProvider
    {
        EventBus EventBus { get; }
        TimeService TimeService { get; }
    }
}
