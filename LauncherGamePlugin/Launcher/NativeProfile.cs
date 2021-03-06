using System.Diagnostics;
using LauncherGamePlugin.Enums;

namespace LauncherGamePlugin.Launcher;

public abstract class NativeProfile : IBootProfile
{
    public string Name => "Native Launcher";
    public Platform CompatiblePlatform { get; protected set; }
    public Platform CompatibleExecutable { get; protected set; }

    public void Launch(LaunchParams args)
    {
        Process p = new Process();

        foreach (var (key, value) in args.EnvironmentOverrides)
            p.StartInfo.Environment[key] = value;

        p.StartInfo.FileName = args.Executable;
        p.StartInfo.WorkingDirectory = args.WorkingDirectory;

        if (args.UsingListArgs)
            args.ListArguments.ForEach(x => p.StartInfo.ArgumentList.Add(x));
        else
            p.StartInfo.Arguments = args.Arguments;

        p.Start();
    }
}

public class NativeWindowsProfile : NativeProfile
{
    public NativeWindowsProfile()
    {
        CompatibleExecutable = Platform.Windows;
        CompatiblePlatform = Platform.Windows;
    }
}

public class NativeLinuxProfile : NativeProfile
{
    public NativeLinuxProfile()
    {
        CompatibleExecutable = Platform.Linux;
        CompatiblePlatform = Platform.Linux;
    }
}