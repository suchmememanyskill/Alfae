using System;
using System.Collections.Generic;
using System.IO;
using Launcher.Launcher;
using LauncherGamePlugin.Interfaces;
using Newtonsoft.Json;

namespace Launcher.Configuration;

public class Config
{
    public string DownloadLocation { get; set; } =
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Games");

    public string IgnoreVersion { get; set; } = "";
    public Dictionary<string, Dictionary<string, GameConfig>> GameConfigs { get; set; } = new();
    public List<LocalBootProfile> CustomProfiles { get; set; } = new();
    public string WindowsDefaultProfile { get; set; } = "";
    public string LinuxDefaultProfile { get; set; } = "";
    public bool SidebarState { get; set; } = true;

    public static Config Load(IApp app)
    {
        Config? config = new();

        string path = Path.Join(app.ConfigDir, "alfae.json");
        if (File.Exists(path))
        {
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
            return config ?? new();
        }
        
        string customBootProfilePath = Path.Join(app.ConfigDir, "custom_boot_profiles.json");

        if (File.Exists(customBootProfilePath))
        {
            var legacy =
                JsonConvert.DeserializeObject<LegacyBootConfiguration>(File.ReadAllText(customBootProfilePath));

            if (legacy != null)
                legacy.SetOnConfig(config);
        }

        string dlLocPath = Path.Join(app.ConfigDir, "dlloc.txt");

        if (File.Exists(dlLocPath))
        {
            config.DownloadLocation = File.ReadAllText(dlLocPath);
        }
        
        return config;
    }

    public void Save(IApp app)
    {
        File.WriteAllText(Path.Join(app.ConfigDir, "alfae.json"), JsonConvert.SerializeObject(this));
    }

    public GameConfig GetGameConfig(IGame game) => GetGameConfig(game.InternalName, game.Source.ShortServiceName);
    public GameConfig GetGameConfig(string internalName, string serviceName)
    {
        if (!GameConfigs.ContainsKey(serviceName))
            GameConfigs.Add(serviceName, new());
        
        if (!GameConfigs[serviceName].ContainsKey(internalName))
            GameConfigs[serviceName].Add(internalName, new());

        return GameConfigs[serviceName][internalName];
    }
    
    public GameConfig? GetGameConfigOptional(IGame game) => GetGameConfigOptional(game.InternalName, game.Source.ShortServiceName);
    public GameConfig? GetGameConfigOptional(string internalName, string serviceName)
    {
        if (!GameConfigs.ContainsKey(serviceName))
            return null;

        if (!GameConfigs[serviceName].ContainsKey(internalName))
            return null;

        return GameConfigs[serviceName][internalName];
    }
}

public class GameConfig
{
    public string BootProfile { get; set; } = "";
    public List<GameSession> Sessions { get; set; } = new();

    public GameSession GetTotalTime()
    {
        GameSession session = new();

        foreach (var gameSession in Sessions)
        {
            if (gameSession.StartTime < session.StartTime)
                session.StartTime = gameSession.StartTime;

            if (gameSession.EndTime > session.EndTime)
                session.EndTime = gameSession.EndTime;

            session.TimeSpent += gameSession.TimeSpent;
        }

        return session;
    }
}

public class GameSession
{
    public DateTime StartTime { get; set; } = DateTime.MaxValue;
    public DateTime EndTime { get; set; } = DateTime.MinValue;
    public TimeSpan TimeSpent { get; set; } = new();
    public void CalcTimeSpent() => TimeSpent = EndTime - StartTime;
}

[Obsolete("This is only used to upgrade from legacy versions")]
public class LegacyBootConfiguration
{
    public Dictionary<string, Dictionary<string, string>> GameConfiguration { get; set; } = new();
    public List<LocalBootProfile> CustomProfiles { get; set; } = new();
    public List<string> UserDefault { get; set; } = new() {"", ""};

    public void SetOnConfig(Config config)
    {
        foreach (var (key, value) in GameConfiguration)
        {
            foreach (var (s, value1) in value)
            {
                GameConfig gameConfig = config.GetGameConfig(s, key);
                gameConfig.BootProfile = value1;
            }
        }

        config.CustomProfiles = CustomProfiles;
        config.WindowsDefaultProfile = UserDefault[0];
        config.LinuxDefaultProfile = UserDefault[1];
    }
}