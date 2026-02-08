using HackingProject.Infrastructure.Events;

namespace HackingProject.Infrastructure.Missions
{
    public readonly struct MissionObjectiveCompletedEvent : IEvent
    {
        public MissionObjectiveCompletedEvent(MissionDefinitionSO mission, int objectiveIndex)
        {
            Mission = mission;
            ObjectiveIndex = objectiveIndex;
        }

        public MissionDefinitionSO Mission { get; }
        public int ObjectiveIndex { get; }
    }
}
