using System;
using System.Collections.Generic;

namespace HackingProject.UI.Apps
{
    public sealed class AppRegistry
    {
        private readonly List<AppDefinitionSO> _installedApps;

        public AppRegistry(IEnumerable<AppDefinitionSO> installedApps)
        {
            if (installedApps == null)
            {
                throw new ArgumentNullException(nameof(installedApps));
            }

            _installedApps = new List<AppDefinitionSO>();
            foreach (var app in installedApps)
            {
                if (app != null)
                {
                    _installedApps.Add(app);
                }
            }
        }

        public IReadOnlyList<AppDefinitionSO> InstalledApps => _installedApps;
    }
}
