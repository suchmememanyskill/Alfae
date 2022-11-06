using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Interfaces;

namespace HideGamesMiddleware;

public class HideGames : IGameSource
{
    public string ServiceName => "Hide Games";
    public string Version => "v1.0.0";
    public string SlugServiceName => "hide-games";
    public string ShortServiceName => SlugServiceName;
    public Storage<Store> Storage { get; set; }
    public IApp App { get; set; }
    public async Task<InitResult?> Initialize(IApp app)
    {
        Storage = new(app, "hidegames.json");
        App = app;

        return new()
        {
            Middlewares = new()
            {
                new Middleware(this)
            }
        };
    }

    public List<Command> GetGlobalCommands()
    {
        List<Command> commands = new();
        
        foreach (var (key, value) in Storage.Data.Games)
        {
            if (value.Count > 0)
            {
                commands.Add(new(key, value.Select(x => new Command($"Show {x}", () => ShowGame(key, x))).ToList()));
            }
        }

        return commands;
    }

    private void ShowGame(string key, string name)
    {
        Storage.Data.ShowGame(key, name);
        Storage.Save();
        App.ReloadGames();
    }
}