using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;
using SteamGridDbMiddleware.Gui;
using SteamGridDbMiddleware.Model;

namespace SteamGridDbMiddleware;

public class Middleware : IServiceMiddleware
{
    private SteamGridDb _instance;

    public Middleware(SteamGridDb instance)
    {
        _instance = instance;
    }

    public async Task<List<IGame>> GetGames(IGameSource next)
    {
        List<IGame> games = await next.GetGames();
        return games.Select(x => (IGame)new GameOverride(x, _instance)).ToList();
    }

    public List<Command> GetGameCommands(IGame game, IGameSource next)
    {
        List<Command> commands = new(next.GetGameCommands(game));

        if (_instance.Api == null)
        {
            commands.Add(new("Edit Cover"));
            commands.Add(new("Edit Background"));
        }
        else
        {
            commands.Add(new("Edit Cover", () => new OnCoverEdit(game, _instance).OnGui()));
            commands.Add(new("Edit Background", () => new OnBackgroundEdit(game, _instance).OnGui()));
        }
        
        return commands;
    }
}