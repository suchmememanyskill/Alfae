using System.Text.Json.Serialization;
using LauncherGamePlugin;
using LauncherGamePlugin.Interfaces;

namespace LocalGames.Data;

public class LocalGame : IGame
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

    [JsonIgnore] public InstalledStatus InstalledStatus => InstalledStatus.Installed;
    [JsonIgnore] public ProgressStatus ProgressStatus { get; set; }

    [JsonIgnore] public string InstalledPath => Path.GetDirectoryName(ExecPath);
    
    [JsonIgnore]
    public IGameSource Source { get; set; }
}