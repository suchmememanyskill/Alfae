using ItchIoIntegration.Service;
using LauncherGamePlugin.Enums;
using Newtonsoft.Json;

namespace ItchIoIntegration.Requests;

public class ItchApiScannedArchive
{
    [JsonProperty("launch_targets")]
    public List<ItchApiLaunchTarget>? Targets { get; set; }

    public async static Task<ItchApiScannedArchive?> Get(ItchApiProfile profile, ItchGame game, ItchApiUpload upload)
    {
        string url = $"https://api.itch.io/uploads/{upload.Id}/scanned-archive";

        if (game.DownloadKeyId != null)
            url += $"?download_key_id={game.DownloadKeyId}";

        return (await ItchApiRequest.ItchRequest<ItchApiScannedArchiveWrapper>(profile, url))?.Archive ?? null;
    }
}

public class ItchApiLaunchTarget
{
    [JsonProperty("path")]
    public string Path { get; set; }
    
    [JsonProperty("flavor")]
    public string Flavour { get; set; }

    public Platform GetPlatform()
    {
        string[] winPlatforms = {"windows-script", "windows"};
        string[] linuxPlatforms = {"script", "linux"};

        if (winPlatforms.Contains(Flavour))
            return Platform.Windows;

        if (linuxPlatforms.Contains(Flavour))
            return Platform.Linux;

        return Platform.Unknown;
    }
}

public class ItchApiScannedArchiveWrapper
{
    [JsonProperty("scanned_archive")]
    public ItchApiScannedArchive Archive { get; set; }
}