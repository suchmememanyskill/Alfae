using System.Diagnostics;
using LauncherGamePlugin.Interfaces;

namespace LocalGames;

public class LocalGameSource : IGameSource
{
    public string ServiceName => "Local Games";
    public string Description => "Games added manually will be shown using this plugin";
    public string Version => "v0.1";
    public async Task<bool> Initialize(IApp app)
    {
        Debug.WriteLine("[LocalGameSource] Hello world!");
        return true;
    }

    public Task<IGame> GetGames()
    {
        throw new NotImplementedException();
    }
}