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
    public string InternalName => _game.InternalName;
    public bool IsRunning
    {
        get => _game.IsRunning;
        set => _game.IsRunning = value;
    }

    public IGameSource Source => _game.Source;
    public long? Size => _game.Size;

    public bool HasImage(ImageType type) =>
        _instance.Storage.Data.GetOverride(this, type) != null || _game.HasImage(type);

    public async Task<byte[]?> GetImage(ImageType type)
    {
        Override? @override = _instance.Storage.Data.GetOverride(this, type);
        if (@override != null)
            return await Storage.Cache($"steamgriddb_{type.ToString()}_{@override.Id}.jpg",
                () => Storage.ImageDownload(@override.Url));

        return await _game.GetImage(type);
    }
    
    public InstalledStatus InstalledStatus => _game.InstalledStatus;
    public Platform EstimatedGamePlatform => _game.EstimatedGamePlatform;
    public ProgressStatus? ProgressStatus => _game.ProgressStatus;
    public event Action? OnUpdate;
    public void InvokeOnUpdate() => OnUpdate?.Invoke();
}