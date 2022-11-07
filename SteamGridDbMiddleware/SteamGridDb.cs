using craftersmine.SteamGridDBNet;
using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using SteamGridDbMiddleware.Gui;
using SteamGridDbMiddleware.Model;

namespace SteamGridDbMiddleware;

public class SteamGridDb : IGameSource
{
    public string ServiceName => "SteamGridDb";
    public string Version => "v1.0.0";
    public string SlugServiceName => "steam-grid-db";
    public string ShortServiceName => "steamgriddb";
    public IApp App { get; set; }
    public craftersmine.SteamGridDBNet.SteamGridDb? Api { get; set; }
    public Storage<Store> Storage { get; set; }
    public async Task<InitResult?> Initialize(IApp app)
    {
        Storage = new(app, "steamgriddb.json");

        await CheckLoggedInStatus(Storage.Data.ApiKey);
        
        App = app;

        return new InitResult()
        {
            Middlewares = new()
            {
                new Middleware(this)
            }
        };
    }

    public async Task<bool> CheckLoggedInStatus(string? key)
    {
        Api = null;
        if (string.IsNullOrEmpty(key))
            return false;
        
        craftersmine.SteamGridDBNet.SteamGridDb api = new(key);
        try
        {
            var game = await api.GetGameByIdAsync(1226);
            Api = api;
            return true;
        }
        catch (Exception e)
        {
            App.Logger.Log($"Failed to check SteamGridDb login status: {e.Message}. Treating as bad login");
            return false;
        }
    }

    public List<Command> GetGlobalCommands()
    {
        if (Api == null)
        {
            return new()
            {
                new("Not logged in"),
                new(),
                new("Log in", () => new Authenticate(this).ShowGui())
            };
        }
        else
        {
            return new()
            {
                new("Logged in"),
                new(),
                new("Log out", Logout),
                new("Set images on installed games with missing images", SetFirstImageOnInstalledMissingImages),
            };
        }
    }

    public async void SetFirstImageOnInstalledMissingImages()
    {
        App.ShowTextPrompt("Looking up images for installed games with missing content...");
        int coverCount = 0;
        int backgroundCount = 0;
        List<IGame> games = App.GetAllGames().Where(x => x.InstalledStatus == InstalledStatus.Installed).ToList();
        foreach (var game in games)
        {
            if (!game.HasCoverImage || !game.HasBackgroundImage)
            {
                var gridGames = await Api.SearchForGamesAsync(game.Name);
                if (gridGames.Length <= 0)
                    continue;
                
                var grid = gridGames.First();
                
                if (!game.HasCoverImage)
                {
                    List<SteamGridDbGrid> covers = new();
                    
                    covers = (await Api.GetGridsForGameAsync(grid, dimensions: SteamGridDbDimensions.W600H900, types: SteamGridDbTypes.Static))?.ToList() ?? new();
                    
                    if (covers.Count > 0)
                    {
                        var cover = covers.First();
                        Override? x = Storage.Data.GetCover(game);
                        x ??= new(game.Name, game.Source.ServiceName, "", "");

                        x.Url = cover.FullImageUrl;
                        x.Id = cover.Id.ToString();

                        if (!Storage.Data.Covers.Contains(x))
                            Storage.Data.Covers.Add(x);

                        coverCount++;
                    }
                }

                if (!game.HasBackgroundImage)
                {
                    List<SteamGridDbHero> heroes = new();
                    heroes = (await Api.GetHeroesForGameAsync(grid,
                        dimensions: SteamGridDbDimensions.W1920H620 | SteamGridDbDimensions.W3840H1240, types: SteamGridDbTypes.Static))?.ToList() ?? new();

                    if (heroes.Count > 0)
                    {
                        var hero = heroes.First();
                        Override? x = Storage.Data.GetBackground(game);
                        x ??= new(game.Name, game.Source.ServiceName, "", "");

                        x.Url = hero.FullImageUrl;
                        x.Id = hero.Id.ToString();

                        if (!Storage.Data.Backgrounds.Contains(x))
                            Storage.Data.Backgrounds.Add(x);

                        backgroundCount++;
                    }
                }
            }
        }
        
        Storage.Save();
        App.ReloadGames();
        App.ShowDismissibleTextPrompt($"Set {coverCount} Covers and {backgroundCount} Backgrounds");
    }

    private async void Logout()
    {
        Storage.Data.ApiKey = "";
        Storage.Save();
        await CheckLoggedInStatus("");
        App.ReloadGames();
    }
}