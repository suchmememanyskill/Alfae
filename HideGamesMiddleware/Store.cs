using LauncherGamePlugin.Interfaces;

namespace HideGamesMiddleware;

public class Store
{
    public Dictionary<string, List<string>> Games { get; set; } = new();

    public bool IsHidden(IGame game)
        => Games.ContainsKey(game.Source.ServiceName) && Games[game.Source.ServiceName].Contains(game.Name);

    public void HideGame(IGame game)
    {
        string key = game.Source.ServiceName;
        string value = game.Name;
        if (!Games.ContainsKey(key))
            Games.Add(key, new());

        List<string> items = Games[key];

        if (!items.Contains(value))
            items.Add(value);
    }

    public void ShowGame(string key, string name)
    {
        if (Games.ContainsKey(key) && Games[key].Contains(name))
            Games[key].Remove(name);
    }
}