using HackingProject.Infrastructure.Vfs;
using NUnit.Framework;

namespace HackingProject.Tests.EditMode
{
    public sealed class VirtualFileSystemTests
    {
        [Test]
        public void Resolve_FindsDirectoriesAndFiles()
        {
            var vfs = DefaultVfsFactory.Create();

            var docs = vfs.Resolve("/home/user/docs") as VfsDirectory;
            var readme = vfs.Resolve("/home/user/docs/readme.txt") as VfsFile;

            Assert.IsNotNull(docs);
            Assert.IsNotNull(readme);
            Assert.AreEqual("readme.txt", readme.Name);
        }

        [Test]
        public void ListChildren_ReturnsExpectedEntries()
        {
            var vfs = DefaultVfsFactory.Create();

            var children = vfs.ListChildren("/home/user");

            var hasDocs = false;
            var hasDownloads = false;
            for (var i = 0; i < children.Count; i++)
            {
                if (children[i].Name == "docs")
                {
                    hasDocs = true;
                }
                else if (children[i].Name == "downloads")
                {
                    hasDownloads = true;
                }
            }

            Assert.IsTrue(hasDocs);
            Assert.IsTrue(hasDownloads);
        }
    }
}
