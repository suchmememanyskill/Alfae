using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;

namespace SteamGridDbMiddleware.Model;

public class GameOverride : IGame
{
    private IGame _game;
    private SteamGridDb _instance;

    public GameOverride(IGame game, SteamGridDb instance)
    {
        _game = game;
        _instance = instance;
        _game.OnUpdate += () => OnUpdate?.Invoke();
    }

    public IGame Original => _game;
    public string Name => _game.Name;
    public bool IsRunning
    {
        get => _game.IsRunning;
        set => _game.IsRunning = value;
    }

    public IGameSource Source => _game.Source;
    public long? Size => _game.Size;
    public async Task<byte[]?> CoverImage()
    {
        if (_instance.Storage.Data.HasCover(_game))
        {
            Override x = _instance.Storage.Data.GetCover(_game)!;
            return await Storage.Cache($"steamgriddb_cover_{x.Id}.jpg", () => Storage.ImageDownload(x.Url));
        }
        else
        {
            return await _game.CoverImage();
        }
    }

    public async Task<byte[]?> BackgroundImage()
    {
        if (_instance.Storage.Data.HasBackground(_game))
        {
            Override x = _instance.Storage.Data.GetBackground(_game)!;
            return await Storage.Cache($"steamgriddb_bg_{x.Id}.jpg", () => Storage.ImageDownload(x.Url));
        }
        else
        {
            return await _game.BackgroundImage();   
        }
    }

    public InstalledStatus InstalledStatus => _game.InstalledStatus;
    public Platform EstimatedGamePlatform => _game.EstimatedGamePlatform;
    public ProgressStatus? ProgressStatus => _game.ProgressStatus;
    public event Action? OnUpdate;
    public void InvokeOnUpdate() => OnUpdate?.Invoke();
}