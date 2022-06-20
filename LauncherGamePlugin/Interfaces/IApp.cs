using LauncherGamePlugin;

namespace LauncherGamePlugin.Interfaces;

public interface IApp
{
    public string ConfigDir { get; }
    public Logger Logger { get; }
}