using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Launcher.Utils;

public static class NewVersionCheck
{
    private static Version AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version!;
    public static string Version => $"{AssemblyVersion.Major}.{AssemblyVersion.Minor}.{AssemblyVersion.Build}";

    public static async Task<string?> GetGitVersion()
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.Add(new("suchmememanyskill_Launcher", Version));
        HttpResponseMessage response = await client.GetAsync("https://api.github.com/repos/suchmememanyskill/Launcher/releases/latest");
        if (!response.IsSuccessStatusCode)
            return null;

        string text = await response.Content.ReadAsStringAsync();
        try
        {
            GithubResponse? p = JsonConvert.DeserializeObject<GithubResponse>(text);
            
            return p.TagName;
        }
        catch
        {
            return null;
        }
    }
}

public class GithubResponse
{
    [JsonProperty("tag_name")]
    public string TagName { get; set; }
}