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

    public void AddGameForm(string possibleWarn = "", string gameName = "", string execPath = "")
    {
        List<FormEntry> entries = new()
        {
            new FormEntry(FormEntryType.TextBox, "Add a local game", "Bold"),
            new FormEntry(FormEntryType.TextInput, "Game name:", gameName),
            new FormEntry(FormEntryType.FilePicker, "Game executable:", execPath),
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
        string gameName = form.FormEntries.Find(x => x.Name == "Game name:").Value;
        string execPath = form.FormEntries.Find(x => x.Name == "Game executable:").Value;

        if (string.IsNullOrWhiteSpace(gameName))
        {
            AddGameForm("Please fill in the game name", gameName, execPath);
            return;
        }

        if (string.IsNullOrWhiteSpace(execPath))
        {
            AddGameForm("Please fill in the executable path", gameName, execPath);
            return;
        }

        if (!File.Exists(execPath))
        {
            AddGameForm("Executable path does not exist!", gameName, execPath);
            return;
        }
        
        Log($"Calculating game {gameName} size at path {execPath}");
        LocalGame localGame = new LocalGame();
        localGame.Name = gameName;
        localGame.ExecPath = execPath;
        localGame.Size = Utils.DirSize(new DirectoryInfo(localGame.InstalledPath));
        Log($"{gameName}'s size is {localGame.ReadableSize()}");
        Games.Add(localGame);
        Log($"Added game {gameName}");
        _app.ReloadGames();
    }

    public void Log(string message, LogType type = LogType.Info) => _app.Logger.Log(message, type, "LocalGames");

    public async Task<List<IGame>> GetGames()
    {
        return Games.Select(x => (IGame)x).ToList();
    }

    public Task CustomCommand(string command, IGame? game)
    {
        throw new NotImplementedException();
    }

    public List<Command> GetGameCommands(IGame game)
    {
        //if ()
        return null;
    }
}