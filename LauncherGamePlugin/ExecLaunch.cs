namespace LauncherGamePlugin;

public class ExecLaunch
{
    /* TODO: Implement
    public event Action<ExecLaunch>? OnGameLaunch;
    public event Action<ExecLaunch>? OnGameExit;
    */
    public string Executable { get; }
    public string Arguments { get; }
    public Dictionary<string, string> EnvironmentOverrides { get; } = new();
    public string WorkingDirectory { get; }
    public Platform Platform { get; }

    public ExecLaunch(string executable, string arguments, string workingDirectory, Platform platform)
    {
        Executable = executable;
        Arguments = arguments;
        WorkingDirectory = workingDirectory;
        Platform = platform;
    }

    public ExecLaunch(string executable, string arguments, string workingDirectory)
        : this(executable, arguments, workingDirectory, GetExecTypeFromFileName(executable))
    { }

    public static Platform GetExecTypeFromFileName(string filename) =>
        filename.EndsWith(".exe") ? Platform.Windows : Platform.Linux;
}