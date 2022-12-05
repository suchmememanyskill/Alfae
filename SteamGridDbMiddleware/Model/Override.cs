namespace SteamGridDbMiddleware.Model;

public class Override
{
    public int Id { get; set; }
    public string Url { get; set; }
    public string Author { get; set; }

    public Override()
    {
    }

    public Override(string url, int id, string author)
    {
        Url = url;
        Id = id;
        Author = author;
    }
}