using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Missions
{
    public readonly struct MissionCompletedEvent : IEvent
    {
        public MissionCompletedEvent(MissionDefinitionSO mission)
        {
            Mission = mission;
        }

        public MissionDefinitionSO Mission { get; }
    }
}
