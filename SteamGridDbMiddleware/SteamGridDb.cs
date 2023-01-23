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
    public string Version => "v1.2.0";
    public string SlugServiceName => "steam-grid-db";
    public string ShortServiceName => "steamgriddb";
    public IApp App { get; set; }
    public craftersmine.SteamGridDBNet.SteamGridDb? Api { get; set; }
    public Storage<Store> Storage { get; set; }
    private Dictionary<string, string> _searchTermTracker = new();
    public static List<ImageType> ImageTypes { get; } = new() { ImageType.VerticalCover, ImageType.HorizontalCover, ImageType.Background, ImageType.Logo, ImageType.Icon };
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
        List<Command> commands = new()
        {
            new("Wiki", () => Utils.OpenUrl("https://github.com/suchmememanyskill/Alfae/wiki/SteamGridDb-Integration")),
            new(),
        };

        if (Api == null)
        {
            commands.Add(new("Not logged in"));
            commands.Add(new());
            commands.Add(new("Log in", () => new Authenticate(this).ShowGui()));
        }
        else
        {
            commands.Add(new("Logged in"));
            commands.Add(new());
            commands.Add(new("Log out", Logout));
            commands.Add(new("Set missing images on installed games", SetFirstImageOnInstalledMissingImages));
        }

        return commands;
    }

    public async void SetFirstImageOnInstalledMissingImages()
    {
        App.ShowTextPrompt("Looking up images for installed games with missing content...");
        int imageCount = 0;
        List<IGame> games = App.GetAllGames().Where(x => x.InstalledStatus == InstalledStatus.Installed).ToList();

        foreach (var game in games)
        {
            List<ImageType> missingTypes = ImageTypes.Where(x => !game.HasImage(x)).ToList();
            
            if (missingTypes.Count <= 0)
                continue;
            
            var gridGames = await Api.SearchForGamesAsync(game.Name);
            
            if (gridGames.Length <= 0)
                continue;
                
            var grid = gridGames.First();
            
            foreach (var missingType in missingTypes)
            {
                imageCount++;
                List<Override> overrides = await GetOverridesForImageType(grid, missingType);
                
                if (overrides.Count <= 0)
                    continue;

                Override first = overrides.First();
                Storage.Data.SetOverride(game, missingType, first);
            }
        }
        
        Storage.Save();
        App.ReloadGames();
        App.ShowDismissibleTextPrompt($"Downloaded and set {imageCount} images");
    }

    public async Task<List<Override>> GetOverridesForImageType(SteamGridDbGame game, ImageType type)
    {
        if (Api == null)
            return new();

        try
        {
            if (type == ImageType.VerticalCover)
                return ((await Api.GetGridsForGameAsync(game, dimensions: SteamGridDbDimensions.W600H900,
                        types: SteamGridDbTypes.Static))?.ToList() ?? new())
                    .Select(x => new Override(x.FullImageUrl, x.Id, x.Author.Name)).ToList();

            if (type == ImageType.HorizontalCover)
                return ((await Api.GetGridsForGameAsync(game,
                        dimensions: SteamGridDbDimensions.W460H215 | SteamGridDbDimensions.W920H430,
                        types: SteamGridDbTypes.Static))?.ToList() ?? new())
                    .Select(x => new Override(x.FullImageUrl, x.Id, x.Author.Name)).ToList();

            if (type == ImageType.Background)
                return ((await Api.GetHeroesForGameAsync(game,
                        dimensions: SteamGridDbDimensions.W1920H620 | SteamGridDbDimensions.W3840H1240,
                        types: SteamGridDbTypes.Static))?.ToList() ?? new())
                    .Select(x => new Override(x.FullImageUrl, x.Id, x.Author.Name)).ToList();

            if (type == ImageType.Logo)
                return ((await Api.GetLogosForGameAsync(game, types: SteamGridDbTypes.Static,
                        formats: SteamGridDbFormats.Png))?.ToList() ?? new())
                    .Select(x => new Override(x.FullImageUrl, x.Id, x.Author.Name)).ToList();

            if (type == ImageType.Icon)
                return ((await Api.GetIconsForGameAsync(game, types: SteamGridDbTypes.Static))?.ToList() ?? new())
                    .Select(x => new Override(x.FullImageUrl, x.Id, x.Author.Name)).ToList();
        }
        catch (Exception e)
        {
            App.Logger.Log($"Failed to get images: {e.Message}", LogType.Warn, "SteamGridDb");
            return new();
        }

        throw new NotImplementedException();
    }

    public string CacheSearchTerm(IGame game, string defaultSearch)
    {
        string key = $"{game.Source.ShortServiceName}:{game.InternalName}";
        if (_searchTermTracker.ContainsKey(key))
            return _searchTermTracker[key];

        SetSearchTermCache(game, defaultSearch);
        return defaultSearch;
    }

    public void SetSearchTermCache(IGame game, string value)
    {
        string key = $"{game.Source.ShortServiceName}:{game.InternalName}";
        _searchTermTracker[key] = value;
    }

    private async void Logout()
    {
        Storage.Data.ApiKey = "";
        Storage.Save();
        await CheckLoggedInStatus("");
        App.ReloadGames();
    }
}