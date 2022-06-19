namespace LauncherGamePlugin.Interfaces;

public interface IGameSource
{
    string ServiceName { get; }
    string Description { get; }
    string Version { get; }

    public Task<bool> Initialize(IApp app);
    public Task<IGame> GetGames();
}