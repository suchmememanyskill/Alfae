using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Launcher;

namespace LauncherGamePlugin.Interfaces;

public interface IGameSource
{
    string ServiceName { get; }
    string Description { get; }
    string Version { get; }
    string SlugServiceName { get; }
    string ShortServiceName { get; }

    public Task Initialize(IApp app);
    public Task<List<IBootProfile>> GetBootProfiles();
    public Task<List<IGame>> GetGames();
    public Task CustomCommand(string command, IGame? game);
    public List<Command> GetGameCommands(IGame game);
    public List<Command> GetGlobalCommands();
}