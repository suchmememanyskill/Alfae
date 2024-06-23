using System.Diagnostics;
using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Extensions;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;

namespace RemoteDownloaderPlugin.Game;

public class OnlineGame : IGame
{
    public string InternalName { get; }
    public string Name => $"{Game.Name} ({Game.Platform})";
    public bool IsRunning { get; set; } = false;
    public IGameSource Source => _plugin;
    public long? Size => Game.GameSize;

    public bool HasImage(ImageType type)
        => ImageTypeToUri(type) != null;
    
    public Task<byte[]?> GetImage(ImageType type)
    {
        Uri? url = ImageTypeToUri(type);

        if (url == null)
            return Task.FromResult<byte[]?>(null);

        return Storage.Cache($"{Game.Id}_{type}.jpg", () => Storage.ImageDownload(url));
    }

    public InstalledStatus InstalledStatus => InstalledStatus.NotInstalled;
    public Platform EstimatedGamePlatform => LauncherGamePlugin.Enums.Platform.Unknown;
    public ProgressStatus? ProgressStatus => _download;
    public event Action? OnUpdate;
    public void InvokeOnUpdate() => OnUpdate?.Invoke();

    public OnlineGameDownload Game { get; }
    private Plugin _plugin;
    private GameDownload? _download = null;

    public OnlineGame(OnlineGameDownload game, Plugin plugin)
    {
        Game = game;
        _plugin = plugin;
        InternalName = $"{Game.Id}_{LauncherGamePlugin.Utils.OnlyLetters(Game.Platform).ToLower()}";
    }

    public async Task Download()
    {
        var baseFiles = Game.Files.Where(x => x.Type == DownloadType.Base).ToList();
        if (baseFiles.Count >= 2)
        {
            var form = new Form(new());
            
            form.FormEntries.Add(Form.TextBox("Pick a base edition of the game:", FormAlignment.Center, "Bold"));
            
            baseFiles.ForEach(x => form.FormEntries.Add(Form.ClickableLinkBox($"{x.Name}: {x.DownloadSize.ReadableSize()}", _ =>
            {
                Game.Files.Where(y => y.Type == DownloadType.Base && y.Name != x.Name).ToList().ForEach(y => Game.Files.Remove(y));
                Download();
                _plugin.App.HideForm();
            }, FormAlignment.Left)));
            
            form.FormEntries.Add(Form.Button("Back", _ => _plugin.App.HideForm()));
            
            _plugin.App.ShowForm(form);
            return;
        }
        
        _download = new GameDownload(Game, _plugin.Storage.Data.GetAuth());
        OnUpdate?.Invoke();
        
        try
        {
            await _download.Download(_plugin.App);
        }
        catch
        {
            _download = null;
            OnUpdate?.Invoke();
            return;
        }

        var installedContent = new InstalledGameContent()
        {
            Id = Game.Id,
            Name = Game.Name,
            Platform = Game.Platform,
            GameSize = _download.TotalSize,
            Version = _download.Version,
            Filename = _download.Filename,
            Images = Game.Images,
            InstalledContent = _download.InstalledEntries,
            BasePath = _download.BasePath
        };
        
        var gamePath = Path.Join(installedContent.BasePath, installedContent.Filename);
        var gameDir = Path.Join(installedContent.BasePath, installedContent.Id);
        
        _plugin.Storage.Data.Games.Add(installedContent);
        
        _plugin.Storage.Save();
        
        var emuProfile = _plugin.Storage.Data.EmuProfiles.FirstOrDefault(x => x.Platform == Game.Platform);

        if (emuProfile != null && !string.IsNullOrWhiteSpace(emuProfile.PostInstallScriptPath) &&
            File.Exists(emuProfile.PostInstallScriptPath))
        {
            var args = emuProfile.PostInstallScriptArgs;

            if (string.IsNullOrWhiteSpace(args))
            {
                args = string.Empty;
            }
            else
            {
                args = args.Replace("{EXEC}", $"\"{gamePath}\"").Replace("{DIR}", $"\"{gameDir}\"");
            }

            var workDir = emuProfile.PostInstallScriptWorkingDirectory;

            if (string.IsNullOrWhiteSpace(workDir) || !Directory.Exists(workDir))
            {
                workDir = Path.GetDirectoryName(emuProfile.PostInstallScriptPath);
            }

            _download.Line1 = "Running post install script...";
            _download.InvokeOnUpdate();
            Process process = new();
            
            process.StartInfo.FileName = emuProfile.PostInstallScriptPath;
            process.StartInfo.WorkingDirectory = workDir;
            process.StartInfo.Arguments = args;
            process.Start();
            await process.WaitForExitAsync();
        }
        
        _plugin.App.ReloadGames();
        _download = null;
        OnUpdate?.Invoke();
    }

    public void Stop()
    {
        _download?.Stop();
        _download = null;
    }
    
    private Uri? ImageTypeToUri(ImageType type)
        => type switch
        {
            ImageType.Background => Game.Images.Background,
            ImageType.VerticalCover => Game.Images.VerticalCover,
            ImageType.HorizontalCover => Game.Images.HorizontalCover,
            ImageType.Logo => Game.Images.Logo,
            ImageType.Icon => Game.Images.Icon,
            _ => null
        };
}