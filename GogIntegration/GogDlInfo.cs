using GogIntegration.Extensions;
using GogIntegration.Requests;
using LauncherGamePlugin;
using LauncherGamePlugin.Interfaces;
using Newtonsoft.Json;

namespace GogIntegration;

public class GogDlInfo
{
    [JsonProperty("download_size")]
    public long DownloadSize { get; set; }

    [JsonProperty("disk_size")]
    public long DiskSize { get; set; }

    [JsonProperty("buildId")]
    public string BuildId { get; set; }

    [JsonProperty("languages")]
    public List<string> Languages { get; set; }

    [JsonProperty("folder_name")]
    public string FolderName { get; set; }

    [JsonProperty("versionEtag")]
    public string VersionEtag { get; set; }

    [JsonProperty("versionName")]
    public string VersionName { get; set; }
    
    [JsonIgnore]
    public string UsedLanguage { get; private set; }

    public static async Task<GogDlInfo?> Get(GogApiAuth auth, GogGame game, IApp app)
    {
        Terminal t = new Terminal(app);
        if (!await t.ExecGog($"info {game.Id} --os windows --token {auth.AccessToken}") || t.ExitCode != 0)
            return null;

        GogDlInfo? round1 = JsonConvert.DeserializeObject<GogDlInfo>(t.StdOut.First());

        if (round1 == null)
            return null;

        var lang = round1.Languages.Contains("en-US") ? "en-US" : round1.Languages.First(); // TODO: Bit of a hack but it works for now
        
        if (!await t.ExecGog($"info {game.Id} --lang={lang} --os windows --token {auth.AccessToken}") || t.ExitCode != 0)
            return null;
        
        GogDlInfo? round2 = JsonConvert.DeserializeObject<GogDlInfo>(t.StdOut.First());

        if (round2 != null)
            round2.UsedLanguage = lang;
        
        return round2;
    }
}