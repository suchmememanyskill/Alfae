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
        OnGameLaunch?.Invoke(args);
        OnGameLaunch = null;
        Thread t = new(() =>
        {
            p.WaitForExit();
            // Until i figure this out, this will stay commented out :(
            //args.ExecutionTime = p.UserProcessorTime; // TODO: This is a hack but i don't really want to change this event chain right now
            OnGameClose?.Invoke(args);
        });
        t.Start();
    }

    public event Action<LaunchParams>? OnGameLaunch;
    public event Action<LaunchParams>? OnGameClose;
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