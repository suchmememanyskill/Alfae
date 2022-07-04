using ItchIoIntegration.Service;
using Newtonsoft.Json;

namespace ItchIoIntegration.Requests;

public class ItchApiGameUploads
{
    [JsonProperty("uploads")]
    public List<ItchApiUpload> Uploads { get; set; }

    private ItchApiProfile? _profile;

    public async static Task<ItchApiGameUploads?> Get(ItchApiProfile profile, ItchGame game)
    {
        string url = $"https://api.itch.io/games/{game.Id}/uploads";
        if (game.DownloadKeyId != null)
            url += $"?download_key_id={game.DownloadKeyId}";

        var uploads = await ItchApiRequest.ItchRequest<ItchApiGameUploads>(profile, url);
        
        if (uploads != null)
            uploads._profile = profile;

        return uploads;
    }
}

public class ItchApiUpload
{
    [JsonProperty("game_id")]
    public long GameId { get; set; }

    [JsonProperty("size")]
    public long Size { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("md5_hash")]
    public string Md5Hash { get; set; }

    [JsonProperty("filename")]
    public string Filename { get; set; }

    [JsonProperty("storage")]
    public string Storage { get; set; }

    [JsonProperty("position")]
    public long Position { get; set; }

    [JsonProperty("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonProperty("display_name")]
    public string DisplayName { get; set; }

    [JsonProperty("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
    
    [JsonProperty("traits")]
    public List<string> Traits { get; set; }

    [JsonProperty("id")]
    public long Id { get; set; }

    public bool IsDemo() => Traits?.Contains("demo") ?? false;

    public string GetDownloadUrl(long? downloadKeyId, ItchApiProfile profile)
    {
        string url = $"https://api.itch.io/uploads/{Id}/download?api_key={profile.ApiKey}";
        if (downloadKeyId != null)
            url += $"&download_key_id={downloadKeyId}";
        return url;
    }
        
}

