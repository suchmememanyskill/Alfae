using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace RemoteDownloaderPlugin;

public enum GameType
{
    Pc,
    Emu,
}

[Obsolete]
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

    [Obsolete]
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

    public void Add(DownloadType type)
    {
        switch (type)
        {
            case DownloadType.Base:
                Base++;
                break;
            case DownloadType.Update:
                Update++;
                break;
            case DownloadType.Dlc:
                Dlc++;
                break;
            case DownloadType.Extra:
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

[Obsolete]
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

[Obsolete]
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
    
    public string? PostInstallScriptPath { get; set; }
    public string? PostInstallScriptArgs { get; set; }
    public string? PostInstallScriptWorkingDirectory { get; set; }
}

public class InstalledGameContent
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Platform { get; set; }
    public long GameSize { get; set; }
    public string Version { get; set; }
    public string Filename { get; set; }
    public Images Images { get; set; }
    public ContentTypes InstalledContent { get; set; } = new();
    public string BasePath { get; set; }
}

public class Store
{
    public List<InstalledGameContent> Games { get; set; } = new();
    [Obsolete]
    public List<InstalledEmuGame> EmuGames { get; set; } = new();
    [Obsolete]
    public List<InstalledPcGame> PcGames { get; set; } = new();
    public List<EmuProfile> EmuProfiles { get; set; } = new();
    public List<string> HiddenRemotePlatforms { get; set; } = new();
    public string IndexUrl { get; set; } = "";

    public void Migrate()
    {
        EmuGames.ForEach(x =>
        {
            Games.Add(new()
            {
                Id = x.Id,
                Name = x.Name,
                Platform = x.Emu,
                GameSize = x.GameSize,
                Version = x.Version,
                Filename = x.BaseFilename,
                Images = x.Images,
                InstalledContent = x.Types,
                BasePath = x.BasePath,
            });
        });
        
        PcGames.ForEach(x =>
        {
            Games.Add(new()
            {
                Id = x.Id,
                Name = x.Name,
                Platform = "Pc",
                GameSize = x.GameSize,
                Version = x.Version,
                Filename = null,
                Images = x.Images,
                InstalledContent = new(){ Base = 1 },
                BasePath = x.BasePath,
            });
        });
        
        PcGames.Clear();
        EmuGames.Clear();
    }
}