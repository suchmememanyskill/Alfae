using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;
using Newtonsoft.Json;

namespace LocalGames.Data;

public class LocalGame : IGame
{
    public string Name { get; set; }
    public string ExecPath { get; set; }
    public long? Size { get; set; }
    public string? CoverImagePath { get; set; } = "";
    public string? BackgroundImagePath { get; set; } = "";
    public string? LaunchArgs { get; set; } = "";

    public async Task<byte[]?> CoverImage()
    {
        if (string.IsNullOrWhiteSpace(CoverImagePath) || !File.Exists(CoverImagePath))
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
    [JsonIgnore] public ProgressStatus? ProgressStatus { get; set; }
    [JsonIgnore] public string InstalledPath => Path.GetDirectoryName(ExecPath);
    [JsonIgnore] public IGameSource Source { get; set; }
    
    public event Action? OnUpdate;
    public void InvokeOnUpdate() => OnUpdate?.Invoke();

    public LaunchParams ToExecLaunch()
    {
        LaunchParams launchParams = new LaunchParams(ExecPath, LaunchArgs, InstalledPath, this);
        return launchParams;
    }
}