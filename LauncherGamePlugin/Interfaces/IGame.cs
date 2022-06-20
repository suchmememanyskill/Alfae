namespace LauncherGamePlugin.Interfaces;

public interface IGame
{
    public string Name { get; }
    public string? Developer { get; }
    public IGameSource Source { get; }
    public Uri? Url { get; }
    public long? Size { get; }
    public string? AvailableVersion { get; }
    public List<Platform> AvailablePlatforms { get; }
    
    public Task<byte[]> CoverImage();
    public Task<byte[]> BackgroundImage();
}