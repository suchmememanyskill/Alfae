using LauncherGamePlugin.Enums;

namespace SteamExporterPlugin;

public class ProtonManager
{
    public static bool IsLinux => PlatformExtensions.CurrentPlatform == Platform.Linux;
    public bool CanUseProton => IsLinux && GetProtonPaths().Count > 0;

    private Dictionary<string, string> cache;

    public Dictionary<string, string> GetProtonPaths()
    {
        if (cache != null)
            return cache;

        string steamApps = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".steam/steam/steamapps/common");

        if (!Directory.Exists(steamApps))
            return new();

        Dictionary<string, string> entries = new();

        foreach (var enumerateDirectory in Directory.EnumerateDirectories(steamApps))
        {
            if (Path.GetFileName(enumerateDirectory).Contains("Proton") && File.Exists(Path.Join(enumerateDirectory, "proton")))
            {
                string version = Path.GetFileName(enumerateDirectory).Split(' ', 2).Last();
                entries.Add(version, enumerateDirectory);
            }
        }

        cache = entries.OrderByDescending(x =>
        {
            if ("0123456789".Contains(x.Key[0]))
            {
                return x.Key;
            }

            return "_" + x.Key;
        }).ToDictionary(g => g.Key, g => g.Value);
        return cache;
    }
}