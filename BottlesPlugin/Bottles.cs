using BottlesPlugin.Model;
using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;
using Newtonsoft.Json;

namespace BottlesPlugin;

public class Bottles : IGameSource
{
    public string ServiceName => "Bottles Integration";
    public string Version => "v1.0.3";
    public string SlugServiceName => "bottles";
    public string ShortServiceName => "bottles";
    private string _message = "";
    private List<BottlesWrapper> _wrappers = new();
    private List<BottlesProgram> _games = new();
    private IApp _app;
    private Config Config => _storage.Data;
    private Storage<Config> _storage;

    public async Task Initialize(IApp app)
    {
        _app = app;
        _storage = new(app, "bottles.json");
        await LoadBottles();
    }

    public async Task LoadBottles()
    {
        _wrappers = new();
        _games = new();
        _message = "";
        
        if (PlatformExtensions.CurrentPlatform == Platform.Windows)
        {
            _message = "Bottles is not supported on windows";
            return;
        }

        Terminal t = new(_app);
        if (!await t.Exec("flatpak", "run --command=bottles-cli com.usebottles.bottles --json list bottles"))
        {
            _message = "Flatpak is not installed";
            return;
        }

        if (t.ExitCode != 0)
        {
            _message = "Couldn't read bottles";
            return;
        }

        Dictionary<string, Bottle> bottles =
            JsonConvert.DeserializeObject<Dictionary<string, Bottle>>(t.StdOut.First())!;
        
        foreach (var (key, value) in bottles)
        {
            BottlesWrapper wrapper = new($"Bottle: {value.Name}", key);
            _wrappers.Add(wrapper);
            if (Config.ImportPrograms)
            {
                List<BottlesProgram> programs =
                    value.Programs.Select(x => new BottlesProgram(x.Value.Name, key, this)).ToList();
                _games.AddRange(programs);
            }
        }
    }

    public void SaveConfig() => _storage.Save();

    public async void Reload()
    {
        _app.ShowTextPrompt("Reloading bottles...");
        await LoadBottles();
        _app.ReloadGames();
        _app.HideForm();
    }

    public void SetOrUnsetImportGames()
    {
        Config.ImportPrograms = !Config.ImportPrograms;
        SaveConfig();
        Reload();
    }

    public async Task<List<IGame>> GetGames() => _games.Select(x => (IGame)x).ToList();
    public async Task<List<IBootProfile>> GetBootProfiles() => _wrappers.Select(x => (IBootProfile)x).ToList();
    public List<Command> GetGlobalCommands()
    {
        List<Command> commands = new();
        if (_message != "")
            commands.Add(new(_message));

        if (PlatformExtensions.CurrentPlatform != Platform.Windows)
        {
            commands.Add(new("Reload", Reload));
            commands.Add(new(Config.ImportPrograms ? "Press to not import programs" : "Press to import programs", SetOrUnsetImportGames));
        }

        return commands;
    }

    public List<Command> GetGameCommands(IGame game)
    {
        BottlesProgram? program = game as BottlesProgram;
        if (program == null)
            throw new InvalidDataException();

        return new()
        {
            new(game.IsRunning ? "Running" : "Launch", () => program.Launch(_app))
        };
    }
}