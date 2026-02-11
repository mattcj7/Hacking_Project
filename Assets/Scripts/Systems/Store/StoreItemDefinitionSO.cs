using UnityEngine;

namespace HackingProject.Systems.Store
{
    [CreateAssetMenu(menuName = "Hacking Project/Store/Store Item", fileName = "StoreItem")]
    public sealed class StoreItemDefinitionSO : ScriptableObject
    {
        public string ItemId;
        public string DisplayName;
        public string Description;
        public int PriceCredits;
        public string AppIdToInstall;
    }
}
