using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;

namespace LocalGames.Data;

public class GeneratedGame : IGame
{
    private LocalGame _base;
    private LocalGameSource _local;
    private string _cli;
    public string FilePath { get; set; }

    public GeneratedGame(LocalGame @base, LocalGameSource local, string cli, string filePath)
    {
        _base = @base;
        _local = local;
        _cli = cli;
        FilePath = filePath;
        Name = Path.GetFileName(filePath).Split('.').First();
        Size = new FileInfo(filePath).Length;
        InstalledStatus = InstalledStatus.Installed;
    }

    public string Name { get; }
    public bool IsRunning { get; set; }
    public IGameSource Source => _local;
    public long? Size { get; }
    public InstalledStatus InstalledStatus { get; }
    public Platform EstimatedGamePlatform => _base.EstimatedGamePlatform;
    public ProgressStatus? ProgressStatus { get; } = null;
    public event Action? OnUpdate;

    public void InvokeOnUpdate()
        => OnUpdate?.Invoke();
    
    public LaunchParams ToExecLaunch()
    {
        LaunchParams baseLaunchParams = _base.ToExecLaunch();
        LaunchParams newLaunchParams = new(baseLaunchParams.Executable, $"{baseLaunchParams.Arguments} {_cli}".Trim(),
            baseLaunchParams.WorkingDirectory, this, baseLaunchParams.Platform);

        return newLaunchParams;
    }

    public bool HasImage(ImageType type) => false;

    public async Task<byte[]?> GetImage(ImageType type) => null;
}