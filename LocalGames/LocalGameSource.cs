using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
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
    public string Version => "v1.0.3";
    public string SlugServiceName => "local-games";

    private IApp _app;
    public List<LocalGame> Games => _storage.Data.LocalGames;
    public List<GenerationRules> Rules => _storage.Data.GenerationRules;
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

        Log("Hello World!");
        return null;
    }

    public void Save() => _storage.Save();

    public void Log(string message, LogType type = LogType.Info) => _app.Logger.Log(message, type, "LocalGames");

    public async Task<List<IGame>> GetGames()
    {
        Games.ForEach(x => x.Source = this);

        List<GeneratedGame> generatedGames = new();

        foreach (var generationRules in Rules)
        {
            if (!Directory.Exists(generationRules.Path))
                continue;
            
            LocalGame? local = Games.Find(x => x.Name == generationRules.LocalGameName);
            if (local == null)
                continue;

            foreach (var enumerateFile in Directory.EnumerateFiles(generationRules.Path))
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
            new Command("Add a game", () => new AddOrEditGameGui(_app, this).ShowGui()),
            new Command("Reload", () => _app.ReloadGames()),
            Games.Count <= 0
                ? new Command("Add a generation rule")
                : new Command("Add a generation rule", () => new AddOrEditGenerationRules(_app, this).ShowGui())
            
        };

        List<Command> edits = Rules
            .Select(x => new Command($"Edit {x.Name}", () => new AddOrEditGenerationRules(_app, this).ShowGui(rules: x)))
            .ToList();

        if (edits.Count >= 0)
            commands.Add(new Command("Edit generation rule", edits));
        
        return commands;
    }

    public void Remove(LocalGame localGame)
    {
        _app.ShowTextPrompt($"Removing {localGame.Name}...");
        Games.Remove(localGame);
        _app.ReloadGames();
        Save();
        _app.HideForm();
    }
}