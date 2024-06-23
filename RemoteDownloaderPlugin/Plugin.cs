using System.Net.Http.Headers;
using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using Newtonsoft.Json;
using RemoteDownloaderPlugin.Game;
using RemoteDownloaderPlugin.Gui;

namespace RemoteDownloaderPlugin;

public class Plugin : IGameSource
{
    public string ServiceName => "Remote Games Integration";
    public string Version => "v1.0.0";
    public string SlugServiceName => "remote-games";
    public string ShortServiceName => "Remote";
    public PluginType Type => PluginType.GameSource;
    
    public Storage<Store> Storage { get; private set; }
    public IApp App { get; private set; }

    private Remote _cachedRemote = new();
    private List<OnlineGame> _onlineGames = new();
    private List<string> _platforms = new();
    
    public async Task<InitResult?> Initialize(IApp app)
    {
        App = app;
        Storage = new(app, "remote_games.json");
        Storage.Data.Migrate();
        await FetchRemote();
        
        return null;
    }

    public List<Command> GetGameCommands(IGame game)
    {
        if (game is OnlineGame onlineGame)
        {
            if (onlineGame.ProgressStatus != null)
            {
                return new()
                {
                    new Command("Stop", () => onlineGame.Stop())
                };
            }
            else
            {
                return new()
                {
                    new Command("Install", () => onlineGame.Download())
                };
            }
        }

        if (game is InstalledGame installedGame)
        {
            var commands = new List<Command>()
            {
                new Command(game.IsRunning ? "Running" : "Launch", installedGame.Play),
                new Command("Open in File Manager", installedGame.OpenInFileManager),
                new Command($"Version: {installedGame.Game.Version}"),
                new Command($"Platform: {installedGame.Game.Platform}"),
            };

            if (installedGame.IsEmu)
            {
                commands.Add(new Command(installedGame.InstalledContentTypes.ToString()));
            }
            
            commands.Add(new Command("Uninstall", () =>         
                App.Show2ButtonTextPrompt($"Do you want to uninstall {game.Name}?", "Yes", "No", _ =>
                {
                    installedGame.Delete();
                    App.HideForm();
                }, _ => App.HideForm(), game)));

            return commands;
        }

        throw new NotImplementedException();
    }
    
    public async Task<List<IGame>> GetGames()
    {
        var installedGames = Storage.Data.Games.Select(x => new InstalledGame(x, this)).ToList();
        var installedIds = new HashSet<string>(installedGames.Select(x => x.InternalName));
        
        return _onlineGames.Where(x => !(installedIds.Contains(x.InternalName) || Storage.Data.HiddenRemotePlatforms.Contains(x.Game.Platform)))
            .Select(x => (IGame)x)
            .Concat(installedGames.Select(x => (IGame)x))
            .ToList();
    }
    
    public async Task<bool> FetchRemote()
    {
        if (string.IsNullOrEmpty(Storage.Data.IndexUrl))
            return false;

        try
        {
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Authorization = Storage.Data.GetAuth();
            
            var data = await client.GetStringAsync(Storage.Data.IndexUrl);
            _cachedRemote = JsonConvert.DeserializeObject<Remote>(data)!;

            _onlineGames = _cachedRemote.Games.Select(x => new OnlineGame(x, this)).ToList();
            _platforms = _onlineGames.Select(x => x.Game.Platform).Distinct().ToList();
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public List<Command> GetGlobalCommands()
    {
        List<Command> commands = new()
        {
            new Command("Reload", Reload),
            new Command(),
            new Command("Edit Index URL", () => new SettingsRemoteIndexGui(App, this).ShowGui()),
            new Command("Emulation Profiles", 
                Storage.Data.EmuProfiles.Select(
                    x => new Command($"Edit {x.Platform}", new AddOrEditEmuProfileGui(App, this, x).ShowGui))
                    .Concat(new List<Command>()
                    {
                        new(),
                        new Command("Add Emulation Profile", () => new AddOrEditEmuProfileGui(App, this).ShowGui()),
                    }).ToList()),
            new Command("Hide Platforms", _platforms.Select(x => 
                new Command(Storage.Data.HiddenRemotePlatforms.Contains(x) ? $"Show {x}" : $"Hide {x}", () =>
                {
                    if (!Storage.Data.HiddenRemotePlatforms.Remove(x))
                    {
                        Storage.Data.HiddenRemotePlatforms.Add(x);
                    }

                    Storage.Save();
                    App.ReloadGames();
                })).ToList()),
            new Command(),
            new Command("Server setup", () => LauncherGamePlugin.Utils.OpenUrl("https://github.com/suchmememanyskill/Alfae-Server"))
        };
        
        return commands;
    }
    
    private async void Reload()
    {
        App.ShowTextPrompt("Reloading Remote Games...");
        await FetchRemote();
        App.ReloadGames();
        App.HideForm();
    }
}