using LauncherGamePlugin.Interfaces;

namespace SteamGridDbMiddleware.Model;

public class Store
{
    public List<Override> Covers { get; set; } = new();
    public List<Override> Backgrounds { get; set; } = new();
    public string ApiKey { get; set; } = "";
    
    private Override? GetOverride(List<Override> overrides, IGame game)
        => overrides.Find(x => x.GameName == game.InternalName && x.GameSource == game.Source.ServiceName);

    public Override? GetCover(IGame game) => GetOverride(Covers, game);
    public Override? GetBackground(IGame game) => GetOverride(Backgrounds, game);
    public bool HasCover(IGame game) => GetCover(game) != null;
    public bool HasBackground(IGame game) => GetBackground(game) != null;
    
    public void ClearBackground(IGame game)
    {
        Override? x = GetBackground(game);
        if (x != null)
            Backgrounds.Remove(x);
    }

    public void ClearCover(IGame game)
    {
        Override? x = GetCover(game);
        if (x != null)
            Covers.Remove(x);
    }

    public void SetBackground(IGame game, string id, string url)
    {
        Override? x = GetBackground(game);
        x ??= new(game.InternalName, game.Source.ServiceName, "", "");

        x.Url = url;
        x.Id = id;

        if (!Backgrounds.Contains(x))
            Backgrounds.Add(x);
    }

    public void SetCover(IGame game, string id, string url)
    {
        Override? x = GetCover(game);
        x ??= new(game.InternalName, game.Source.ServiceName, "", "");

        x.Url = url;
        x.Id = id;

        if (!Covers.Contains(x))
            Covers.Add(x);
    }
}