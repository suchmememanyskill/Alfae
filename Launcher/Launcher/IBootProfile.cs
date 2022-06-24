using System.Collections.Generic;
using LauncherGamePlugin;
using LauncherGamePlugin.Commands;

namespace Launcher.Launcher;

public interface IBootProfile
{
    public string Name { get; }
    public Platform CompatiblePlatform { get; }
    public Platform CompatibleExecutable { get; }
    public List<Command> CustomCommands() => new();
    public void Launch(ExecLaunch launch);
}