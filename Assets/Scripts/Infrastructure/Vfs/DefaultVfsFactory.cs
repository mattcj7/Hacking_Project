namespace HackingProject.Infrastructure.Vfs
{
    public static class DefaultVfsFactory
    {
        public static VirtualFileSystem Create()
        {
            var root = new VfsDirectory("root");
            var home = root.AddDirectory("home");
            var user = home.AddDirectory("user");
            var docs = user.AddDirectory("docs");
            var downloads = user.AddDirectory("downloads");

            docs.AddFile("readme.txt", "Welcome to HackingOS.");
            docs.AddFile("todo.txt", "- Learn the terminal\n- Organize files");
            downloads.AddFile("installer.log", "Download complete.");
            user.AddFile("notes.txt", "Remember to check new messages.");

            return new VirtualFileSystem(root);
        }
    }
}
