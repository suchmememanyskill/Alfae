using LauncherGamePlugin.Enums;

namespace LauncherGamePlugin.Interfaces;

public interface IGame
{
    public string Name { get; }
    public bool IsRunning { get; set; }
    public string InternalName => Name;
    public IGameSource Source { get; }
    public long? Size { get; }
    public Task<byte[]?> CoverImage();
    public Task<byte[]?> BackgroundImage();
    public InstalledStatus InstalledStatus { get; }
    public Platform EstimatedGamePlatform { get; } // Used to limit options for the boot profiles
    public ProgressStatus? ProgressStatus { get; }
    public event Action? OnUpdate;
    public void InvokeOnUpdate();
}