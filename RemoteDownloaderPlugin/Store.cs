using System.Text.Json.Serialization;

namespace RemoteDownloaderPlugin;

public enum GameType
{
    Pc,
    Emu,
}

public interface IInstalledGame
{
    public string Id { get; }
    public string Name { get; }
    public string Version { get; }
    public long GameSize { get; }
    public Images Images { get; }
    public string BasePath { get; set; }
}

public class ContentTypes
{
    public int Base { get; set; }
    public int Update { get; set; }
    public int Dlc { get; set; }
    public int Extra { get; set; }

    public void Add(string type)
    {
        switch (type)
        {
            case "base":
                Base++;
                break;
            case "update":
                Update++;
                break;
            case "dlc":
                Dlc++;
                break;
            case "extra":
                Extra++;
                break;
        }
    }

    public override string ToString()
        => "Content: " + string.Join(", ", new List<string>()
        {
            (Base > 0) ? $"{Base} Base" : "",
            (Update > 0) ? $"{Update} Update" : "",
            (Dlc > 0) ? $"{Dlc} Dlc" : "",
            (Extra > 0) ? $"{Extra} Extra" : ""
        }.Where(x => !string.IsNullOrEmpty(x)));
}

public class InstalledEmuGame : IInstalledGame
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Emu { get; set; }
    public long GameSize { get; set; }
    public string Version { get; set; }
    public string BaseFilename { get; set; }
    public Images Images { get; set; }
    public ContentTypes Types { get; set; } = new();
    public string BasePath { get; set; }
}

public class InstalledPcGame : IInstalledGame
{
    public string Id { get; set; }
    public string Name { get; set; }
    public long GameSize { get; set; }
    public string Version { get; set; }
    public Images Images { get; set; }
    public string BasePath { get; set; }
}

public class EmuProfile
{
    public string Platform { get; set; }
    public string ExecPath { get; set; }
    public string WorkingDirectory { get; set; } = "";
    public string CliArgs { get; set; } = "";
}

public class Store
{
    public List<InstalledEmuGame> EmuGames { get; set; } = new();
    public List<InstalledPcGame> PcGames { get; set; } = new();
    public List<EmuProfile> EmuProfiles { get; set; } = new();
    public List<string> HiddenRemotePlatforms { get; set; } = new();
    public string IndexUrl { get; set; } = "";
}