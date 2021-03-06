using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;

namespace SteamExporterPlugin;

public class ProtonWrapper : IBootProfile
{
    public string Name { get; }
    public Platform CompatiblePlatform => Platform.Linux;
    public Platform CompatibleExecutable => Platform.Windows;
    public void Launch(LaunchParams launchParams)
    {
        GameConfig config = _exporter.Config.GetConfigForGame(launchParams.Game);
        
        List<string> args = new()
        {
            "run",
            launchParams.Executable
        };
        args.AddRange(launchParams.ListArguments);

        LaunchParams wrapper = new(Path.Join(_dirPath, "proton"), args, launchParams.WorkingDirectory,
            launchParams.Game, Platform.Linux);
        
        string homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        string prefixBaseFolder = Path.Join(homeFolder, ".proton_alfae");

        if (!Directory.Exists(prefixBaseFolder))
            Directory.CreateDirectory(prefixBaseFolder);
        
        // To keep compatibility with Alfae <= v1.1.0
        if (Directory.Exists(Path.Join(homeFolder, ".proton_launcher")) && !Directory.Exists(Path.Join(prefixBaseFolder, "default")))
            Directory.Move(Path.Join(homeFolder, ".proton_launcher"), Path.Join(prefixBaseFolder, "default"));
        
        string prefixFolder = Path.Join(prefixBaseFolder, (config.SeparateProtonPath) ? $"{launchParams.Game.Source.SlugServiceName}.{launchParams.Game.InternalName}" : "default");
        
        if (!Directory.Exists(prefixFolder))
            Directory.CreateDirectory(prefixFolder);
        
        wrapper.EnvironmentOverrides.Add("STEAM_COMPAT_DATA_PATH", prefixFolder);
        wrapper.EnvironmentOverrides.Add("STEAM_COMPAT_CLIENT_INSTALL_PATH", Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "steam"));
        
        new NativeLinuxProfile().Launch(wrapper);
    }

    private string _dirPath;
    private Exporter _exporter;
    
    public ProtonWrapper(string name, string dirPath, Exporter exporter)
    {
        Name = name;
        _dirPath = dirPath;
        _exporter = exporter;
    }

    public List<FormEntry> GameOptions(IGame game)
    {
        GameConfig config = _exporter.Config.GetConfigForGame(game);
        FormEntry entry = Form.Toggle("Separate proton path", config.SeparateProtonPath);
        entry.OnChange += (_) =>
        {
            config.SeparateProtonPath = entry.Value == "1";
            _exporter.Config.Save(_exporter.App!);
        };

        return new() {entry};
    }
}