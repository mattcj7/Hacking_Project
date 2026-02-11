using HackingProject.Infrastructure.Save;
using NUnit.Framework;

namespace HackingProject.Tests.EditMode
{
    public sealed class SaveMigrationServiceTests
    {
        [Test]
        public void Migration_FillsDefaults_ForV1()
        {
            var service = new SaveMigrationService();
            var data = new SaveGameData
            {
                Version = 1,
                Credits = 0,
                LastSavedUtcIso = null,
                OsSession = null,
                OwnedAppIds = null,
                InstalledAppIds = null
            };

            var version = service.Migrate(data, 1);

            Assert.AreEqual(SaveGameData.CurrentVersion, version);
            Assert.AreEqual(SaveGameData.CurrentVersion, data.Version);
            Assert.IsNotNull(data.OsSession);
            Assert.IsNotNull(data.OwnedAppIds);
            Assert.IsNotNull(data.InstalledAppIds);
            Assert.IsNotNull(data.LastSavedUtcIso);
        }
    }
}
