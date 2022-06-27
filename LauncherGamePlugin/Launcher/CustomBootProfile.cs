using System.Text.Json.Serialization;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Extensions;

namespace LauncherGamePlugin.Launcher;

public class CustomBootProfile : IBootProfile
{
    public string Executable { get; set; } = "";
    public string Args { get; set; } = "";
    public string EnviromentVariables { get; set; } = "";

    public string Name { get; set; } = "";
    [JsonIgnore]
    public Platform CompatiblePlatform => PlatformExtensions.CurrentPlatform;
    [JsonIgnore]
    public Platform CompatibleExecutable { get => (Platform)CompatibleExecutableInt; set => CompatibleExecutableInt = (int)value; }
    public int CompatibleExecutableInt { get; set; } = 0;
    public bool EscapeReplaceables { get; set; } = false;

    // TODO: use listargs instead of args
    public void Launch(LaunchParams launchParams)
    {
        if (launchParams.Platform != CompatibleExecutable)
            throw new Exception("Incompatible profile");

        string exec = Replace(Executable, launchParams);
        string args = Replace(Args, launchParams);

        LaunchParams convertedLaunchParams = new(exec, args, launchParams.WorkingDirectory,
            launchParams.Game, CompatibleExecutable);

        foreach (var x in EnviromentVariables.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] split = x.Split("=");
            convertedLaunchParams.EnvironmentOverrides[split[0]] = split[1];
        }
        
        if (PlatformExtensions.CurrentPlatform == Platform.Windows)
            new NativeWindowsProfile().Launch(convertedLaunchParams);
        else
            new NativeLinuxProfile().Launch(convertedLaunchParams);
    }

    public virtual List<Command> CustomCommands() => new();

    private string Replace(string s, LaunchParams launchParams)
    {
        if (EscapeReplaceables)
        {
            return s.Replace("{EXEC}", launchParams.Executable.Curse())
                .Replace("{ARGS}", launchParams.Arguments.Curse())
                .Replace("{WORKDIR}", launchParams.WorkingDirectory.Curse());
        }
        else
        {
            return s.Replace("{EXEC}", launchParams.Executable)
                .Replace("{ARGS}", launchParams.Arguments)
                .Replace("{WORKDIR}", launchParams.WorkingDirectory);
        }
    }
}