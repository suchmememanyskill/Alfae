using System.Diagnostics;
using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Extensions;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LocalGames.Data;
using LocalGames.Gui;
using Newtonsoft.Json;

namespace LocalGames;

public class LocalGameSource : IGameSource
{
    public string ServiceName => "Local Games Integration";
    public string ShortServiceName => "Local";
    public string Version => "v2.0.1";
    public string SlugServiceName => "local-games";
    public PluginType Type => PluginType.GameSource;

    private IApp _app;
    public List<LocalGame> Games => _storage.Data.LocalGames;
    public List<GenerationRules> Rules => _storage.Data.GenerationRules;
    public Dictionary<string, List<AppListing>> ExternalApps { get; private set; }
    private Storage<Store> _storage;

    public async Task<InitResult?> Initialize(IApp app)
    {
        _app = app;
        _storage = new(app, "localgames_v2.json");
        
        Storage<List<LocalGame>> legacy = new(app, "localgames.json");

        if (File.Exists(legacy.Path))
        {
            _storage.Data.LocalGames = legacy.Data;
            _storage.Save();
            File.Delete(legacy.Path);
        }

        ExternalApps = await GetExternalApps();
        Log("Hello World!");
        return null;
    }

    public void Save() => _storage.Save();

    public void Log(string message, LogType type = LogType.Info) => _app.Logger.Log(message, type, "LocalGames");

