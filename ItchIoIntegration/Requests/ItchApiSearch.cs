using Newtonsoft.Json;

namespace ItchIoIntegration.Requests;

public class ItchApiSearch
{
    [JsonProperty("games")]
    public List<ItchApiGame> Games { get; set; }

    [JsonProperty("page")]
    public long Page { get; set; }

    [JsonProperty("per_page")]
    public long PerPage { get; set; }
    
    public static async Task<ItchApiSearch?> Get(ItchApiProfile profile, string query)
    {
        string url = $"https://api.itch.io/search/games?query={query}";
        return await ItchApiRequest.ItchRequest<ItchApiSearch>(profile, url);
    }
}
