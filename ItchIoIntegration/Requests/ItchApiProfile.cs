using Newtonsoft.Json;

namespace ItchIoIntegration.Requests;

public class ItchApiProfile
{
    [JsonProperty("user")] 
    public ItchApiUserProfile User { get; set; }
    
    [JsonIgnore]
    public string ApiKey { get; private set; }

    public Task<ItchApiProfileOwnedKeys?> GetOwnedKeys(int page = 1) => ItchApiProfileOwnedKeys.Get(this, page);

    public async static Task<ItchApiProfile?> Get(string apiKey)
    {
        ItchApiProfile? p = await ItchApiRequest.ItchRequest<ItchApiProfile>(apiKey, "https://api.itch.io/profile");

        if (p != null)
            p.ApiKey = apiKey;

        return p;
    }
}

public class ItchApiUserProfile
{
    [JsonProperty("username")]
    public string Username { get; set; }
    
    [JsonProperty("id")]
    public long Id { get; set; }
}