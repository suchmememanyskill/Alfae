using Newtonsoft.Json;

namespace GogIntegration.Requests;

public class GogApiAuth
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("expires_in")] 
    public int ExpiresIn { get; set; }
    
    [JsonProperty("token_type")]
    public string TokenType { get; set; }
    
    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
    
    [JsonProperty("requested_time", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset RequestedTime { get; set; } = DateTime.Now;
    
    public static async Task<GogApiAuth?> Get(string code)
    {
        try
        {
            using HttpClient client = new();
            var response = await client.GetAsync(
                $"https://auth.gog.com/token?client_id=46899977096215655&client_secret=9d85c43b1482497dbbce61f6e4aa173a433796eeae2ca8c5f6129f2dc4de46d9&grant_type=authorization_code&redirect_uri=https%3A%2F%2Fembed.gog.com%2Fon_login_success%3Forigin%3Dclient&code={code}");
            
            response.EnsureSuccessStatusCode();
            string text = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GogApiAuth>(text);
        }
        catch
        {
            return null;
        }
    }

    public bool NeedsRefresh() => DateTime.Now > (RequestedTime + TimeSpan.FromSeconds(ExpiresIn));

    public async Task<GogApiAuth?> Refresh()
    {
        try
        {
            using HttpClient client = new();
            var response = await client.GetAsync(
                $"https://auth.gog.com/token?client_id=46899977096215655&client_secret=9d85c43b1482497dbbce61f6e4aa173a433796eeae2ca8c5f6129f2dc4de46d9&grant_type=refresh_token&refresh_token={RefreshToken}");
            
            response.EnsureSuccessStatusCode();
            string text = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GogApiAuth>(text);
        }
        catch
        {
            return null;
        }
    }
}