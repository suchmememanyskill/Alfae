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
                var settings = new JsonSerializerSettings
                {
                    Error = (obj, args) =>
                    {
                        var contextErrors = args.ErrorContext;
                        contextErrors.Handled = true;
                    }
                };
                var p = JsonConvert.DeserializeObject<ItchApiProfileOwnedKeys>(text, settings);
                if (p != null)
                {
                    p.Profile = profile;
                    p.OwnedKeys.ForEach(x => x.Owner = p);
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

    [JsonIgnore]
    public ItchApiProfileOwnedKeys Owner { get; set; }

    public Task<ItchApiGameUploads?> GetUploads() => ItchApiGameUploads.Get(Owner.Profile!, this);
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

    [JsonProperty("traits", Required = Required.AllowNull, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Traits { get; set; }

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

    public string? GetCoverUrlFilename()
    {
        Uri? uri = GetCoverUrl();
        if (uri == null)
            return null;

        return uri.AbsoluteUri.Split("/").Last();
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