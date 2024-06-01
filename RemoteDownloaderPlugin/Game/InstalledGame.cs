using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;

namespace RemoteDownloaderPlugin.Game;

public class InstalledGame : IGame
{
    public string InternalName => Game.Id;
    public string Name => Game.Name;
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

    public InstalledStatus InstalledStatus => InstalledStatus.Installed;

    public Platform EstimatedGamePlatform => (_type == GameType.Emu)
        ? LauncherGamePlugin.Utils.GuessPlatformBasedOnString(_plugin.Storage.Data.EmuProfiles.FirstOrDefault(x => x.Platform == _emuGame!.Emu)?.ExecPath)
        : LauncherGamePlugin.Utils.GuessPlatformBasedOnString(_pcLaunchDetails!.LaunchExec);

    public string GamePlatform => (_type == GameType.Emu)
        ? _emuGame!.Emu
        : "Pc";

    public ProgressStatus? ProgressStatus => null;
    public event Action? OnUpdate;

    public void InvokeOnUpdate()
        => OnUpdate?.Invoke();

    public IInstalledGame Game { get; }
    private Plugin _plugin;
    private PcLaunchDetails? _pcLaunchDetails;
    private InstalledEmuGame? _emuGame;
    private GameType _type;
    
    public InstalledGame(IInstalledGame game, Plugin plugin)
    {
        Game = game;
        _plugin = plugin;
        _type = game is InstalledEmuGame ? GameType.Emu : GameType.Pc;
        _pcLaunchDetails = null;

        if (_type == GameType.Pc)
        {
            var fullPath = Path.Join(plugin.App.GameDir, "Remote", "Pc", Game.Id, "game.json");
            _pcLaunchDetails = PcLaunchDetails.GetFromPath(fullPath);
        }
        else
        {
            _emuGame = (game as InstalledEmuGame)!;
        }
    }

    public void Play()
    {
        try
        {
            if (_type == GameType.Emu)
            {
                var emuProfile = _plugin.Storage.Data.EmuProfiles.FirstOrDefault(x => x.Platform == _emuGame!.Emu);

                if (emuProfile == null)
                {
                    throw new Exception($"No '{_emuGame!.Emu}' emulation profile exists");
                }

                var baseGamePath = Path.Join(_plugin.App.GameDir, "Remote", _emuGame!.Emu, _emuGame.BaseFilename);

                LaunchParams args = new(emuProfile.ExecPath,
                    emuProfile.CliArgs.Replace("{EXEC}", $"\"{baseGamePath}\""), emuProfile.WorkingDirectory, this,
                    EstimatedGamePlatform);
                _plugin.App.Launch(args);
            }
            else
            {
                var execPath = Path.Join(_plugin.App.GameDir, "Remote", "Pc", Game.Id, _pcLaunchDetails!.LaunchExec);
                var workingDir = Path.Join(_plugin.App.GameDir, "Remote", "Pc", Game.Id, _pcLaunchDetails!.WorkingDir);
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
        if (_type == GameType.Emu)
        {
            var baseGamePath = Path.Join(_plugin.App.GameDir, "Remote", _emuGame!.Emu, _emuGame.BaseFilename);
            var extraDir = Path.Join(_plugin.App.GameDir, "Remote", _emuGame!.Emu, Game.Id);
            var success = false;

            try
            {
                File.Delete(baseGamePath);
                Directory.Delete(extraDir, true);
                success = true;
            }
            catch {}

            var game = _plugin.Storage.Data.EmuGames.Find(x => x.Id == Game.Id);
            _plugin.Storage.Data.EmuGames.Remove(game!);
            
            if (!success)
            {
                _plugin.App.ShowTextPrompt("Failed to delete files. Game has been unlinked");
            }
        }
        else
        {
            var path = Path.Join(_plugin.App.GameDir, "Remote", "Pc", Game.Id);
            var success = false;
            
            try
            {
                Directory.Delete(path, true);
                success = true;
            }
            catch {}

            var game = _plugin.Storage.Data.PcGames.Find(x => x.Id == Game.Id);
            _plugin.Storage.Data.PcGames.Remove(game!);

            if (!success)
            {
                _plugin.App.ShowTextPrompt("Failed to delete files. Game has been unlinked");
            }
        }
        
        _plugin.App.ReloadGames();
        _plugin.Storage.Save();
    }

    public void OpenInFileManager()
    {
        LauncherGamePlugin.Utils.OpenFolder(_type == GameType.Emu
            ? Path.Join(_plugin.App.GameDir, "Remote", _emuGame!.Emu)
            : Path.Join(_plugin.App.GameDir, "Remote", "Pc", Game.Id));
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