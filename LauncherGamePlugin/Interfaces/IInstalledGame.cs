namespace LauncherGamePlugin.Interfaces;

public interface IInstalledGame : IGame
{
    public string InstalledVersion { get; }
    public string InstalledPath { get; }
}