using System.Collections.Generic;
using HackingProject.UI.Apps;
using NUnit.Framework;

namespace HackingProject.Tests.EditMode
{
    public sealed class AppRegistryTests
    {
        [Test]
        public void InstalledApps_ReturnsProvidedApps()
        {
            var apps = new List<AppDefinition>
            {
                new AppDefinition(AppId.Terminal, "Terminal"),
                new AppDefinition(AppId.FileManager, "File Manager")
            };

            var registry = new AppRegistry(apps);

            Assert.AreEqual(2, registry.InstalledApps.Count);
            Assert.AreEqual(AppId.Terminal, registry.InstalledApps[0].Id);
            Assert.AreEqual(AppId.FileManager, registry.InstalledApps[1].Id);
        }
    }
}
