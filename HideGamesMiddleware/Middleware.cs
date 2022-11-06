using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Interfaces;

namespace HideGamesMiddleware;

public class Middleware : IServiceMiddleware
{
    private HideGames _instance;

    public Middleware(HideGames instance)
    {
        _instance = instance;
    }

    public async Task<List<IGame>> GetGames(IGameSource next)
    {
        List<IGame> games = await next.GetGames();
        return games.Where(x => !_instance.Storage.Data.IsHidden(x)).ToList();
    }

    public List<Command> GetGameCommands(IGame game, IGameSource next)
    {
        List<Command> items = next.GetGameCommands(game);
        items.Add(new());
        items.Add(new("Hide Game", () => HideGame(game)));
        return items;
    }

    private void HideGame(IGame game)
    {
        _instance.Storage.Data.HideGame(game);
        _instance.Storage.Save();
        _instance.App.ReloadGames();
    }
}