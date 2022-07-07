using GogIntegration.Extensions;
using GogIntegration.Requests;
using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;
using Newtonsoft.Json;

namespace GogIntegration;

public class GogDlImport
{
    [JsonProperty("appName")]
    public string AppName { get; set; }

    [JsonProperty("buildId")]
    public string BuildId { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("tasks")]
    public List<GogDlTask> Tasks { get; set; }

    [JsonProperty("installedLanguage")]
    public string InstalledLanguage { get; set; }

    [JsonProperty("installedWithDlcs")]
    public bool InstalledWithDlcs { get; set; }

    [JsonProperty("platform")]
    public string Platform { get; set; }

    [JsonProperty("versionName")]
    public string VersionName { get; set; }

    public static async Task<GogDlImport?> Get(IApp app, GogGame game, GogApiAuth auth)
    {
        if (game.InstalledStatus != InstalledStatus.Installed)
            throw new Exception("Game is not installed");
        
        Terminal t = new(app);
        if (!(await t.ExecGog($"import --token {auth.AccessToken} \"{game.InstallPath}\"")) || t.ExitCode != 0)
            return null;

        return JsonConvert.DeserializeObject<GogDlImport>(t.StdOut.First());
    }
}

public class GogDlTask
{
    [JsonProperty("category")]
    public string Category { get; set; }

    [JsonProperty("isPrimary")]
    public bool IsPrimary { get; set; }

    [JsonProperty("languages")]
    public List<string> Languages { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("osBitness")]
    public List<string> OsBitness { get; set; }

    [JsonProperty("path")]
    public string Path { get; set; }
    
    [JsonProperty("type")]
    public string Type { get; set; }
}