using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Missions
{
    public readonly struct MissionStartedEvent : IEvent
    {
        public MissionStartedEvent(MissionDefinitionSO mission)
        {
            Mission = mission;
        }

        public MissionDefinitionSO Mission { get; }
    }
}
