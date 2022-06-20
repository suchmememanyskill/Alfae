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
    public List<BaseCommand> GameCommands { get; private set; }
    public List<BaseCommand> GlobalCommands { get; private set; }

    public async Task Initialize(IApp app)
    {
        app.Logger.Log("Hello world!", LogType.Info, "LocalGames");
        GameCommands = new();
        GlobalCommands = new();
        
        GlobalCommands.Add(new BaseCommand("Sup"));
        GlobalCommands.Add(new SeparatorCommand());
        GlobalCommands.Add(new ActionCommand("Attempt a log", () => app.Logger.Log("Test", LogType.Info, "LocalGames")));
        
        await Task.Delay(1);
    }

    public Task<IGame> GetGames()
    {
        throw new NotImplementedException();
    }

    public Task Command(string command, IGame? game)
    {
        throw new NotImplementedException();
    }

    public Task Start(IGame game)
    {
        throw new NotImplementedException();
    }
}