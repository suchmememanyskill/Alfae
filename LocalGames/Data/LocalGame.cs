using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;
using Newtonsoft.Json;

namespace LocalGames.Data;

public class LocalGame : IGame
{
    public string InternalName { get; set; }
    public string Name { get; set; }
    public string ExecPath { get; set; }
    public string WorkingDirectory { get; set; } = "";
    public long? Size { get; set; }
    public string? CoverImagePath { get; set; } = "";
    public string? BackgroundImagePath { get; set; } = "";
    public string? LaunchArgs { get; set; } = "";
    [JsonIgnore] public bool IsRunning { get; set; }

    public async Task<byte[]?> CoverImage()
    {
        if (!HasCoverImage)
            return null;

        return await File.ReadAllBytesAsync(CoverImagePath);
    }

    public async Task<byte[]?> BackgroundImage()
    {
        if (string.IsNullOrWhiteSpace(BackgroundImagePath) || !File.Exists(BackgroundImagePath))
            return null;

        return await File.ReadAllBytesAsync(BackgroundImagePath);
    }

    [JsonIgnore] public InstalledStatus InstalledStatus => InstalledStatus.Installed;
    [JsonIgnore] public Platform EstimatedGamePlatform => ExecPath.EndsWith(".exe") ? Platform.Windows : Platform.Linux;
    [JsonIgnore] public ProgressStatus? ProgressStatus { get; set; }
    [JsonIgnore] public string InstalledPath => string.IsNullOrWhiteSpace(WorkingDirectory) ? Path.GetDirectoryName(ExecPath) : WorkingDirectory;
    [JsonIgnore] public IGameSource Source { get; set; }
    [JsonIgnore] public bool HasCoverImage => !string.IsNullOrWhiteSpace(CoverImagePath) && File.Exists(CoverImagePath);
    [JsonIgnore] public bool HasBackgroundImage => !string.IsNullOrWhiteSpace(BackgroundImagePath) && File.Exists(BackgroundImagePath);

    public event Action? OnUpdate;
    public void InvokeOnUpdate() => OnUpdate?.Invoke();

    public LaunchParams ToExecLaunch()
    {
        LaunchParams launchParams =
            new LaunchParams(ExecPath, LaunchArgs, InstalledPath, this);
        return launchParams;
    }
    
    public bool HasImage(ImageType type)
    {
        if (type == ImageType.VerticalCover)
            return HasCoverImage;
        if (type == ImageType.Background)
            return HasBackgroundImage;

        return false;
    }

    public async Task<byte[]?> GetImage(ImageType type)
    {
        if (type == ImageType.VerticalCover)
            return await CoverImage();
        if (type == ImageType.Background)
            return await BackgroundImage();

        return null;
    }
}