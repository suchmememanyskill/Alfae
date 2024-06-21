using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;

namespace RemoteDownloaderPlugin.Game;

public class InstalledGame : IGame
{
    public string InternalName { get; }
    public string Name => Game.Name;
    public bool IsRunning { get; set; } = false;
    public IGameSource Source => _plugin;
    public long? Size => Game.GameSize;
    public bool HasImage(ImageType type)
        => ImageTypeToUri(type) != null;

    public bool IsEmu => _type == GameType.Emu;

    public ContentTypes InstalledContentTypes => Game.InstalledContent;
    
    public Task<byte[]?> GetImage(ImageType type)
    {
        Uri? url = ImageTypeToUri(type);

        if (url == null)
            return Task.FromResult<byte[]?>(null);

        return Storage.Cache($"{Game.Id}_{type}.jpg", () => Storage.ImageDownload(url));
    }

    public InstalledStatus InstalledStatus => InstalledStatus.Installed;

    public Platform EstimatedGamePlatform => IsEmu
        ? LauncherGamePlugin.Utils.GuessPlatformBasedOnString(_plugin.Storage.Data.EmuProfiles.FirstOrDefault(x => x.Platform == Game.Platform)?.ExecPath)
        : LauncherGamePlugin.Utils.GuessPlatformBasedOnString(_pcLaunchDetails!.LaunchExec);
    
    public ProgressStatus? ProgressStatus => null;
    public event Action? OnUpdate;

    public void InvokeOnUpdate()
        => OnUpdate?.Invoke();

    public InstalledGameContent Game { get; }
    private Plugin _plugin;
    private PcLaunchDetails? _pcLaunchDetails;
    private GameType _type;
    
    public InstalledGame(InstalledGameContent game, Plugin plugin)
    {
        Game = game;
        _plugin = plugin;
        _type = game.Platform == "Pc" ? GameType.Pc : GameType.Emu;
        _pcLaunchDetails = null;
        InternalName = $"{Game.Id}_{LauncherGamePlugin.Utils.OnlyLetters(Game.Platform).ToLower()}";

        if (_type == GameType.Pc)
        {
            var fullPath = Path.Join(game.BasePath, "game.json");
            _pcLaunchDetails = PcLaunchDetails.GetFromPath(fullPath);
        }
    }

    public void Play()
    {
        try
        {
            if (IsEmu)
            {
                var emuProfile = _plugin.Storage.Data.EmuProfiles.FirstOrDefault(x => x.Platform == Game.Platform);

                if (emuProfile == null)
                {
                    throw new Exception($"No '{Game.Platform}' emulation profile exists");
                }

                var baseGamePath = Path.Join(Game.BasePath, Game.Filename);

                LaunchParams args = new(emuProfile.ExecPath,
                    emuProfile.CliArgs.Replace("{EXEC}", $"\"{baseGamePath}\""), emuProfile.WorkingDirectory, this,
                    EstimatedGamePlatform);
                _plugin.App.Launch(args);
            }
            else
            {
                var execPath = Path.Join(Game.BasePath, _pcLaunchDetails!.LaunchExec);
                var workingDir = Path.Join(Game.BasePath, _pcLaunchDetails!.WorkingDir);
                LaunchParams args = new(execPath, _pcLaunchDetails.LaunchArgs, Path.GetDirectoryName(workingDir)!, this,
                    EstimatedGamePlatform);
                _plugin.App.Launch(args);
            }
        }
        catch (Exception e)
        {
            _plugin.App.ShowDismissibleTextPrompt($"Game Failed to launch: {e.Message}");
        }
    }

    public void Delete()
    {
        bool success = false;
        if (IsEmu)
        {
            var baseGamePath = Path.Join(Game.BasePath, Game.Filename);
            var extraDir = Path.Join(Game.BasePath, Game.Id);
            
            try
            {
                File.Delete(baseGamePath);
                Directory.Delete(extraDir, true);
                success = true;
            }
            catch {}
        }
        else
        {
            try
            {
                Directory.Delete(Game.BasePath, true);
                success = true;
            }
            catch {}
        }
        
        var game = _plugin.Storage.Data.Games.Find(x => x.Id == Game.Id && x.Platform == Game.Platform);
        _plugin.Storage.Data.Games.Remove(game!);

        if (!success)
        {
            _plugin.App.ShowTextPrompt("Failed to delete files. Game has been unlinked");
        }
        
        _plugin.App.ReloadGames();
        _plugin.Storage.Save();
    }

    public void OpenInFileManager()
        => LauncherGamePlugin.Utils.OpenFolder(Game.BasePath);
    
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