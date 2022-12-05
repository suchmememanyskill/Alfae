using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;

namespace SteamGridDbMiddleware.Model;

public class Store
{
    // Dictionary<"GameSource:InternalGameName", Dictionary<ImageType, {Id, Url}>>
    public Dictionary<string, Dictionary<ImageType, Override>> Overrides { get; set; } = new();
    public string ApiKey { get; set; } = "";

    public Override? GetOverride(IGame game, ImageType type)
    {
        string key = $"{game.Source.ShortServiceName}:{game.InternalName}";

        if (!Overrides.ContainsKey(key))
            return null;

        if (!Overrides[key].ContainsKey(type))
            return null;

        return Overrides[key][type];
    }

    public void SetOverride(IGame game, ImageType type, Override? @override)
    {
        string key = $"{game.Source.ShortServiceName}:{game.InternalName}";
        
        if (!Overrides.ContainsKey(key))
            Overrides.Add(key, new());

        if (@override == null)
            Overrides[key].Remove(type);
        else
            Overrides[key][type] = @override;
    }
}