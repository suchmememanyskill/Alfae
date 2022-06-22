using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LegendaryIntegration.Service;

namespace LegendaryIntegration;

public class LegendaryGameSource : IGameSource
{
    public string ServiceName => "Epic Games Integration";
    public string Description => "Uses legendary for epic games integration";
    public string Version => "v0.1";
    public string SlugServiceName => "epic-games";
    public string ShortServiceName => "EpicGames";
    public LegendaryAuth? auth;
    public LegendaryGameManager? manager;
    public static LegendaryGameSource Source { get; private set; }
    public IApp App { get; private set; }
    
    public async Task Initialize(IApp app)
    {
        App = app;
        Source = this;

        auth = new();

        if (!await auth.AttemptLogin())
            auth = null;
    }

    public async Task<List<IGame>> GetGames()
    {
        if (auth == null)
            return new();

        manager ??= new(auth);

        return (await manager.GetGames()).Select(x => (IGame) x).ToList();
    }

    public Task CustomCommand(string command, IGame? game)
    {
        throw new NotImplementedException();
    }

    public List<Command> GetGameCommands(IGame game)
    {
        LegendaryGame legendaryGame = game as LegendaryGame;
        if (legendaryGame == null)
            throw new InvalidDataException();
        
        List<Command> commands = new()
        {
            new Command("NotImplemented", () => { }),
        };

        if (!legendaryGame.IsInstalled && legendaryGame.Size == 0)
        {
            commands.Add(new("Get game install size", () => GetOfflineGameSize(legendaryGame)));
        }

        if (!legendaryGame.IsInstalled)
        {
            commands.Add(new("Install", () => legendaryGame.StartDownload()));
        }

        if (legendaryGame.Download != null)
        {
            commands = new();
            if (legendaryGame.Download.Active)
                commands.Add(new("Pause", legendaryGame.Download.Pause));
            else
                commands.Add(new ("Continue", legendaryGame.Download.Start));
            
            commands.Add(new("Stop", legendaryGame.Download.Stop));
        }
        
        return commands;
    }

    private async Task GetOfflineGameSize(LegendaryGame game)
    {
        await game.GetInfo();
        game.InvokeOnUpdate();
    }

    public List<Command> GetGlobalCommands()
    {
        List<Command> commands = new List<Command>();
        
        if (auth == null)
            commands.Add(new("Not logged in"));
        else
        {
            commands.Add(new($"Logged in as {auth.StatusResponse.AccountName}"));
            if (auth.OfflineLogin)
                commands.Add(new("Started in offline mode"));
        }
        
        commands.Add(new());
        
        if (auth == null)
            commands.Add(new("Login", () => LoginForm()));
        else
            commands.Add(new("Logout", () => Logout()));

        return commands;
    }

    public async Task Login(Form form)
    {
        string? SID = form.GetValue("SID:");
        App.ShowForm(new(new()
        {
            new FormEntry(FormEntryType.TextBox, "Logging in...", alignment: FormAlignment.Center)
        }));

        if (string.IsNullOrWhiteSpace(SID))
        {
            LoginForm("SID field was left blank");
            return;
        }

        auth = new();
        if (!await auth.Authenticate(SID))
        {
            LoginForm("Legendary is seemingly not installed");
            auth = null;
            return;
        }

        if (!await auth.AttemptLogin())
        {
            LoginForm("Login failed");
            auth = null;
            return;
        }
        
        App.ReloadGames();
        App.HideOverlay();
    }

    private void LoginForm(string warningMessage = "")
    {
        List<FormEntry> entries = new()
        {
            new FormEntry(FormEntryType.TextBox, "Log into Epic Games", "Bold", alignment: FormAlignment.Center),
            new FormEntry(FormEntryType.TextBox, "Click the link, log in, and copy the SID value into the field below"),
            new FormEntry(FormEntryType.ClickableLinkBox,
                "https://www.epicgames.com/id/login?redirectUrl=https://www.epicgames.com/id/api/redirect", linkClick: entry => Utils.OpenUrl(entry.Name)),
            new FormEntry(FormEntryType.TextInput, "SID:"),
            new FormEntry(FormEntryType.ButtonList, "", buttonList: new()
            {
                {"Back", entry => App.HideOverlay() },
                {"Login", entry =>
                {
                    App.HideOverlay();
                    Login(entry.ContainingForm);
                }}
            })
        };
        
        if (warningMessage != "")
            entries.Add(new FormEntry(FormEntryType.TextBox, warningMessage, "Bold"));
        
        App.ShowForm(new(entries));
    }

    public async Task Logout()
    {
        if (auth == null)
            return;
        
        App.ShowForm(new(new()
        {
            new FormEntry(FormEntryType.TextBox, "Logging out...", alignment: FormAlignment.Center)
        }));

        if (manager != null)
        {
            manager.StopAllDownloads();
        }
        
        await auth.Logout();
        auth = null;
        manager = null;
        App.ReloadGames();
        App.HideOverlay();
    }
    
    public void Log(string message, LogType type = LogType.Info) => App.Logger.Log(message, type, "EpicGames");
}