using System;
using System.Collections.Generic;
using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Save;

namespace HackingProject.Systems.Store
{
    public sealed class InstallService
    {
        private readonly EventBus _eventBus;
        private readonly SaveGameData _saveData;

        public InstallService(EventBus eventBus, SaveGameData saveData)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _saveData = saveData ?? throw new ArgumentNullException(nameof(saveData));
            EnsureInstalledList();
        }

        public bool IsInstalled(string appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                return false;
            }

            var installed = _saveData.InstalledAppIds;
            if (installed == null)
            {
                return false;
            }

            return installed.Contains(appId);
        }

        public bool Install(string appId)
        {
            EnsureInstalledList();
            if (string.IsNullOrWhiteSpace(appId))
            {
                return false;
            }

            if (IsInstalled(appId))
            {
                return false;
            }

            if (!IsOwned(appId))
            {
                return false;
            }

            _saveData.InstalledAppIds.Add(appId);
            _eventBus.Publish(new AppInstalledEvent(appId));
            return true;
        }

        private bool IsOwned(string appId)
        {
            var owned = _saveData.OwnedAppIds;
            if (owned == null)
            {
                return false;
            }

            return owned.Contains(appId);
        }

        private void EnsureInstalledList()
        {
            if (_saveData.InstalledAppIds == null)
            {
                _saveData.InstalledAppIds = new List<string>();
            }
        }
    }
}
