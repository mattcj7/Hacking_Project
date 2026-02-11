using HackingProject.UI.Apps;
using NUnit.Framework;
using UnityEditor;

namespace HackingProject.Tests.EditMode
{
    public sealed class AppCatalogTests
    {
        private const string CatalogPath = "Assets/ScriptableObjects/Apps/AppCatalog_Default.asset";

        [Test]
        public void DefaultCatalog_ContainsTerminalFileManagerMissionsAndStore()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<AppCatalogSO>(CatalogPath);
            Assert.IsNotNull(catalog, $"Missing AppCatalog asset at {CatalogPath}.");

            var hasTerminal = false;
            var hasFileManager = false;
            var hasMissions = false;
            var hasStore = false;
            for (var i = 0; i < catalog.Apps.Count; i++)
            {
                var app = catalog.Apps[i];
                if (app == null)
                {
                    continue;
                }

                if (app.Id == AppId.Terminal)
                {
                    hasTerminal = true;
                }
                else if (app.Id == AppId.FileManager)
                {
                    hasFileManager = true;
                }
                else if (app.Id == AppId.Missions)
                {
                    hasMissions = true;
                }
                else if (app.Id == AppId.Store)
                {
                    hasStore = true;
                }
            }

            Assert.IsTrue(hasTerminal);
            Assert.IsTrue(hasFileManager);
            Assert.IsTrue(hasMissions);
            Assert.IsTrue(hasStore);
        }
    }
}
