using System;

namespace HackingProject.Infrastructure.Missions
{
    [Serializable]
    public sealed class MissionObjectiveDefinition
    {
        public MissionObjectiveType Type;
        public string Description;
        public string Command;
        public string Path;
    }
}
