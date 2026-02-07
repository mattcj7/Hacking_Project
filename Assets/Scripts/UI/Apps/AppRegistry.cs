using System;
using System.Collections.Generic;

namespace HackingProject.UI.Apps
{
    public sealed class AppRegistry
    {
        private readonly List<AppDefinition> _installedApps;

        public AppRegistry(IEnumerable<AppDefinition> installedApps)
        {
            if (installedApps == null)
            {
                throw new ArgumentNullException(nameof(installedApps));
            }

            _installedApps = new List<AppDefinition>(installedApps);
        }

        public IReadOnlyList<AppDefinition> InstalledApps => _installedApps;
    }
}
