using System.Collections.Generic;
using UnityEngine;

namespace HackingProject.UI.Apps
{
    [CreateAssetMenu(menuName = "Hacking Project/Apps/App Catalog", fileName = "AppCatalog")]
    public sealed class AppCatalogSO : ScriptableObject
    {
        public List<AppDefinitionSO> Apps = new List<AppDefinitionSO>();
    }
}
