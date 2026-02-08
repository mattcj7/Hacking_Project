using System.Collections.Generic;
using UnityEngine;

namespace HackingProject.Infrastructure.Missions
{
    [CreateAssetMenu(menuName = "Hacking Project/Missions/Mission Catalog", fileName = "MissionCatalog")]
    public sealed class MissionCatalogSO : ScriptableObject
    {
        public List<MissionDefinitionSO> Missions = new List<MissionDefinitionSO>();
    }
}
