using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Extensions;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LocalGames.Data;
using Newtonsoft.Json;

namespace LocalGames;

public class LocalGameSource : IGameSource
{
    public string ServiceName => "Local Games Integration";
    public string ShortServiceName => "Local";
    public string Version => "v1.0.0";
    public string SlugServiceName => "local-games";

    private IApp _app;
    private List<LocalGame> _games = new();
    private string _configPath;
    
    public async Task Initialize(IApp app)
    {
        _app = app;

        _configPath = Path.Join(_app.ConfigDir, "localgames.json");
        if (File.Exists(_configPath))
        {
            string json = await File.ReadAllTextAsync(_configPath);
            _games = JsonConvert.DeserializeObject<List<LocalGame>>(json);
        }
        
        Log("Hello World!");
    }

    public async Task Save()
    {
        string json = JsonConvert.SerializeObject(_games);
        await File.WriteAllTextAsync(_configPath, json);
    }
    
    public void AddGameForm(string possibleWarn = "", string gameName = "", string execPath = "", string coverImage = "", string backgroundImage = "", string args = "", LocalGame? game = null)
    {
        string addOrEdit = game == null ? "Add" : "Edit";

        if (game != null)
        {
            if (gameName == "")
                gameName = game.Name;

            if (execPath == "")
                execPath = game.ExecPath;

            if (coverImage == "")
                coverImage = game.CoverImagePath;

            if (backgroundImage == "")
                backgroundImage = game.BackgroundImagePath;

            if (args == "")
                args = game.LaunchArgs;
        }

        List<FormEntry> entries = new()
        {
            new FormEntry(FormEntryType.TextBox, $"{addOrEdit} a local game", "Bold"),
            new FormEntry(FormEntryType.TextInput, "Game name:", gameName),
            new FormEntry(FormEntryType.FilePicker, "Game executable:", execPath),
            new FormEntry(FormEntryType.TextBox, "\nOptional", "Bold"),
            new FormEntry(FormEntryType.FilePicker, "Cover Image:", coverImage),
            new FormEntry(FormEntryType.FilePicker, "Background Image:", backgroundImage),
            new FormEntry(FormEntryType.TextInput, "CLI Arguments:", args),
            Form.Button("Cancel", _ => _app.HideForm(), addOrEdit, entry =>
            {
                _app.HideForm();
                new Thread(() => AddGame(entry)).Start();
            })
        };
        
        if (possibleWarn != "")
            entries.Add(new(FormEntryType.TextBox, possibleWarn, "Bold"));
        
        Form form = new(entries);
        if (game != null)
        {
            form.Game = game;
            form.Background = game.BackgroundImage;
        }
        _app.ShowForm(form);
    }

    public void AddGame(Form form)
    {
        string? gameName = form.GetValue("Game name:");
        string? execPath = form.GetValue("Game executable:");
        string? coverImage = form.GetValue("Cover Image:");
        string? backgroundImage = form.GetValue("Background Image:");
        string? args = form.GetValue("CLI Arguments:");
        string errMessage = "";
        
        if (string.IsNullOrWhiteSpace(gameName))
            errMessage = "Please fill in the game name";

        if (string.IsNullOrWhiteSpace(execPath) && errMessage == "")
            errMessage = "Please fill in the executable path";

        if (!File.Exists(execPath) && errMessage == "")
            errMessage = "Executable path does not exist!";

        if (errMessage == "" && coverImage != "" && !File.Exists(coverImage))
            errMessage = "Cover image path does not exist!";

        if (errMessage == "" && coverImage != "" && !File.Exists(backgroundImage))
            errMessage = "Background image path does not exist!";

        if (errMessage != "")
        {
            AddGameForm(errMessage, gameName, execPath,coverImage, backgroundImage, args, form.Game as LocalGame);
            return;
        }
        
        Log($"Calculating game {gameName} size at path {execPath}");
        _app.ShowTextPrompt($"Processing {gameName}...");
        LocalGame localGame;
        
        if (form.Game == null)
            localGame = new LocalGame();
        else
            localGame = (form.Game as LocalGame)!;

        localGame.Name = gameName;
        localGame.ExecPath = execPath;
        localGame.Size = Utils.DirSize(new DirectoryInfo(localGame.InstalledPath));
        localGame.CoverImagePath = coverImage;
        localGame.BackgroundImagePath = backgroundImage;
        localGame.LaunchArgs = args;
        Log($"{gameName}'s size is {localGame.ReadableSize()}");
        
        if (form.Game == null)
        {
            _games.Add(localGame);
            Log($"Added game {gameName}");
        }

        _app.ReloadGames();
        Save();
        _app.HideForm();
    }

    public void Log(string message, LogType type = LogType.Info) => _app.Logger.Log(message, type, "LocalGames");

    public async Task<List<IGame>> GetGames()
    {
        _games.ForEach(x => x.Source = this);
        return _games.Select(x => (IGame)x).ToList();
    }

    public List<Command> GetGameCommands(IGame game)
    {
        LocalGame localGame = game as LocalGame;

        return new()
        {
            new Command("Launch", () =>
            {
                Log($"Starting {localGame.Name}");
                _app.Launch(localGame.ToExecLaunch());
            }),
            new Command("Edit", () => AddGameForm(game: localGame)),
            new Command("Remove From Launcher", () =>
            {
                _app.Show2ButtonTextPrompt($"Are you sure you want to remove '{localGame.Name}' from the launcher?", "Remove", "Back", x => Remove(localGame), x => _app.HideForm());
            }),
        };
    }

    public List<Command> GetGlobalCommands() => new()
    {
        new Command($"Loaded {_games.Count} games"),
        new Command(),
        new Command("Add a game", () => AddGameForm())
    };

    public async void Remove(LocalGame localGame)
    {
        _app.ShowTextPrompt($"Removing {localGame.Name}...");
        _games.Remove(localGame);
        _app.ReloadGames();
        _app.HideForm();
        await Save();
        _app.HideForm();
    }
    
}