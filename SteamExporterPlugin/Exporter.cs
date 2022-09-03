using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;
using VDFMapper.ShortcutConfig;
using VDFMapper.ShortcutMap;
using VDFMapper.VDF;

namespace SteamExporterPlugin;

public class Exporter : IGameSource
{
    public string ServiceName => "Steam Exporter";
    public string Version => "v1.2.1";
    public string SlugServiceName => "steam-exporter";
    public string ShortServiceName => "Steam";
    public IApp? App { get; private set; }
    private bool _initialised = false;
    private ProtonManager? _protonManager;
    public Config Config { get; private set; } = new();
    public async Task Initialize(IApp app)
    {
        App = app;
        Config = await Config.Load(app);
        try
        {
            _initialised = InitialisePaths();
            _protonManager = new();
        }
        catch
        {
            _initialised = false;
        }
    }

    public async Task<List<IBootProfile>> GetBootProfiles()
    {
        if (_protonManager == null)
            return new();

        if (!_protonManager.CanUseProton)
            return new();

        var protons = _protonManager.GetProtonPaths();
        return protons.Select(x => (IBootProfile) new ProtonWrapper($"Proton {x.Key}", x.Value, this)).ToList();
    }

    public List<Command> GetGlobalCommands()
    {
        if (!_initialised)
        {
            return new()
            {
                new("Failed to initialize. Is steam running?"),
                new(),
                new("Re-Initialize", ReInitialize)
            };
        }
        else
        {
            return new()
            {
                new("Sync games to steam", UpdateSteamGames),
                new("Remove steam games", RemoveSteamGames)
            };
        }
    }

    public void ReInitialize()
    {
        try
        {
            _initialised = InitialisePaths();
        }
        catch
        {
            _initialised = false;
        }
        App.ReloadGlobalCommands();
    }

    public async void UpdateSteamGames()
    {
        App.ShowTextPrompt("Updating steam games...");
        if (!Read())
        {
            App.ShowDismissibleTextPrompt($"Failure reading {vdfPath}.\nDo you have at least 1 non-steam game shortcut in steam?");
            return;
        }

        Tuple<int, int> res = await Update();

        if (!Write())
        {
            App.ShowDismissibleTextPrompt($"Failure writing to {vdfPath}");
            return;
        }
        
        App.ShowDismissibleTextPrompt($"Added {res.Item2} games to steam, and removed {res.Item1} games from steam");
    }

    public void RemoveSteamGames()
    {
        App.ShowTextPrompt("Removing steam games...");
        
        if (!Read())
        {
            App.ShowDismissibleTextPrompt($"Failure reading {vdfPath}.\nDo you have at least 1 non-steam game shortcut in steam?");
            return;
        }

        int res = RemoveAllGamesWithTag();

        if (!Write())
        {
            App.ShowDismissibleTextPrompt($"Failure writing to {vdfPath}");
            return;
        }
        
        App.ShowDismissibleTextPrompt($"Removed {res} games from steam");
    }
    
    private string vdfPath = "";
    private string gridPath = "";
    private VDFMap? root;
    public ShortcutRoot? ShortcutRoot { get; private set; }

    public bool InitialisePaths()
    {
        if (GetSteamShortcutPath.GetUserDataPath() == "")
            return false;

        if (GetSteamShortcutPath.GetCurrentlyLoggedInUser() <= 0)
            return false;

        vdfPath = GetSteamShortcutPath.GetShortcutsPath();
        gridPath = GetSteamShortcutPath.GetGridPath();
        return true;
    }
    
    public bool Read()
    {
        if (!File.Exists(vdfPath))
            return false;

        VDFStream stream = new VDFStream(vdfPath);
        root = new VDFMap(stream);
        ShortcutRoot = new ShortcutRoot(root);
        stream.Close();
        return true;
    }

    public bool Write()
    {
        File.WriteAllText(vdfPath, "");
        BinaryWriter writer = new BinaryWriter(new FileStream(vdfPath, FileMode.OpenOrCreate));
        root!.Write(writer, null);
        writer.Close();
        return true;
    }
    
