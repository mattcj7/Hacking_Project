using System;
using System.Collections.Generic;

namespace HackingProject.Infrastructure.Save
{
    [Serializable]
    public sealed class SaveGameData
    {
        public int Version = 1;
        public string LastSavedUtcIso;
        public int Credits;
        public OsSessionData OsSession;
        public List<string> OwnedAppIds;
        public List<string> InstalledAppIds;

        public static SaveGameData CreateDefault()
        {
            return new SaveGameData
            {
                Version = 1,
                Credits = 0,
                LastSavedUtcIso = string.Empty,
                OsSession = new OsSessionData(),
                OwnedAppIds = new List<string>(),
                InstalledAppIds = new List<string>()
            };
        }
    }
}
