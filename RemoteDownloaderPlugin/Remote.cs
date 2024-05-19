using Newtonsoft.Json;

namespace RemoteDownloaderPlugin;

public class Images
{
    public Uri Background { get; set; }
    public Uri HorizontalCover { get; set; }
    public Uri Icon { get; set; }
    public Uri Logo { get; set; }
    public Uri VerticalCover { get; set; }
}

public class EmuFileEntry
{
    [JsonProperty("download_size")]
    public long DownloadSize { get; set; }
    public string Ext { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public Uri Url { get; set; }
    public string Version { get; set; }
}

public interface IEntry
{
    public string GameId { get; }
    public string GameName { get; }
    public Images Img { get; }
    public long GameSize { get; }
}

public class EmuEntry : IEntry
{
    [JsonProperty("game_id")]
    public string GameId { get; set; }
    [JsonProperty("game_name")]
    public string GameName { get; set; }
    public List<EmuFileEntry> Files { get; set; }
    public Images Img { get; set; }
    public string Emu { get; set; }

    [System.Text.Json.Serialization.JsonIgnore] 
    public long GameSize => Files.FirstOrDefault(x => x.Type == "base")?.DownloadSize ?? 0;
}

public class PcEntry : IEntry
{
    [JsonProperty("game_id")]
    public string GameId { get; set; }
    [JsonProperty("game_name")]
    public string GameName { get; set; }
    [JsonProperty("download_size")]
    public long DownloadSize { get; set; }
    [JsonProperty("game_size")]
    public long GameSize { get; set; }
    public Uri Url { get; set; }
    public string Version { get; set; }
    public Images Img { get; set; }
}

public class Remote
{
    public List<PcEntry> Pc { get; set; } = new();
    public List<EmuEntry> Emu { get; set; } = new();
}