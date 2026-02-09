using System.Collections.Generic;
using UnityEngine;

namespace HackingProject.Infrastructure.Missions
{
    [CreateAssetMenu(menuName = "Hacking Project/Missions/Mission Definition", fileName = "MissionDefinition")]
    public sealed class MissionDefinitionSO : ScriptableObject
    {
        public string Id;
        public string Title;
        public string Description;
        public int RewardCredits;
        public List<MissionObjectiveDefinition> Objectives = new List<MissionObjectiveDefinition>();
    }
}
