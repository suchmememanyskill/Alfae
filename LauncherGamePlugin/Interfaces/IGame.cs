namespace LauncherGamePlugin.Interfaces;

public interface IGame
{
    public string Name { get; }
    public string InternalName => Name;
    public IGameSource Source { get; }
    public long? Size { get; }
    public Task<byte[]?> CoverImage();
    public Task<byte[]?> BackgroundImage();
    public InstalledStatus InstalledStatus { get; }
    public ProgressStatus? ProgressStatus { get; }
    public event Action? OnUpdate;
}