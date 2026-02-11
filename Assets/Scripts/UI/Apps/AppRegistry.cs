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

        public bool AddInstalledApp(AppDefinitionSO app)
        {
            if (app == null)
            {
                return false;
            }

            if (TryGetById(app.Id, out _))
            {
                return false;
            }

            _installedApps.Add(app);
            return true;
        }

        public bool TryGetById(AppId id, out AppDefinitionSO app)
        {
            for (var i = 0; i < _installedApps.Count; i++)
            {
                var candidate = _installedApps[i];
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
