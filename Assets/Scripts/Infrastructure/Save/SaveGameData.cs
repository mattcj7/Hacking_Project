using System;

namespace HackingProject.Infrastructure.Save
{
    [Serializable]
    public sealed class SaveGameData
    {
        public int Version = 1;
        public string LastSavedUtcIso;
        public int Credits;
        public OsSessionData OsSession;

        public static SaveGameData CreateDefault()
        {
            return new SaveGameData
            {
                Version = 1,
                Credits = 0,
                LastSavedUtcIso = string.Empty,
                OsSession = new OsSessionData()
            };
        }
    }
}
