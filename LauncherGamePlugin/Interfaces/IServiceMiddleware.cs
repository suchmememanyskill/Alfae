using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Launcher;

namespace LauncherGamePlugin.Interfaces;

public interface IServiceMiddleware
{
    Task<List<IBootProfile>> GetBootProfiles(IGameSource next);
    Task<List<IGame>> GetGames(IGameSource next);
    List<Command> GetGameCommands(IGame game, IGameSource next);
    List<Command> GetGlobalCommands(IGameSource next);
}