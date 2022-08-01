using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Launcher;

namespace BottlesPlugin;

public class BottlesWrapper : IBootProfile
{
    public string Name { get; }
    public Platform CompatiblePlatform => Platform.Linux;
    public Platform CompatibleExecutable => Platform.Windows;
    private string _internalName;

    public BottlesWrapper(string name, string internalName)
    {
        Name = name;
        _internalName = internalName;
    }
    
    public void Launch(LaunchParams launchParams)
    {
        // Hack to execute .cmd files in bottles
        if (launchParams.Executable.EndsWith(".cmd"))
        {
            string newPath = launchParams.Executable[..^4] + ".bat";
            if (!File.Exists(newPath))
                File.Copy(launchParams.Executable, newPath);

            launchParams.Executable = newPath;
        }
        
        List<string> args = new()
        {
            "run",
            "--command=bottles-cli",
            "com.usebottles.bottles",
            "run",
            "-b",
            _internalName,
            "-e",
            launchParams.Executable,
        };

        if (launchParams.ListArguments.Count >= 1)
        {
            args.Add("-a");
            args.Add(launchParams.Arguments);
        }

        LaunchParams newParams = new("flatpak", args, launchParams.WorkingDirectory, launchParams.Game);
        
        IBootProfile profile = new NativeLinuxProfile();
        profile.OnGameLaunch += _ => OnGameLaunch?.Invoke(launchParams);
        profile.OnGameClose += _ => OnGameClose?.Invoke(launchParams);
        profile.Launch(newParams);
    }

    public event Action<LaunchParams>? OnGameLaunch;
    public event Action<LaunchParams>? OnGameClose;
}