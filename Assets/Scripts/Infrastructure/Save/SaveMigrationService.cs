using System;
using System.Collections.Generic;

namespace HackingProject.Infrastructure.Save
{
    public sealed class SaveMigrationService
    {
        public int Migrate(SaveGameData data, int fromVersion)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var sourceVersion = fromVersion <= 0 ? 1 : fromVersion;
            if (sourceVersion < SaveGameData.CurrentVersion)
            {
                EnsureDefaults(data);
                data.Version = SaveGameData.CurrentVersion;
                return data.Version;
            }

            EnsureDefaults(data);
            if (data.Version <= 0)
            {
                data.Version = SaveGameData.CurrentVersion;
            }

            return data.Version;
        }

        private static void EnsureDefaults(SaveGameData data)
        {
            if (data.OsSession == null)
            {
                data.OsSession = new OsSessionData();
            }

            if (data.OwnedAppIds == null)
            {
                data.OwnedAppIds = new List<string>();
            }

            if (data.InstalledAppIds == null)
            {
                data.InstalledAppIds = new List<string>();
            }

            if (data.Credits < 0)
            {
                data.Credits = 0;
            }

            if (data.LastSavedUtcIso == null)
            {
                data.LastSavedUtcIso = string.Empty;
            }
        }
    }
}
