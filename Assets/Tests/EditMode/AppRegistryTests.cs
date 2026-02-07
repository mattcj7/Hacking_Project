using HackingProject.UI.Apps;
using NUnit.Framework;
using UnityEngine;

namespace HackingProject.Tests.EditMode
{
    public sealed class AppRegistryTests
    {
        [Test]
        public void InstalledApps_ReturnsProvidedApps()
        {
            var terminal = ScriptableObject.CreateInstance<AppDefinitionSO>();
            terminal.Id = AppId.Terminal;
            terminal.DisplayName = "Terminal";

            var fileManager = ScriptableObject.CreateInstance<AppDefinitionSO>();
            fileManager.Id = AppId.FileManager;
            fileManager.DisplayName = "File Manager";

            var registry = new AppRegistry(new[] { terminal, fileManager });

            Assert.AreEqual(2, registry.InstalledApps.Count);
            Assert.AreEqual(AppId.Terminal, registry.InstalledApps[0].Id);
            Assert.AreEqual(AppId.FileManager, registry.InstalledApps[1].Id);
        }
    }
}
