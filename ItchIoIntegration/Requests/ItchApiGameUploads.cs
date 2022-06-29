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
        using (HttpClient client = new())
        {
            client.DefaultRequestHeaders.Add("Authorization", profile.ApiKey);
            HttpResponseMessage response = await client.GetAsync($"https://api.itch.io/games/{game.Id}/uploads?download_key_id={game.DownloadKeyId}");
            if (!response.IsSuccessStatusCode)
                return null;

            string text = await response.Content.ReadAsStringAsync();
            try
            {
                ItchApiGameUploads? uploads = JsonConvert.DeserializeObject<ItchApiGameUploads>(text);

                if (uploads != null)
                {
                    uploads._profile = profile;
                }

                return uploads;
            }
            catch
            {
                return null;
            }
        }
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

    [JsonProperty("id")]
    public long Id { get; set; }

    public string GetDownloadUrl(long downloadKeyId, ItchApiProfile profile) =>
        $"https://api.itch.io/uploads/{Id}/download?download_key_id={downloadKeyId}&api_key={profile.ApiKey}";
}

