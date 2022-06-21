using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Extensions;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LocalGames.Data;

namespace LocalGames;

public class LocalGameSource : IGameSource
{
    public string ServiceName => "Local Games Integration";
    public string Description => "Games added manually will be shown using this plugin";
    public string Version => "v0.1";
    public string SlugServiceName => "local-games";
    public List<Command> GameCommands { get; private set; }
    public List<Command> GlobalCommands { get; private set; }

    private IApp _app;
    private List<LocalGame> Games = new();
    
    public async Task Initialize(IApp app)
    {
        _app = app;
        Log("Hello World!");
        GameCommands = new();
        GlobalCommands = new();
        
        GlobalCommands.Add(new Command("Sup"));
        GlobalCommands.Add(new Command());
        GlobalCommands.Add(new Command("Attempt a log", () => Log("Test")));
        GlobalCommands.Add(new Command("Show all kinds of controls", ShowAllForm));
        GlobalCommands.Add(new Command("Add a game", () => AddGameForm()));
        
        await Task.Delay(1);
    }

    public void ShowAllForm()
    {
        List<FormEntry> entries = new()
        {
            new FormEntry(FormEntryType.TextBox, "hello this is a piece of text", "Bold"),
            new FormEntry(FormEntryType.ClickableLinkBox, "Open google", "https://google.com",
                linkClick: entry => Utils.OpenUrl(entry.Value)),
            new FormEntry(FormEntryType.TextInput, "Input some text!", "Test"),
            new FormEntry(FormEntryType.Toggle, "Hello i'm a toggle", "1"),
            new FormEntry(FormEntryType.FilePicker, "Pick File:"),
            new FormEntry(FormEntryType.FolderPicker, "Pick Folder:"),
            new FormEntry(FormEntryType.Dropdown, "Pick an option", "b", new() {"a", "b", "c"}),
            new FormEntry(FormEntryType.ButtonList, "", buttonList: new Dictionary<string, Action<FormEntry>>()
            {
                {"button1", x => Log("button1")},
                {"Back", x => _app.HideOverlay()}
            })
        };
        
        _app.ShowForm(new(entries));
    }

    public void AddGameForm(string possibleWarn = "", string gameName = "", string execPath = "", string coverImage = "", string backgroundImage = "", string args = "")
    {
        List<FormEntry> entries = new()
        {
            new FormEntry(FormEntryType.TextBox, "Add a local game", "Bold"),
            new FormEntry(FormEntryType.TextInput, "Game name:", gameName),
            new FormEntry(FormEntryType.FilePicker, "Game executable:", execPath),
            new FormEntry(FormEntryType.TextBox, "\nOptional", "Bold"),
            new FormEntry(FormEntryType.FilePicker, "Cover Image:", coverImage),
            new FormEntry(FormEntryType.FilePicker, "Background Image:", backgroundImage),
            new FormEntry(FormEntryType.TextInput, "CLI Arguments:", args),
            new FormEntry(FormEntryType.ButtonList, "", buttonList: new()
            {
                {"Cancel", entry => _app.HideOverlay()},
                {
                    "Add", entry =>
                    {
                        _app.HideOverlay();
                        new Thread(() => AddGame(entry.ContainingForm)).Start();
                    }
                }
            })
        };
        
        if (possibleWarn != "")
            entries.Add(new(FormEntryType.TextBox, possibleWarn, "Bold"));
        
        _app.ShowForm(new(entries));
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
            AddGameForm(errMessage, gameName, execPath,coverImage);
            return;
        }
        
        Log($"Calculating game {gameName} size at path {execPath}");
        LocalGame localGame = new LocalGame();
        localGame.Name = gameName;
        localGame.ExecPath = execPath;
        localGame.Size = Utils.DirSize(new DirectoryInfo(localGame.InstalledPath));
        localGame.CoverImagePath = coverImage;
        localGame.BackgroundImagePath = backgroundImage;
        localGame.LaunchArgs = args;
        Log($"{gameName}'s size is {localGame.ReadableSize()}");
        Games.Add(localGame);
        Log($"Added game {gameName}");
        _app.ReloadGames();
    }

    public void Log(string message, LogType type = LogType.Info) => _app.Logger.Log(message, type, "LocalGames");

    public async Task<List<IGame>> GetGames()
    {
        Games.ForEach(x => x.Source = this);
        return Games.Select(x => (IGame)x).ToList();
    }

    public Task CustomCommand(string command, IGame? game)
    {
        throw new NotImplementedException();
    }

    public void EditGameForm(IGame game)
    {
        List<FormEntry> entries = new()
        {
            new FormEntry(FormEntryType.ButtonList, "", buttonList: new()
            {
                {"Back", x => _app.HideOverlay()}
            })
        };

        Form form = new(entries);
        form.Background = game.BackgroundImage;
        _app.ShowForm(form);
    }

    public List<Command> GetGameCommands(IGame game)
    {
        LocalGame localGame = game as LocalGame;

        return new()
        {
            new Command("Play", () =>
            {
                Log($"Starting {localGame.Name}");
                _app.Launch(localGame.ToExecLaunch());
            }),
            new Command("Delete", () =>
            {
                Games.Remove(localGame);
                _app.ReloadGames();
            }),
            new("Set download active", () =>
            {
                localGame.ProgressStatus = new ProgressStatus();
                localGame.ProgressStatus.Percentage = 0;
                localGame.ProgressStatus.Line1 = "Line 1";
                localGame.InvokeOnUpdate();
            }),
            new("Set download inactive", () =>
            {
                localGame.ProgressStatus = null;
                localGame.InvokeOnUpdate();
            }),
            new Command("Set download to 50%", () =>
            {
                localGame.ProgressStatus.Percentage = 50;
                localGame.ProgressStatus.InvokeOnUpdate();
            }),
            new Command("Set download to 100%", () =>
            {
                localGame.ProgressStatus.Percentage = 100;
                localGame.ProgressStatus.InvokeOnUpdate();
            }),
            new Command("Edit", () => EditGameForm(game)),
        };
    }
}