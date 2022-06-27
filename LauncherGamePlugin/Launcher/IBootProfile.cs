using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Enums;

namespace LauncherGamePlugin.Launcher;

public interface IBootProfile
{
    public string Name { get; }
    public Platform CompatiblePlatform { get; }
    public Platform CompatibleExecutable { get; }
    public List<Command> CustomCommands() => new();
    public void Launch(LaunchParams launchParams);
}