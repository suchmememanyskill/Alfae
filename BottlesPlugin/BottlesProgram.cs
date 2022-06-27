using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;

namespace BottlesPlugin;

public class BottlesProgram : IGame
{
    public string Name { get; }
    public IGameSource Source { get; }
    public long? Size { get; } = 0;
    public async Task<byte[]?> CoverImage() => null;

    public async Task<byte[]?> BackgroundImage() => null;
    public InstalledStatus InstalledStatus { get; } = InstalledStatus.Installed;
    public ProgressStatus? ProgressStatus { get; } = null;
    public event Action? OnUpdate;
    private string _bottleName;
    
    public BottlesProgram(string name, string bottleName, Bottles bottles)
    {
        Name = name;
        Source = bottles;
        _bottleName = bottleName;
    }

    public void Launch(IApp app)
    {
        LaunchParams launchParams = new("flatpak",
            $"run --command=bottles-cli com.usebottles.bottles run -b \"{_bottleName}\" -p \"{Name}\"",
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), this);

        launchParams.ForceBootProfile = new NativeLinuxProfile();
        
        app.Launch(launchParams);
    }
}