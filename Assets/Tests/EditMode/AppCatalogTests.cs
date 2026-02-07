using HackingProject.UI.Apps;
using NUnit.Framework;
using UnityEditor;

namespace HackingProject.Tests.EditMode
{
    public sealed class AppCatalogTests
    {
        private const string CatalogPath = "Assets/ScriptableObjects/Apps/AppCatalog_Default.asset";

        [Test]
        public void DefaultCatalog_ContainsTerminalAndFileManager()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<AppCatalogSO>(CatalogPath);
            Assert.IsNotNull(catalog, $"Missing AppCatalog asset at {CatalogPath}.");

            Assert.AreEqual(2, catalog.Apps.Count);
            var hasTerminal = false;
            var hasFileManager = false;
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
            }

            Assert.IsTrue(hasTerminal);
            Assert.IsTrue(hasFileManager);
        }
    }
}
