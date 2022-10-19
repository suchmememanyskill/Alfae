using ItchIoIntegration.Gui;
using ItchIoIntegration.Model;
using ItchIoIntegration.Requests;
using ItchIoIntegration.Service;
using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;

namespace ItchIoIntegration;

public class ItchGameSource : IGameSource
{
    public string ServiceName => "Itch.io Integration";
    public string Version => "v1.1.4";
    public string SlugServiceName => "itch-io";
    public string ShortServiceName => "Itch.io";

    private Config Config => _storage.Data;
    private Storage<Config> _storage;
    private bool _offline = false;
    public IApp App { get; private set; }

    public ItchApiProfile? Profile { get; private set; }
    private List<ItchGame> _games = new();
    public List<string> IgnoredExecutables { get; private set; } = new()
    {
        "UnityCrashHandler32.exe",
        "UnityCrashHandler64.exe",
    };

    public async Task Initialize(IApp app)
    {
        App = app;
        _storage = new(app, "itch.json");
        
        if (File.Exists(Path.Join(App.ConfigDir, "itch_ignore_execs.txt")))
            IgnoredExecutables.AddRange((await File.ReadAllLinesAsync(Path.Join(App.ConfigDir, "itch_ignore_execs.txt"))).Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)));
        
        await Load();
    }

    public async Task Load()
    {
        _games = new();
        Config.InstalledGames.ForEach(x => x.ItchSource = this);
        _games.AddRange(Config.InstalledGames);
        Profile = null;

        if (string.IsNullOrWhiteSpace(Config.ApiKey))
        {
            Log("Api key is empty!", LogType.Warn);
            return;
        }
        
        if (!await Utils.HasNetworkAsync())
        {
            Log("Cannot seem to connect online?");
            _offline = true;
            return;
        }
        
        Profile = await ItchApiProfile.Get(Config.ApiKey);
        if (Profile == null)
        {
            Log("Api key is invalid!", LogType.Error);
        }

        int lastKeyCount = 1;
        int page = 1;

        while (lastKeyCount > 0)
        {
            ItchApiProfileOwnedKeys? keys = await Profile.GetOwnedKeys(page);
            if (keys == null)
            {
                Log($"Failed to get games at page {page}!", LogType.Error);
                break;
            }

            page++;
            lastKeyCount = keys.OwnedKeys.Count;
            _games.AddRange(keys.OwnedKeys.Where(x => x.Game.Classification == "game")
                .Select(x => new ItchGame(x, this)));
        }

        _games.RemoveAll(x => Config.InstalledGames.Any(y => x.Id == y.Id && x.InstalledStatus == InstalledStatus.NotInstalled));
    }

    public async void LoadWithGui()
    {
        App.ShowTextPrompt("Reloading Itch.io games...");
        await Load();
        App.ReloadGames();
        App.HideForm();
    }

    public async void SetNewApiKey(string key)
    {
        Config.ApiKey = key;
        SaveConfig();
        LoadWithGui();
    }

    public async Task<List<IGame>> GetGames()
    {
        if (Profile == null)
            return _games.Select(x => (IGame) x).Where(x => x.InstalledStatus == InstalledStatus.Installed).ToList();
        else
            return _games.Select(x => (IGame) x).ToList();
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
        
        if (Profile == null)
        {
            return new()
            {
                new("Not logged in..."),
                new(),
                new("Log in", () => new LoginForm(this, App).ShowForm())
            };
        }

        return new List<Command>()
        {
            new($"Logged in as {Profile.User.Username}"),
            new(),
            new("Reload games", LoadWithGui),
            new("Logout", () => SetNewApiKey("")),
            new("Search free games/demos", () => new SearchForm(App, Profile!, this).ShowForm())
        };
    }

    public void Log(string message, LogType type = LogType.Info) => App.Logger.Log(message, type, "ItchIo");

    public List<Command> GetGameCommands(IGame game)
    {
        ItchGame? itchGame = game as ItchGame;
        if (itchGame == null)
            throw new InvalidDataException();

        List<Command> commands = new();
        
        if (itchGame.InstalledStatus == InstalledStatus.Installed)
        {
            commands.Add(new(game.IsRunning ? "Running" : "Launch", itchGame.Play));
            commands.Add(new("Config/Info", () => new GameOptionsGui(itchGame).ShowGui()));
            
            if (itchGame.GameUrl != null)
                commands.Add(new("View in browser", () => Utils.OpenUrl(itchGame.GameUrl.AbsoluteUri)));
            
            commands.Add(new("Uninstall", () => App.Show2ButtonTextPrompt($"Are you sure you want to uninstall {itchGame.Name}?", "Uninstall", "Back", x => Uninstall(itchGame), x => App.HideForm())));
        }
        else
        {
            if (itchGame.Download == null)
            {
                commands.Add(new("Install", () => new DownloadSelectForm(itchGame, App, this).InitiateForm()));
                if (itchGame.GameUrl != null)
                    commands.Add(new("View in browser", () => Utils.OpenUrl(itchGame.GameUrl.AbsoluteUri)));
            }
            else
            {
                commands.Add(new("Stop", () => itchGame.Download.Stop()));
            }
        }

        return commands;
    }

    public void AddToInstalled(ItchGame game)
    {
        Config.InstalledGames.Add(game);
        SaveConfig();
        App.ReloadGames();
    }

    public void AddFakeGameToGames(ItchGame fakeGame)
    {
        _games.Add(fakeGame);
        App.ReloadGames();
    }

    public async void Uninstall(ItchGame game)
    {
        App.ShowTextPrompt($"Uninstalling {game.Name}...");
        await game.UninstallGame();
        Config.InstalledGames.Remove(game);
        SaveConfig();
        App.ReloadGames();
        App.HideForm();
    }

    public void SaveConfig() => _storage.Save();
}