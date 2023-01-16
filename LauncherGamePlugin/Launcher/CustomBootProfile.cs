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
    
    public void Launch(LaunchParams launchParams)
    {
        if (launchParams.Platform != CompatibleExecutable)
            throw new Exception("Incompatible profile");

        List<string> args = new();
        foreach (var s in Args.Split(" ", StringSplitOptions.RemoveEmptyEntries))
        {
            if (s == "{EXEC}" || s == "\"{EXEC}\"") // Backwards compat
                args.Add((EscapeReplaceables ? launchParams.Executable.Curse() : launchParams.Executable));
            
            else if (s == "{ARGS}")
                args.AddRange(launchParams.ListArguments);
            
            else if (s == "{WORKDIR}")
                args.Add((EscapeReplaceables ? launchParams.WorkingDirectory.Curse() : launchParams.WorkingDirectory));
            
            else
                args.Add(s);
        }

        string exec = Replace(Executable, launchParams);

        LaunchParams convertedLaunchParams = new(exec, args, launchParams.WorkingDirectory,
            launchParams.Game, CompatibleExecutable);

        foreach (var x in EnviromentVariables.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] split = x.Split("=");
            convertedLaunchParams.EnvironmentOverrides[split[0]] = split[1];
        }

        IBootProfile profile = (PlatformExtensions.CurrentPlatform == Platform.Windows)
            ? new NativeWindowsProfile()
            : new NativeLinuxProfile();

        profile.OnGameLaunch += _ => OnGameLaunch?.Invoke(launchParams);
        profile.OnGameClose += _ => OnGameClose?.Invoke(launchParams);
        profile.Launch(convertedLaunchParams);
    }

    public event Action<LaunchParams>? OnGameLaunch;
    public event Action<LaunchParams>? OnGameClose;

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