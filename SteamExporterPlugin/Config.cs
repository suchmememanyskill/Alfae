using LauncherGamePlugin.Interfaces;
using Newtonsoft.Json;

namespace SteamExporterPlugin;

public class Config
{
    public Dictionary<string, Dictionary<string, GameConfig>> GameConfigs { get; set; } = new();

    public void Save(IApp app)
    {
        string path = Path.Combine(app.ConfigDir, "proton.json");
        File.WriteAllText(path, JsonConvert.SerializeObject(this));
    }
    
    public GameConfig GetConfigForGame(IGame game)
    {
        if (!GameConfigs.ContainsKey(game.Source.ShortServiceName))
            GameConfigs.Add(game.Source.ShortServiceName, new());

        var configs = GameConfigs[game.Source.ShortServiceName];
        
        if (!configs.ContainsKey(game.InternalName))
            configs.Add(game.InternalName, new());

        return configs[game.InternalName];
    }

    public static async Task<Config> Load(IApp app)
    {
        string path = Path.Combine(app.ConfigDir, "proton.json");
        if (!File.Exists(path))
            return new();

        Config? config = JsonConvert.DeserializeObject<Config>(await File.ReadAllTextAsync(path));
        return config ?? new();
    }
}

public class GameConfig
{
    public bool SeparateProtonPath { get; set; } = false;
}