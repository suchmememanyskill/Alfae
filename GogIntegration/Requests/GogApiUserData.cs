using Newtonsoft.Json;

namespace GogIntegration.Requests;

public class GogApiUserData
{
    [JsonProperty("IsLoggedIn")]
    public bool IsLoggedIn { get; set; }
    
    [JsonProperty("username")]
    public string Username { get; set; }

    public static async Task<GogApiUserData?> Get(GogApiAuth auth)
    {
        return await GogApiRequest.Get<GogApiUserData>(auth, "https://embed.gog.com/userData.json");
    }
}