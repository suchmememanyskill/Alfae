using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RemoteDownloaderPlugin;

public class Images
{
    public Uri Background { get; set; }
    public Uri HorizontalCover { get; set; }
    public Uri Icon { get; set; }
    public Uri Logo { get; set; }
    public Uri VerticalCover { get; set; }
}

public enum DownloadType
{
    Base,
    Update,
    Dlc,
    Extra
}

public class OnlineGameDownloadFileEntry
{
    [JsonProperty("download_size")]
    public long DownloadSize { get; set; }
    [JsonProperty("installed_size")]
    public long InstalledSize { get; set; }
    public string Ext { get; set; }
    public string Name { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public DownloadType Type { get; set; }
    public Uri Url { get; set; }
    public string Version { get; set; }
}

public class OnlineGameDownload
{
    public IList<OnlineGameDownloadFileEntry> Files { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public Images Images { get; set; }
    public string Platform { get; set; }
    [JsonIgnore]
    public long GameSize => Files.Sum(x => x.InstalledSize);
}

public class Remote
{
    public IList<OnlineGameDownload> Games { get; set; } = [];
}