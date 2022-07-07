using LauncherGamePlugin.Enums;
using Newtonsoft.Json;

namespace GogIntegration.Requests;

public class GogApiGames
{
    [JsonProperty("page")]
    public int Page { get; set; }
    
    [JsonProperty("totalPages")]
    public int TotalPages { get; set; }
    
    [JsonProperty("products")]
    public List<GogApiProduct> Products { get; set; }

    public static async Task<GogApiGames?> Get(GogApiAuth auth, int page = 1)
    {
        return await GogApiRequest.Get<GogApiGames>(auth,
            $"https://embed.gog.com/account/getFilteredProducts?mediaType=1&page={page}");
    }
}

public class GogApiProduct
{
    [JsonProperty("id")] 
    public long Id { get; set; }
    
    [JsonProperty("title")]
    public string Name { get; set; }
    
    [JsonProperty("image")]
    public string CoverUrl { get; set; }

    public string GetCoverUrl() => "http:" + CoverUrl + ".jpg";
    
    [JsonProperty("slug")]
    public string Slug { get; set; }

    public string GetPageUrl() => $"https://www.gog.com/en/game/{Slug}";
    
    [JsonProperty("worksOn")]
    public GogApiWorksOn Platforms { get; set; }
}

public class GogApiWorksOn
{
    [JsonProperty("Windows")]
    public bool Windows { get; set; }
    
    // Macos doesn't exist lol
    
    [JsonProperty("Linux")]
    public bool Linux { get; set; }

    public Platform GetIdealPlatform()
    {
        if (PlatformExtensions.CurrentPlatform == Platform.Windows)
        {
            if (Windows)
                return Platform.Windows;

            return Platform.None;
        }
        else
        {
            if (Linux)
                return Platform.Linux;

            if (Windows)
                return Platform.Windows;

            return Platform.None;
        }
    }
}