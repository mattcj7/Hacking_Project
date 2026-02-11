using System;
using UnityEngine;

namespace HackingProject.Systems.Store
{
    [Serializable]
    public sealed class InstallerPackage
    {
        public string appId;
        public string displayName;
        public int version;
        public int pricePaid;
        public string createdAt;

        public static InstallerPackage Create(string appId, string displayName, int version, int pricePaid)
        {
            return new InstallerPackage
            {
                appId = appId,
                displayName = displayName,
                version = version,
                pricePaid = pricePaid,
                createdAt = DateTime.UtcNow.ToString("o")
            };
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static bool TryParse(string json, out InstallerPackage package)
        {
            package = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                package = JsonUtility.FromJson<InstallerPackage>(json);
            }
            catch
            {
                package = null;
                return false;
            }

            return package != null && !string.IsNullOrWhiteSpace(package.appId);
        }
    }
}
