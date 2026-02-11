using System.Collections.Generic;
using UnityEngine;

namespace HackingProject.Systems.Store
{
    [CreateAssetMenu(menuName = "Hacking Project/Store/Store Catalog", fileName = "StoreCatalog")]
    public sealed class StoreCatalogSO : ScriptableObject
    {
        public List<StoreItemDefinitionSO> Items = new List<StoreItemDefinitionSO>();
    }
}
