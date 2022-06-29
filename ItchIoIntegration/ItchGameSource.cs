using ItchIoIntegration.Gui;
using ItchIoIntegration.Model;
using ItchIoIntegration.Requests;
using ItchIoIntegration.Service;
using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
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
    private IApp _app;

    private ItchApiProfile? _profile;
    private List<ItchGame> _games = new();
    
    public async Task Initialize(IApp app)
    {
        _app = app;
        _config = Config.Load(app);
        await Load();
    }

    public async Task Load()
    {
        _games = new();
        _profile = null;
        if (string.IsNullOrWhiteSpace(_config.ApiKey))
        {
            Log("Api key is empty!", LogType.Warn);
            return;
        }
        
        _profile = await ItchApiProfile.Get(_config.ApiKey);
        if (_profile == null)
        {
            Log("Api key is invalid!", LogType.Error);
        }

        int lastKeyCount = 1;
        int page = 1;

        while (lastKeyCount > 0)
        {
            ItchApiProfileOwnedKeys? keys = await _profile.GetOwnedKeys(page);
            if (keys == null)
            {
                Log("Failed to get games!", LogType.Error);
                break;
            }

            page++;
            lastKeyCount = keys.OwnedKeys.Count;
            _games.AddRange(keys.OwnedKeys.Select(x => new ItchGame(x, this)).Where(x => x.IsGame));
        }
    }

    public async void LoadWithGui()
    {
        _app.ShowTextPrompt("Reloading Itch.io games...");
        await Load();
        _app.ReloadGames();
        _app.HideOverlay();
    }

    public async void SetNewApiKey(string key)
    {
        _config.ApiKey = key;
        _config.Save(_app);
        LoadWithGui();
    }

    public async Task<List<IGame>> GetGames() => _games.Select(x => (IGame) x).ToList();

    public List<Command> GetGlobalCommands()
    {
        if (_profile == null)
        {
            return new()
            {
                new("Not logged in..."),
                new(),
                new("Log in", () => new LoginForm(this, _app).ShowForm())
            };
        }

        return new List<Command>()
        {
            new($"Logged in as {_profile.User.Username}"),
            new(),
            new("Reload games", LoadWithGui),
            new("Logout", () => SetNewApiKey(""))
        };
    }

    public void Log(string message, LogType type = LogType.Info) => _app.Logger.Log(message, type, "ItchIo");
}