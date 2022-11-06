namespace SteamGridDbMiddleware.Model;

public class Override
{
    public string GameName { get; set; }
    public string GameSource { get; set; }
    public string Url { get; set; }
    public string Id { get; set; }

    public Override()
    {
    }

    public Override(string gameName, string gameSource, string url, string id)
    {
        GameName = gameName;
        GameSource = gameSource;
        Url = url;
        Id = id;
    }
}