using LauncherGamePlugin.Interfaces;

namespace SteamGridDbMiddleware.Model;

public class Store
{
    public List<Override> Covers { get; set; } = new();
    public List<Override> Backgrounds { get; set; } = new();
    public string ApiKey { get; set; } = "";
    
    private Override? GetOverride(List<Override> overrides, IGame game)
        => overrides.Find(x => x.GameName == game.Name && x.GameSource == game.Source.ServiceName);

    public Override? GetCover(IGame game) => GetOverride(Covers, game);
    public Override? GetBackground(IGame game) => GetOverride(Backgrounds, game);
    public bool HasCover(IGame game) => GetCover(game) != null;
    public bool HasBackground(IGame game) => GetBackground(game) != null;
}