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

// TODO: Offline compatibility
public class ItchGameSource : IGameSource
{
    public string ServiceName => "Itch.io Integration";
    public string Description => "Integrates Itch.io games";
    public string Version => "v0.1";
    public string SlugServiceName => "itch-io";
    public string ShortServiceName => "Itch.io";

    private Config _config;
    public IApp App { get; private set; }

    public ItchApiProfile? Profile { get; private set; }
    private List<ItchGame> _games = new();
    
    public static string IMAGECACHEDIR
    {
        get
        {
            string path = Path.Join(Path.GetTempPath(), "ItchIoPluginImageCache");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
    
    public async Task Initialize(IApp app)
    {
        App = app;
        _config = Config.Load(app);
        await Load();
    }

    public async Task Load()
    {
        _games = new();
        _config.InstalledGames.ForEach(x => x.ItchSource = this);
        _games.AddRange(_config.InstalledGames);
        Profile = null;
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            Log("Api key is empty!", LogType.Warn);
            return;
        }
        
        Profile = await ItchApiProfile.Get(_config.ApiKey);
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
                Log("Failed to get games!", LogType.Error);
                break;
            }

            page++;
            lastKeyCount = keys.OwnedKeys.Count;
            _games.AddRange(keys.OwnedKeys.Where(x => x.Game.Classification == "game")
                .Select(x => new ItchGame(x, this)));
        }

        _games.RemoveAll(x => _config.InstalledGames.Any(y => x.Id == y.Id && x.InstalledStatus == InstalledStatus.NotInstalled));
    }

    public async void LoadWithGui()
    {
        App.ShowTextPrompt("Reloading Itch.io games...");
        await Load();
        App.ReloadGames();
        App.HideOverlay();
    }

    public async void SetNewApiKey(string key)
    {
        _config.ApiKey = key;
        _config.Save(App);
        LoadWithGui();
    }

    public async Task<List<IGame>> GetGames() => _games.Select(x => (IGame) x).ToList();

    public List<Command> GetGlobalCommands()
    {
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
            new("Logout", () => SetNewApiKey(""))
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
            commands.Add(new("Launch", itchGame.Play));
            
            if (itchGame.Targets.Count > 1)
            {
                commands.Add(new("Configure", () => new GameOptionsGui(itchGame).ShowGui()));
            }
            
            commands.Add(new("Uninstall", () => App.Show2ButtonTextPrompt($"Are you sure you want to uninstall {itchGame.Name}?", "Uninstall", "Back", x => Uninstall(itchGame), x => App.HideOverlay())));
        }
        else
        {
            if (itchGame.Download == null)
            {
                commands.Add(new("Install", () => new DownloadSelectForm(itchGame, App, this).InitiateForm()));
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
        _config.InstalledGames.Add(game);
        _config.Save(App);
        App.ReloadGames();
    }

    public async void Uninstall(ItchGame game)
    {
        App.ShowTextPrompt($"Uninstalling {game.Name}...");
        await game.UninstallGame();
        _config.InstalledGames.Remove(game);
        _config.Save(App);
        App.ReloadGames();
        App.HideOverlay();
    }

    public void SaveConfig() => _config.Save(App);
}