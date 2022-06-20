namespace LauncherGamePLugin;

public class ExecLaunch
{
    public event Action<ExecLaunch>? OnGameLaunch;
    public event Action<ExecLaunch>? OnGameExit;

    public string Executable { get; }
    public string Arguments { get; }
    public Dictionary<string, string> EnvironmentOverrides { get; }
    public string WorkingDirectory { get; }

    public ExecLaunch(string executable, string arguments, Dictionary<string, string> environmentOverrides, string workingDirectory)
    {
        Executable = executable;
        Arguments = arguments;
        EnvironmentOverrides = environmentOverrides;
        WorkingDirectory = workingDirectory;
    }
}