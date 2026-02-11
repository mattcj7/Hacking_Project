using System.Collections.Generic;
using UnityEngine;

namespace HackingProject.UI.Apps
{
    [CreateAssetMenu(menuName = "Hacking Project/Apps/App Package Database", fileName = "AppPackageDatabase")]
    public sealed class AppPackageDatabaseSO : ScriptableObject
    {
        public List<AppDefinitionSO> Apps = new List<AppDefinitionSO>();

        public bool TryGetById(AppId id, out AppDefinitionSO app)
        {
            for (var i = 0; i < Apps.Count; i++)
            {
                var candidate = Apps[i];
                if (candidate != null && candidate.Id == id)
                {
                    app = candidate;
                    return true;
                }
            }

            app = null;
            return false;
        }
    }
}
