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
    public string Version => "v1.2.5";
    public string SlugServiceName => "steam-exporter";
    public string ShortServiceName => "Steam";
    public IApp? App { get; private set; }
    private bool _initialised = false;
    private ProtonManager? _protonManager;
    public Config Config { get; private set; } = new();
    public async Task<InitResult?> Initialize(IApp app)
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

        return null;
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
        string bakDir = Path.Join(Path.GetDirectoryName(vdfPath), "alfae_bak");

        if (!Directory.Exists(bakDir))
            Directory.CreateDirectory(bakDir);

        string bakFile = Path.Join(bakDir, $"{DateTime.Now:yy-MM-dd HH-mm-ss}.vdf");
        File.Copy(vdfPath, bakFile);
        
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
        string iconPath = Path.Combine(gridPath, $"{entry.AppId}_icon.png");
        entry.Icon = iconPath;
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

                    App.ShowTextPrompt($"Updating old entry '{entry.AppName}'");
                    IGame? game = copy.Find(x => gameName == x.Name && x.Source.ShortServiceName == serviceName);
                    if (game != null)
                    {
                        copy.Remove(game);
                        UpdateExe(entry, game);
                        await SetImage(entry, game);
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
                App.ShowTextPrompt($"Adding new entry '{game.Name}'");
                ShortcutEntry entry = ShortcutRoot.AddEntry();
                entry.AppName = $"{game.Name} ({game.Source.ShortServiceName})";
                UpdateExe(entry, game);
                entry.AppId = ShortcutEntry.GenerateSteamGridAppId(entry.AppName, entry.Exe);
                entry.AddTag("UniversalLauncher");

                await SetImage(entry, game);

                addedCount++;
            }

            return new Tuple<int, int>(removedCount, addedCount);
    }

    private async Task SetImage(ShortcutEntry entry, IGame game)
    {
        string verticalCoverPath = Path.Combine(gridPath, $"{entry.AppId}p.jpg");
        string horizontalCoverPath = Path.Combine(gridPath, $"{entry.AppId}.jpg");
        string backgroundPath = Path.Combine(gridPath, $"{entry.AppId}_hero.jpg");
        string logoPath = Path.Combine(gridPath, $"{entry.AppId}_logo.jpg");
        string iconPath = Path.Combine(gridPath, $"{entry.AppId}_icon.png");
        
        byte[]? data = (game.HasImage(ImageType.VerticalCover)) ? await game.GetImage(ImageType.VerticalCover) : null;
        if (data != null)
            await File.WriteAllBytesAsync(verticalCoverPath, data);

        if (game.HasImage(ImageType.HorizontalCover))
        {
            data = await game.GetImage(ImageType.HorizontalCover);
            if (data != null)
                await File.WriteAllBytesAsync(horizontalCoverPath, data);
        }
        else if (game.HasImage(ImageType.VerticalCover)) // To make behaviour consistent with older alfae versions
        {
            if (File.Exists(horizontalCoverPath))
                File.Delete(horizontalCoverPath);
            
            File.Copy(verticalCoverPath, horizontalCoverPath);
        }
        
        data = (game.HasImage(ImageType.Background)) ? await game.GetImage(ImageType.Background) : null;
        if (data != null)
            await File.WriteAllBytesAsync(backgroundPath, data);

        data = (game.HasImage(ImageType.Logo)) ? await game.GetImage(ImageType.Logo) : null;
        if (data != null)
            await File.WriteAllBytesAsync(logoPath, data);
        
        data = (game.HasImage(ImageType.Icon)) ? await game.GetImage(ImageType.Icon) : null;
        if (data != null)
            await File.WriteAllBytesAsync(iconPath, data);
    }
}