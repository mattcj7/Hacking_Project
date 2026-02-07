namespace HackingProject.Infrastructure.Vfs
{
    public interface IVfsProvider
    {
        VirtualFileSystem Vfs { get; }
    }
}
