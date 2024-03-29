﻿using Newtonsoft.Json;

namespace ItchIoIntegration.Requests;

public static class ItchApiRequest
{
    private static readonly List<string> SkipErrors = new()
    {
        "launch_targets",
        "traits"
    };
    
    public static Task<T?> ItchRequest<T>(ItchApiProfile profile, string url) => ItchRequest<T>(profile.ApiKey, url);
    public static async Task<T?> ItchRequest<T>(string apiKey, string url)
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("Authorization", apiKey);


        var jsonSettings = new JsonSerializerSettings
        {
            Error = ((sender, args) =>
            {
                if (SkipErrors.Contains(args.ErrorContext.Member))
                    args.ErrorContext.Handled = true;
            })
        };
        
        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return default;
            
            string text = await response.Content.ReadAsStringAsync();
            
            var p = JsonConvert.DeserializeObject<T>(text, jsonSettings);
            return p;
        }
        catch (Exception e)
        {
            return default;
        }
    }
}