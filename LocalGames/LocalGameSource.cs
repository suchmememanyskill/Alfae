using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Interfaces;

namespace LocalGames;

public class LocalGameSource : IGameSource
{
    public string ServiceName => "Local Games Integration";
    public string Description => "Games added manually will be shown using this plugin";
    public string Version => "v0.1";
    public string SlugServiceName => "local-games";
    public List<Command> GameCommands { get; private set; }
    public List<Command> GlobalCommands { get; private set; }

    public async Task Initialize(IApp app)
    {
        app.Logger.Log("Hello world!", LogType.Info, "LocalGames");
        GameCommands = new();
        GlobalCommands = new();
        
        GlobalCommands.Add(new Command("Sup"));
        GlobalCommands.Add(new Command());
        GlobalCommands.Add(new Command("Attempt a log", () => app.Logger.Log("Test", LogType.Info, "LocalGames")));
        
        await Task.Delay(1);
    }

    public Task<IGame> GetGames()
    {
        throw new NotImplementedException();
    }

    public Task CustomCommand(string command, IGame? game)
    {
        throw new NotImplementedException();
    }

    public Task Start(IGame game)
    {
        throw new NotImplementedException();
    }
}