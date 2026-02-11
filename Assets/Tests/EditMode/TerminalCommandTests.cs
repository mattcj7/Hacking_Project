using HackingProject.Infrastructure.Events;
using HackingProject.Infrastructure.Save;
using HackingProject.Infrastructure.Terminal;
using HackingProject.Infrastructure.Vfs;
using HackingProject.Systems.Store;
using NUnit.Framework;

namespace HackingProject.Tests.EditMode
{
    public sealed class TerminalCommandTests
    {
        [Test]
        public void Parser_SplitsCommandAndArgs()
        {
            var parsed = TerminalCommandParser.TryParse("ls docs", out var command);

            Assert.IsTrue(parsed);
            Assert.AreEqual("ls", command.Name);
            Assert.AreEqual(1, command.Args.Length);
            Assert.AreEqual("docs", command.Args[0]);
        }

        [Test]
        public void Commands_CdAndLs_Work()
        {
            var vfs = DefaultVfsFactory.Create();
            var session = new TerminalSession(vfs, "/home/user");
            var processor = new TerminalCommandProcessor(vfs, session, null, new EventBus());

            processor.Execute("cd docs");
            Assert.AreEqual("/home/user/docs", session.CurrentPath);

            processor.Execute("cd ..");
            Assert.AreEqual("/home/user", session.CurrentPath);

            var list = processor.Execute("ls");
            CollectionAssert.Contains(list.OutputLines, "docs/");
            CollectionAssert.Contains(list.OutputLines, "downloads/");
        }

        [Test]
        public void InstallCommand_InstallsFromInstaller()
        {
            var vfs = DefaultVfsFactory.Create();
            var downloads = vfs.Resolve("/home/user/downloads") as VfsDirectory;
            var package = InstallerPackage.Create("Notes", "Notes", 1, 25);
            downloads.AddFile("Notes.installer", package.ToJson());

            var saveData = SaveGameData.CreateDefault();
            saveData.OwnedAppIds.Add("Notes");
            var eventBus = new EventBus();
            var installService = new InstallService(eventBus, saveData);

            var result = InstallerCommand.Execute(vfs, installService, "/home/user", "/home/user/downloads/Notes.installer", out var resolvedPath);

            Assert.AreEqual("/home/user/downloads/Notes.installer", resolvedPath);
            Assert.IsTrue(saveData.InstalledAppIds.Contains("Notes"));
            CollectionAssert.Contains(result.OutputLines, "Installed Notes");
        }
    }
}