    public async Task<List<IGame>> GetGames()
    {
        int idMissingCount = 0;
        
        foreach (var localGame in Games)
        {
            localGame.Source = this;

            if (string.IsNullOrWhiteSpace(localGame.InternalName))
            {
                idMissingCount++;
                localGame.InternalName = Guid.NewGuid().ToString();
            }
        }

        if (idMissingCount > 0)
            Save();
        
        List<GeneratedGame> generatedGames = new();

        foreach (var generationRules in Rules)
        {
            if (!Directory.Exists(generationRules.Path))
                continue;
            
            LocalGame? local = Games.Find(x => x.Name == generationRules.LocalGameName);
            if (local == null)
                continue;

            foreach (var enumerateFile in Directory.EnumerateFiles(generationRules.Path, "*", (generationRules.DrillDown) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                if (generationRules.Extensions.Any(x => enumerateFile.EndsWith(x)))
                {
                    GeneratedGame game = new(local, this,
                        generationRules.AdditionalCliArgs.Replace("{EXEC}", $"\"{enumerateFile}\""), enumerateFile);
                    generatedGames.Add(game);
                }
            }
        }

        List<IGame> games = Games.Select(x => (IGame)x).ToList();
        games.AddRange(generatedGames.Select(x => (IGame)x));

        return games;
    }

    public List<Command> GetGameCommands(IGame game)
    {
        if (game is LocalGame localGame)
        {
            return new()
            {
                new Command(game.IsRunning ? "Running" : "Launch", () =>
                {
                    Log($"Starting local game {localGame.Name}");
                    _app.Launch(localGame.ToExecLaunch());
                }),
                new Command("Edit", () => new AddOrEditGameGui(_app, this).ShowGui(game: localGame)),
                new Command("Remove From Launcher", () =>
                {
                    _app.Show2ButtonTextPrompt($"Are you sure you want to remove '{localGame.Name}' from the launcher?", "Remove", "Back", x => Remove(localGame), x => _app.HideForm());
                }),
            };
        }

        if (game is GeneratedGame generatedGame)
        {
            return new()
            {
                new Command(game.IsRunning ? "Running" : "Launch", () =>
                {
                    Log($"Starting generated game {generatedGame.Name}");
                    _app.Launch(generatedGame.ToExecLaunch());
                }),
                new Command("Open File Location", () => Utils.OpenFolderWithHighlightedFile(generatedGame.FilePath))
            };
        }

        throw new NotImplementedException();
    }

    public List<Command> GetGlobalCommands()
    {
        List<Command> commands = new()
        {
            new Command($"Loaded {Games.Count} games"),
            new Command(),
            new Command("Reload", Reload),
            new Command("Add a game", () => new AddOrEditGameGui(_app, this).ShowGui()),
            
        };

        if (ExternalApps.Count > 0)
        {
            List<Command> existingSources = ExternalApps.Select(x => new Command(x.Key, x.Value.Select(y => new Command(y.Name, () => ShowExistingGuiModal(y))).ToList())).ToList();
            commands.Add(new("Add a game from existing app", existingSources));
        }

        commands.Add(Games.Count <= 0
            ? new Command("Add a generation rule")
            : new Command("Add a generation rule", () => new AddOrEditGenerationRules(_app, this).ShowGui()));
        
        List<Command> edits = Rules
            .Select(x => new Command($"Edit {x.Name}", () => new AddOrEditGenerationRules(_app, this).ShowGui(rules: x)))
            .ToList();

        if (edits.Count >= 1)
            commands.Add(new Command("Edit generation rule", edits));
        
        return commands;
    }

    private async Task<Dictionary<string, List<AppListing>>> GetExternalApps()
    {
        Dictionary<string, List<AppListing>> items = new();
        
        // Flatpak
        if (PlatformExtensions.CurrentPlatform == Platform.Linux)
        {
            Terminal t = new(_app);
            string? flatpakLoc = Utils.WhereSearch("flatpak");

            if (flatpakLoc != null)
            {
                if (await t.Exec(flatpakLoc, "list --app --columns=name,application"))
                {
                    List<AppListing> flatpaks = new();
                    
                    foreach (var s in t.StdOut.Where(x => !x.Contains("Application ID"))) // Skip possible first header line
                    {
                        List<string> split = s.Split("	").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();

                        if (split.Count == 2)
                        {
                            AppListing listing = new(split[0], flatpakLoc, $"run {split[1]}");
                            flatpaks.Add(listing);
                        }
                    }
                    
                    if (flatpaks.Count > 0)
                        items.Add("Flatpak", flatpaks.OrderBy(x => x.Name).ToList());
                }
            }
        }
        
        // Win Apps
        if (PlatformExtensions.CurrentPlatform == Platform.Windows)
        {
            await Task.Run(() => GetWinStartApps(ref items));
        }
        
        return items;
    }

    private void GetWinStartApps(ref Dictionary<string, List<AppListing>> items)
    {
        Utils.DirRepresentation startMenu =
            Utils.GetDirRepresentation(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));

        List<AppListing> userStartMenu = new();
            
        foreach (var (key, value) in startMenu.Files.Where(x => x.Key.EndsWith(".lnk")))
        {
            var link = Lnk.Lnk.LoadFile(key);

            if (File.Exists(link.LocalPath))
            {
                userStartMenu.Add(new(Path.GetFileName(key)[..^4], link.LocalPath, link.Arguments ?? ""));
            }
        }
            
        if (userStartMenu.Count > 0)
            items.Add("User Start Menu", userStartMenu.OrderBy(x => x.Name).ToList());

        Utils.DirRepresentation commonStartMenu =
            Utils.GetDirRepresentation(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));

        List<AppListing> commonStart = new();
            
        foreach (var (key, value) in commonStartMenu.Files.Where(x => x.Key.EndsWith(".lnk")))
        {
            var link = Lnk.Lnk.LoadFile(key);

            if (File.Exists(link.LocalPath))
            {
                commonStart.Add(new(Path.GetFileName(key)[..^4], link.LocalPath, link.Arguments ?? ""));
            }
        }
            
        if (commonStart.Count > 0)
            items.Add("Common Start Menu", commonStart.OrderBy(x => x.Name).ToList());
    }

    private async void Reload()
    {
        _app.ShowTextPrompt("Reloading Local Games...");
        _app.ReloadGames();
        ExternalApps = await GetExternalApps();
        _app.HideForm();
    }

    public void Remove(LocalGame localGame)
    {
        _app.ShowTextPrompt($"Removing {localGame.Name}...");
        Games.Remove(localGame);
        _app.ReloadGames();
        Save();
        _app.HideForm();
    }

    public void ShowExistingGuiModal(AppListing appListing)
    {
        new AddOrEditGameGui(_app, this).ShowGui(gameName: appListing.Name, execPath: appListing.ExecPath, args: appListing.CliArgs);
    }
}