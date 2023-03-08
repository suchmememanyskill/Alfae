using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;

namespace LauncherGamePlugin.Launcher;

public interface IBootProfile
{
    public string Name { get; }
    public Platform CompatiblePlatform { get; }
    public Platform CompatibleExecutable { get; }
    public List<Command> CustomCommands() => new();
    public void Launch(LaunchParams launchParams) => Launch(launchParams, null);
    public void Launch(LaunchParams launchParams, IApp? app);
    public List<FormEntry> GameOptions(IGame game) => new();
    public event Action<LaunchParams> OnGameLaunch;
    public event Action<LaunchParams> OnGameClose;
}