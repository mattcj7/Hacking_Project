namespace HackingProject.UI.Apps
{
    public readonly struct AppDefinition
    {
        public AppDefinition(AppId id, string displayName)
        {
            Id = id;
            DisplayName = displayName ?? string.Empty;
        }

        public AppId Id { get; }
        public string DisplayName { get; }
    }
}
