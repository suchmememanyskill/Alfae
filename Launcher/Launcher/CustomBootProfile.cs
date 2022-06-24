using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using LauncherGamePlugin;
using LauncherGamePlugin.Commands;

namespace Launcher.Launcher;

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

    public void Launch(ExecLaunch launch)
    {
        if (launch.Platform != CompatibleExecutable)
            throw new Exception("Incompatible profile");
        
        string filledString = TemplateString.Replace("{EXEC}", launch.Executable)
            .Replace("{ARGS}", launch.Arguments)
            .Replace("{WORKDIR}", launch.WorkingDirectory);

        string[] split = filledString.Split(" ", 2);

        ExecLaunch convertedLaunch = new(split[0], split[1], Directory.GetCurrentDirectory(),
            launch.Game, CompatibleExecutable);

        foreach (var x in EnviromentVariables.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            split = x.Split("=");
            convertedLaunch.EnvironmentOverrides[split[0]] = split[1];
        }
        
        if (PlatformExtensions.CurrentPlatform == Platform.Windows)
            new NativeWindowsProfile().Launch(convertedLaunch);
        else
            new NativeLinuxProfile().Launch(convertedLaunch);
    }

    public List<Command> CustomCommands()
    {
        return new()
        {
            new("Edit", () => Loader.App.GetInstance().Launcher.CreateProfileForm(this, "", true)),
            new ("Delete")
        };
    }
}