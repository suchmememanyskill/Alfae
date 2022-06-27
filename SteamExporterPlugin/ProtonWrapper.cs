using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Launcher;

namespace SteamExporterPlugin;

public class ProtonWrapper : IBootProfile
{
    public string Name { get; }
    public Platform CompatiblePlatform => Platform.Linux;
    public Platform CompatibleExecutable => Platform.Windows;
    public void Launch(LaunchParams launchParams)
    {
        List<string> args = new()
        {
            "run",
            launchParams.Executable
        };
        args.AddRange(launchParams.ListArguments);

        LaunchParams wrapper = new(Path.Join(_dirPath, "proton"), args, launchParams.WorkingDirectory,
            launchParams.Game, Platform.Linux);
        
        wrapper.EnvironmentOverrides.Add("STEAM_COMPAT_DATA_PATH", _prefixPath);
        wrapper.EnvironmentOverrides.Add("STEAM_COMPAT_CLIENT_INSTALL_PATH", Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "steam"));
        
        new NativeLinuxProfile().Launch(wrapper);
    }

    private string _dirPath;
    private string _prefixPath;

    public ProtonWrapper(string name, string dirPath, string prefixPath)
    {
        Name = name;
        _dirPath = dirPath;
        _prefixPath = prefixPath;
    }
}