    public int RemoveAllGamesWithTag()
    {
        List<string> uniqueServices = App.GetAllSources().Select(x => x.ShortServiceName).ToList();
        
        int count = 0;

        for (int i = 0; i < ShortcutRoot!.GetSize(); i++)
        {
            if (uniqueServices.Any(x => ShortcutRoot.GetEntry(i).AppName?.Contains($"({x})") ?? false))
            {
                ShortcutEntry entry = ShortcutRoot.GetEntry(i);
                string path = Path.Combine(gridPath, $"{entry.AppId}.jpg");
                string pPath = Path.Combine(gridPath, $"{entry.AppId}p.jpg");
                string heroPath = Path.Combine(gridPath, $"{entry.AppId}_hero.jpg");
                
                if (File.Exists(path))
                    File.Delete(path);
                
                if (File.Exists(pPath))
                    File.Delete(pPath);
                
                if (File.Exists(heroPath))
                    File.Delete(heroPath);
                
                ShortcutRoot.RemoveEntry(i);
                i--;
                count++;
            }
        }

        return count;
    }
    
    private void UpdateExe(ShortcutEntry entry, IGame game)
    {
        entry.Exe = Utils.GetExecutablePath();
        if (entry.Exe.Contains(" "))
            entry.Exe = $"\"{entry.Exe}\"";
        entry.LaunchOptions = $"{game.Source.SlugServiceName} \"{game.InternalName}\" Launch";
    }
    
    public async Task<Tuple<int, int>> Update()
    {
        List<IGame> copy = App.GetAllGames().Where(x => x.InstalledStatus == InstalledStatus.Installed).ToList();
            List<string> uniqueServices = App.GetAllSources().Select(x => x.ShortServiceName).ToList();
            List<int> unknownIndexes = new();

            int removedCount = 0;
            int addedCount = 0;

            for (int i = 0; i < ShortcutRoot!.GetSize(); i++)
            {
                ShortcutEntry entry = ShortcutRoot.GetEntry(i);
                if (uniqueServices.Any(x => entry.AppName?.Contains($"({x})") ?? false))
                {
                    int idx = entry.AppName.LastIndexOf("(");

                    string gameName = entry.AppName[..(idx - 1)];
                    string serviceName = entry.AppName[(idx + 1)..^1];

                    IGame? game = copy.Find(x => gameName == x.Name && x.Source.ShortServiceName == serviceName);
                    if (game != null)
                    {
                        copy.Remove(game);
                        UpdateExe(entry, game);
                    }
                    else // Game that doesn't seem to be in the list. lets remove it
                    {
                        ShortcutRoot.RemoveEntry(i);
                        removedCount++;
                        i--;
                    }
                }
            }

            foreach (var game in copy)
            {
                ShortcutEntry entry = ShortcutRoot.AddEntry();
                entry.AppName = $"{game.Name} ({game.Source.ShortServiceName})";
                UpdateExe(entry, game);
                entry.AppId = ShortcutEntry.GenerateSteamGridAppId(entry.AppName, entry.Exe);
                entry.AddTag("UniversalLauncher");

                string path = Path.Combine(gridPath, $"{entry.AppId}.jpg");
                string pPath = Path.Combine(gridPath, $"{entry.AppId}p.jpg");
                string heroPath = Path.Combine(gridPath, $"{entry.AppId}_hero.jpg");

                if (!File.Exists(path))
                {
                    byte[]? cover = await game.CoverImage();
                    if (cover != null)
                        await File.WriteAllBytesAsync(path, cover);
                }

                if (File.Exists(path) && !File.Exists(pPath))
                    File.Copy(path, pPath);
                
                if (!File.Exists(heroPath))
                {
                    byte[]? background = await game.BackgroundImage();
                    if (background != null)
                        await File.WriteAllBytesAsync(heroPath, background);
                }

                addedCount++;
            }

            return new Tuple<int, int>(removedCount, addedCount);
        }
}