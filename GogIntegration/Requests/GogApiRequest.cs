using Newtonsoft.Json;

namespace GogIntegration.Requests;

public static class GogApiRequest
{
    public static async Task<T?> Get<T>(GogApiAuth auth, string url)
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {auth.AccessToken}");
        client.DefaultRequestHeaders.Add("User-Agent", "GOGGalaxyClient/2.0.45.61 (GOG Galaxy)");
        HttpResponseMessage response = await client.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
            return default;

        string text = await response.Content.ReadAsStringAsync();
        try
        {
            var p = JsonConvert.DeserializeObject<T>(text);
            return p;
        }
        catch (Exception e)
        {
            return default;
        }
    }
}