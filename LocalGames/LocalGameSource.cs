using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;

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
        
        _app.ShowForm(entries);
    }

    public void Log(string message, LogType type = LogType.Info) => _app.Logger.Log(message, type, "LocalGames");

    public Task<IGame> GetGames()
    {
        throw new NotImplementedException();
    }

    public Task CustomCommand(string command, IGame? game)
    {
        throw new NotImplementedException();
    }

    public Task Start(IGame game)
    {
        throw new NotImplementedException();
    }
}