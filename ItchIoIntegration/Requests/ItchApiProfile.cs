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
        using (HttpClient client = new())
        {
            client.DefaultRequestHeaders.Add("Authorization", apiKey);
            HttpResponseMessage response = await client.GetAsync("https://api.itch.io/profile");
            if (!response.IsSuccessStatusCode)
                return null;

            string text = await response.Content.ReadAsStringAsync();
            try
            {
                ItchApiProfile? p = JsonConvert.DeserializeObject<ItchApiProfile>(text);

                if (p != null)
                    p.ApiKey = apiKey;

                return p;
            }
            catch
            {
                return null;
            }
        }
    }
}

public class ItchApiUserProfile
{
    [JsonProperty("username")]
    public string Username { get; set; }
    
    [JsonProperty("id")]
    public long Id { get; set; }
}