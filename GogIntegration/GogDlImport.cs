using GogIntegration.Extensions;
using GogIntegration.Requests;
using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;
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
        
        string path = game.InstalledPlatform == LauncherGamePlugin.Enums.Platform.Linux
            ? Path.Join(game.InstallPath!, "game")
            : game.InstallPath!;
        
        Terminal t = new(app);
        if (!(await t.ExecGog($"import --token {auth.AccessToken} \"{path}\"")) || t.ExitCode != 0 || t.StdOut.Count <= 0)
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
    public string ExecPath { get; set; }
    
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("arguments")] 
    public List<string> Args { get; set; } = new();

    public LaunchParams ToLaunchParams(GogGame game)
    {
        if (Type != "FileTask")
            throw new Exception($"Unknown task type: {Type}");

        string execPath = game.InstalledPlatform == Platform.Linux
            ? Path.Join(game.InstallPath, "game", ExecPath)
            : Path.Join(game.InstallPath, ExecPath);

        List<string> args = new(Args);
        
        if (!string.IsNullOrWhiteSpace(game.ExtraArgs))
            args.Add(game.ExtraArgs);
        
        LaunchParams launchParams = new(execPath, args, Path.GetDirectoryName(execPath)!, game, game.InstalledPlatform);
        return launchParams;
    }
}