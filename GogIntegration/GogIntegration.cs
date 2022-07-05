using GogIntegration.Gui;
using GogIntegration.Model;
using GogIntegration.Requests;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Interfaces;
using Newtonsoft.Json;

namespace GogIntegration;

public class GogIntegration : IGameSource
{
    public string ServiceName => "GOG Integration";
    public string Version => "v1.0";
    public string SlugServiceName => "gog-integration";
    public string ShortServiceName => "GOG";

    public IApp App { get; private set; }
    public Config Config { get; private set; } = new();
    public GogApiUserData? UserData { get; private set; }
    private bool _successfulLogin = false;
    
    public static string IMAGECACHEDIR
    {
        get
        {
            string path = Path.Join(Path.GetTempPath(), "GOGPluginImageCache");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
    
    private string ConfigFile => Path.Join(App.ConfigDir, "gog.json");
    private List<GogGame> _games = new();

    public async Task Initialize(IApp app)
    {
        App = app;
        string configPath = ConfigFile;

        if (File.Exists(configPath))
        {
            string text = await File.ReadAllTextAsync(configPath);
            Config = JsonConvert.DeserializeObject<Config>(text)!;
        }

        await AttemptLogin();
        await ReloadGames();
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
            Config.Save(ConfigFile);

            if (Config.Auth == null)
                return null;
        }

        return Config.Auth;
    }

    public async Task ReloadGames()
    {
        _games = new();

        if (!_successfulLogin)
            return;

        GogApiAuth? auth = await GetAuth();

        if (auth == null)
            return;

        GogApiGames? games = await GogApiGames.Get(auth);

        if (games == null)
            return;

        // TODO: add all pages, not just the first one
        _games = games.Products.Select(x => new GogGame(this, x)).ToList();
    }

    public async Task<List<IGame>> GetGames() => _games.Select(x => (IGame) x).ToList();

    public async void Logout()
    {
        Config.Auth = null;
        Config.Save(ConfigFile);
        await AttemptLogin();
        await ReloadGames();
        App.ReloadGames();
    }

    public List<Command> GetGlobalCommands()
    {
        List<Command> commands = new();
        
        if (_successfulLogin)
        {
            commands.Add(new($"Logged in as {UserData!.Username}"));
            commands.Add(new());
            commands.Add(new("Log out", Logout));
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
        Config.Save(ConfigFile);
        await AttemptLogin();

        if (!_successfulLogin)
            return false;

        await ReloadGames();
        App.ReloadGames();
        return true;
    }
}