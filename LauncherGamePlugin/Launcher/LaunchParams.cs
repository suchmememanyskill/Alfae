using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;

namespace LauncherGamePlugin.Launcher;

public class LaunchParams
{
    public event Action<LaunchParams>? OnGameLaunch;
    public event Action<LaunchParams>? OnGameClose;
    
    public string Executable { get; set; }
    public Dictionary<string, string> EnvironmentOverrides { get; } = new();
    public string WorkingDirectory { get; }
    public Platform Platform { get; }
    public IGame Game { get; }
    public bool UsingListArgs { get; } = false;

    private string _args = "";
    private List<string> _listArgs = new();
    public IBootProfile? ForceBootProfile { get; set; } = null;

    [Obsolete("Please use ListArguments instead")]
    public string Arguments => (UsingListArgs) ? String.Join(" ", _listArgs) : _args;
    public List<string> ListArguments => (UsingListArgs) ? _listArgs : _args.Split(" ").ToList();

    public LaunchParams(string executable, string arguments, string workingDirectory, IGame game, Platform platform)
    {
        Executable = executable;
        _args = arguments;
        WorkingDirectory = workingDirectory;
        Platform = platform;
        Game = game;
    }

    public LaunchParams(string executable, List<string> listArguments, string workingDirectory, IGame game,
        Platform platform)
    {
        Executable = executable;
        _listArgs = listArguments;
        WorkingDirectory = workingDirectory;
        Platform = platform;
        Game = game;
        UsingListArgs = true;
    }

    public LaunchParams(string executable, string arguments, string workingDirectory, IGame game)
        : this(executable, arguments, workingDirectory, game, GetExecTypeFromFileName(executable))
    { }
    
    public LaunchParams(string executable, List<string> arguments, string workingDirectory, IGame game)
        : this(executable, arguments, workingDirectory, game, GetExecTypeFromFileName(executable))
    { }

    public void InvokeOnGameLaunch() => OnGameLaunch?.Invoke(this);
    public void InvokeOnGameClose() => OnGameClose?.Invoke(this);

    public static Platform GetExecTypeFromFileName(string filename) =>
        filename.EndsWith(".exe") ? Platform.Windows : Platform.Linux;
}