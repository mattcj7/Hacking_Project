using HackingProject.Infrastructure.Terminal;
using HackingProject.Infrastructure.Vfs;
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
            var processor = new TerminalCommandProcessor(vfs, session, null);

            processor.Execute("cd docs");
            Assert.AreEqual("/home/user/docs", session.CurrentPath);

            processor.Execute("cd ..");
            Assert.AreEqual("/home/user", session.CurrentPath);

            var list = processor.Execute("ls");
            CollectionAssert.Contains(list.OutputLines, "docs/");
            CollectionAssert.Contains(list.OutputLines, "downloads/");
        }
    }
}
