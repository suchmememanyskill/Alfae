using System;
using System.Diagnostics;
using LauncherGamePlugin;

namespace Launcher.Launcher;

public class Launcher
{
    public void Launch(ExecLaunch args)
    {
        if (PlatformExtensions.CurrentPlatform == Platform.Windows)
        {
            if (args.Platform == Platform.Linux)
                throw new InvalidOperationException("Cannot launch the given executable");

            Process p = new Process();
            
            foreach (var (key, value) in args.EnvironmentOverrides)
                p.StartInfo.Environment[key] = value;

            p.StartInfo.FileName = args.Executable;
            p.StartInfo.WorkingDirectory = args.WorkingDirectory;
            p.StartInfo.Arguments = args.Arguments;
            p.Start();
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}