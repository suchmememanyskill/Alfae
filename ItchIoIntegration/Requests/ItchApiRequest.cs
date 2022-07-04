using Newtonsoft.Json;

namespace ItchIoIntegration.Requests;

public static class ItchApiRequest
{
    public static Task<T?> ItchRequest<T>(ItchApiProfile profile, string url) => ItchRequest<T>(profile.ApiKey, url);
    public static async Task<T?> ItchRequest<T>(string apiKey, string url)
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("Authorization", apiKey);
        HttpResponseMessage response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return default;

        var jsonSettings = new JsonSerializerSettings
        {
            Error = ((sender, args) =>
            {
                if ("traits".Equals(args.ErrorContext.Member))
                    args.ErrorContext.Handled = true;
            })
        };
            
        string text = await response.Content.ReadAsStringAsync();
        try
        {
            var p = JsonConvert.DeserializeObject<T>(text, jsonSettings);
            return p;
        }
        catch (Exception e)
        {
            return default;
        }
    }
}