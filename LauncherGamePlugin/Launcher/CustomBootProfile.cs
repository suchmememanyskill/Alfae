using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Extensions;

namespace LauncherGamePlugin.Launcher;

public class CustomBootProfile : IBootProfile
{
    public string TemplateString { get; set; } = "";
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

        string filledString;
        
        if (EscapeReplaceables)
        {
            filledString = TemplateString.Replace("{EXEC}", launchParams.Executable.Curse())
                .Replace("{ARGS}", launchParams.Arguments.Curse())
                .Replace("{WORKDIR}", launchParams.WorkingDirectory.Curse());
        }
        else
        {
            filledString = TemplateString.Replace("{EXEC}", launchParams.Executable)
                .Replace("{ARGS}", launchParams.Arguments)
                .Replace("{WORKDIR}", launchParams.WorkingDirectory);
        }

        string[] split = filledString.Split(" ", 2);

        LaunchParams convertedLaunchParams = new(split[0], split.Length > 1 ? split[1] : "", launchParams.WorkingDirectory,
            launchParams.Game, CompatibleExecutable);

        foreach (var x in EnviromentVariables.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            split = x.Split("=");
            convertedLaunchParams.EnvironmentOverrides[split[0]] = split[1];
        }
        
        if (PlatformExtensions.CurrentPlatform == Platform.Windows)
            new NativeWindowsProfile().Launch(convertedLaunchParams);
        else
            new NativeLinuxProfile().Launch(convertedLaunchParams);
    }

    public virtual List<Command> CustomCommands() => new();
}