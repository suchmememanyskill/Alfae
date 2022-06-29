using Newtonsoft.Json;

namespace ItchIoIntegration.Requests;

public class ItchApiProfileOwnedKeys
{
    [JsonProperty("owned_keys")]
    public List<ItchApiOwnedGameKey> OwnedKeys { get; set; }

    [JsonProperty("page")]
    public long Page { get; set; }

    [JsonProperty("per_page")]
    public long PerPage { get; set; }

    public ItchApiProfile? Profile { get; private set; }
    
    public async static Task<ItchApiProfileOwnedKeys?> Get(ItchApiProfile profile, int page = 1)
    {
        using (HttpClient client = new())
        {
            client.DefaultRequestHeaders.Add("Authorization", profile.ApiKey);
            HttpResponseMessage response = await client.GetAsync($"https://api.itch.io/profile/owned-keys?page={page}");
            if (!response.IsSuccessStatusCode)
                return null;

            string text = await response.Content.ReadAsStringAsync();
            try
            {
                var p = JsonConvert.DeserializeObject<ItchApiProfileOwnedKeys>(text);
                if (p != null)
                {
                    p.Profile = profile;
                }

                return p;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}

public class ItchApiOwnedGameKey
{
    [JsonProperty("purchase_id")]
    public long PurchaseId { get; set; }

    [JsonProperty("game_id")]
    public long GameId { get; set; }

    [JsonProperty("id")]
    public long DownloadKeyId { get; set; }

    [JsonProperty("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonProperty("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonProperty("downloads")]
    public long Downloads { get; set; }

    [JsonProperty("game")]
    public ItchApiGame Game { get; set; }
}

public class ItchApiGame
{
    [JsonProperty("cover_url")]
    public Uri? CoverUrl { get; set; }

    [JsonProperty("user")]
    public ItchApiUser User { get; set; }

    [JsonProperty("short_text", NullValueHandling = NullValueHandling.Ignore)]
    public string ShortText { get; set; }

    [JsonProperty("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonProperty("classification")]
    public string Classification { get; set; }

    [JsonProperty("min_price")]
    public long MinPrice { get; set; }

    [JsonProperty("url")]
    public Uri Url { get; set; }

    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("published_at")]
    public DateTimeOffset PublishedAt { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }
    
    [JsonProperty("still_cover_url", NullValueHandling = NullValueHandling.Ignore)]
    public Uri? StillCoverUrl { get; set; }
    
    public Uri? GetCoverUrl()
    {
        if (StillCoverUrl != null)
            return StillCoverUrl;

        if (CoverUrl != null)
            return CoverUrl;

        return null;
    }
}

public class ItchApiUser
{
    [JsonProperty("cover_url", NullValueHandling = NullValueHandling.Ignore)]
    public Uri CoverUrl { get; set; }

    [JsonProperty("url")]
    public Uri Url { get; set; }

    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("display_name", NullValueHandling = NullValueHandling.Ignore)]
    public string DisplayName { get; set; }
}