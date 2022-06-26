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
    public async Task<List<IBootProfile>> GetBootProfiles() => new();
    public async Task<List<IGame>> GetGames() => new();

    public async Task CustomCommand(string command, IGame? game) { }
    public List<Command> GetGameCommands(IGame game) => new();
    public List<Command> GetGlobalCommands() => new();
}