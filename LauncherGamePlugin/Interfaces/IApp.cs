using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Launcher;

namespace LauncherGamePlugin.Interfaces;

public interface IApp
{
    public string ConfigDir { get; }
    public string GameDir { get; }
    public Logger Logger { get; }
    void ShowForm(Form form);
    void HideForm();
    void ReloadGames();
    void ReloadGlobalCommands();
    void Launch(LaunchParams launchParams);
    List<IGame> GetAllGames();
    List<IGameSource> GetAllSources();
}