using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Launcher;

namespace LauncherGamePlugin.Interfaces;

public interface IGameSource
{
    string ServiceName { get; }
    string Version { get; }
    string SlugServiceName { get; }
    string ShortServiceName { get; }
    PluginType Type { get; }

    public Task<InitResult?> Initialize(IApp app);
    public async Task<List<IBootProfile>> GetBootProfiles() => new();
    public async Task<List<IGame>> GetGames() => new();
    public List<Command> GetGameCommands(IGame game) => new();
    public List<Command> GetGlobalCommands() => new();
}