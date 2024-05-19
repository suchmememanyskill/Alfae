using Newtonsoft.Json;

namespace RemoteDownloaderPlugin.Game;

public class PcLaunchDetails
{
    [JsonProperty("launch_exec")]
    public string LaunchExec { get; set; }
    [JsonProperty("working_dir")]
    public string WorkingDir { get; set; }
    [JsonProperty("launch_args")]
    public List<string> LaunchArgs { get; set; }

    public static PcLaunchDetails GetFromPath(string path)
        => JsonConvert.DeserializeObject<PcLaunchDetails>(File.ReadAllText(path))!;
}