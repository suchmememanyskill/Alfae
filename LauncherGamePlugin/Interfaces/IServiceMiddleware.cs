using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Launcher;

namespace LauncherGamePlugin.Interfaces;

public interface IServiceMiddleware
{
    Task<List<IBootProfile>> GetBootProfiles(IGameSource next)
        => next.GetBootProfiles();
    Task<List<IGame>> GetGames(IGameSource next)
        => next.GetGames();
    List<Command> GetGameCommands(IGame game, IGameSource next)
        => next.GetGameCommands(game);
    List<Command> GetGlobalCommands(IGameSource next)
        => next.GetGlobalCommands();
}