using GogIntegration.Gui;
using GogIntegration.Model;
using GogIntegration.Requests;
using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using Newtonsoft.Json;

namespace GogIntegration;

public class GogIntegration : IGameSource
{
    public string ServiceName => "GOG Integration";
    public string Version => "v1.1.2";
    public string SlugServiceName => "gog-integration";
    public string ShortServiceName => "GOG";

    public IApp App { get; private set; }
    public Config Config => _storage.Data;
    public Storage<Config> _storage;
    public GogApiUserData? UserData { get; private set; }
    private bool _successfulLogin = false;
    private List<GogGame> _games = new();
    private bool _offline = false;

    public async Task<InitResult?> Initialize(IApp app)
    {
        App = app;
        _storage = new(app, "gog.json");
        _offline = !await Utils.HasNetworkAsync();

        if (!_offline)
            await AttemptLogin();
        
        await ReloadGames();
        return null;
    }

    public async Task AttemptLogin()
    {
        _successfulLogin = false;
        GogApiAuth? auth = await GetAuth();
        if (auth == null)
            return;

        UserData = await GogApiUserData.Get(auth);
        _successfulLogin = (UserData != null);
    }

    public async Task<GogApiAuth?> GetAuth()
    {
        if (Config.Auth == null)
            return null;

        if (Config.Auth.NeedsRefresh())
        {
            Config.Auth = await Config.Auth.Refresh();
            SaveConfig();

            if (Config.Auth == null)
                return null;
        }

        return Config.Auth;
    }

    public async Task ReloadGames()
    {
        _games = new(Config.InstalledGames);
        _games.ForEach(x => x.GogSource = this);

        if (!_successfulLogin)
            return;

        GogApiAuth? auth = await GetAuth();

        if (auth == null)
            return;

        int page = 0;
        int totalPages = -1;

        do
        {
            GogApiGames? games = await GogApiGames.Get(auth, page + 1);

            if (games == null)
                return;

            totalPages = games.TotalPages;
            page = games.Page;
            
            _games.AddRange(games.Products.Where(x => _games.All(y => x.Id != y.Id)).Select(x => new GogGame(this, x)));
        } while (page <= totalPages);
    }

    public async Task<List<IGame>> GetGames() => _games.Select(x => (IGame) x).ToList();

    public async void Logout()
    {
        Config.Auth = null;
        SaveConfig();
        await AttemptLogin();
        await ReloadGames();
        App.ReloadGames();
    }

    public List<Command> GetGlobalCommands()
    {
        if (_offline)
        {
            return new()
            {
                new("Offline...")
            };
        }
        
        List<Command> commands = new();
        
        if (_successfulLogin)
        {
            commands.Add(new($"Logged in as {UserData!.Username}"));
            commands.Add(new());
            commands.Add(new("Log out", Logout));
            commands.Add(new("Reload games", ReloadGamesMenu));
        }
        else
        {
            commands.Add(new("Not logged in..."));
            commands.Add(new());
            commands.Add(new("Log in", () => new LogInGui(this).Show()));
        }

        return commands;
    }

    public async Task<bool> Login(GogApiAuth auth)
    {
        Config.Auth = auth;
        SaveConfig();
        await AttemptLogin();

        if (!_successfulLogin)
            return false;

        await ReloadGames();
        App.ReloadGames();
        return true;
    }

    public List<Command> GetGameCommands(IGame game)
    {
        GogGame? gogGame = game as GogGame;

        if (gogGame == null)
            throw new Exception("???");

        List<Command> commands = new();

        if (gogGame.ProgressStatus != null)
        {
            commands.Add(new("Stop", gogGame.DownloadStatus.Stop));
            return commands;
        }

        if (gogGame.InstalledStatus == InstalledStatus.NotInstalled)
        {
            commands.Add(new("Install", () => gogGame.Download()));
            
            if (gogGame.Size == 0)
                commands.Add(new("Get game size", () => gogGame.GetDlInfo()));
            
            commands.Add(new("View in browser", () => Utils.OpenUrl(gogGame.PageUrl)));
        }
        else
        {
            commands.Add(new(game.IsRunning ? "Running" : "Launch", () => Play(gogGame)));
            commands.Add(new("Config/Info", () => new ConfigurationGui(this, gogGame).Show()));
            commands.Add(new("View in browser", () => Utils.OpenUrl(gogGame.PageUrl)));
            commands.Add(new("Uninstall", () => App.Show2ButtonTextPrompt($"Are you sure you want to uninstall {gogGame.Name}?","Uninstall", "Back", x => Uninstall(gogGame), x => App.HideForm())));
        }

        return commands;
    }

    public void FinalizeDownload(GogGame game)
    {
        Config.InstalledGames.Add(game);
        SaveConfig();
        App.ReloadGames();
    }

    public async void Uninstall(GogGame game)
    {
        App.ShowTextPrompt($"Uninstalling {game.Name}...");
        await Task.Run(game.Uninstall);
        Config.InstalledGames.Remove(game);
        SaveConfig();
        App.ReloadGames();
        App.HideForm();
    }

    public async void ReloadGamesMenu()
    {
        App.ShowTextPrompt("Reloading GOG games...");
        await ReloadGames();
        App.ReloadGames();
        App.HideForm();
    }

    public void Play(GogGame game)
    {
        try
        {
            game.Play();
        }
        catch (Exception e)
        {
            App.ShowDismissibleTextPrompt(e.Message);
        }
    }

    public void SaveConfig() => _storage.Save();
}