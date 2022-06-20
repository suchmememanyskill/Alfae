using LauncherGamePlugin.Commands;

namespace LauncherGamePlugin.Interfaces;

public interface IGameSource
{
    string ServiceName { get; }
    string Description { get; }
    string Version { get; }
    string SlugServiceName { get; }
    List<BaseCommand> GameCommands { get; }
    List<BaseCommand> GlobalCommands { get; }

    public Task Initialize(IApp app);
    public Task<IGame> GetGames();
    public Task Command(string command, IGame? game);
    public Task Start(IGame game);
    
}