using System.Text.Json.Serialization;
using LauncherGamePlugin;
using LauncherGamePlugin.Interfaces;

namespace LocalGames.Data;

public class LocalGame : IInstalledGame
{
    public string Name { get; set; }
    public string ExecPath { get; set; }
    public long? Size { get; set; }

    
    public Task<byte[]> CoverImage()
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> BackgroundImage()
    {
        throw new NotImplementedException();
    }

    [JsonIgnore]
    public List<Platform> AvailablePlatforms => new() {ExecPath.EndsWith(".exe") ? Platform.Windows : Platform.Linux};

    [JsonIgnore] public string InstalledPath => Path.GetDirectoryName(ExecPath);
    
    [JsonIgnore]
    public IGameSource Source { get; set; }
    
    [JsonIgnore]
    public Uri? Url => null;

    [JsonIgnore] public string? AvailableVersion => null;

    [JsonIgnore] public string? InstalledVersion => null;

    [JsonIgnore] public string? Developer => null;
}