using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;
using Newtonsoft.Json;

namespace Launcher.Launcher;

// TODO: Add time played
public class LauncherConfiguration
{
    public Dictionary<string, Dictionary<string, string>> GameConfiguration { get; set; } = new();
    public List<LocalBootProfile> CustomProfiles { get; set; } = new();
    public List<string> UserDefault { get; set; } = new() {"", ""};

    [JsonIgnore] public List<IBootProfile> Profiles { get; private set; } = new();
    [JsonIgnore] public IBootProfile? WindowsDefaultProfile { get; private set; }
    [JsonIgnore] public IBootProfile? LinuxDefaultProfile { get; private set; }

    [JsonIgnore] private Loader.App _app;

    public LauncherConfiguration()
    {
    }

    public LauncherConfiguration(Loader.App app)
    {
        _app = app;
    }

    public async Task GetProfiles()
    {
        Profiles = new()
        {
            new NativeWindowsProfile(),
            new NativeLinuxProfile()
        };
        
        foreach (var appGameSource in _app.GameSources)
        {
            Profiles.AddRange(await appGameSource.GetBootProfiles());
        }
        
        Profiles.AddRange(CustomProfiles);

        Profiles.RemoveAll(x => x.CompatiblePlatform != PlatformExtensions.CurrentPlatform);

        string windowsDefault = UserDefault[0];
        string linuxDefault = UserDefault[1];

        IBootProfile? windowsDefaultProfile = Profiles.Find(x => x.Name == windowsDefault && x.CompatibleExecutable == Platform.Windows);
        IBootProfile? linuxDefaultProfile = Profiles.Find(x => x.Name == linuxDefault && x.CompatibleExecutable == Platform.Linux);

        if (windowsDefaultProfile == null)
            windowsDefaultProfile = Profiles.FirstOrDefault(x => x.CompatibleExecutable == Platform.Windows);

        WindowsDefaultProfile = windowsDefaultProfile;

        if (linuxDefaultProfile == null)
            linuxDefaultProfile = Profiles.FirstOrDefault(x => x.CompatibleExecutable == Platform.Linux);

        LinuxDefaultProfile = linuxDefaultProfile;
    }

    public List<Command> BuildCommands()
    {
        List<Command> commands = new();
        
        if (WindowsDefaultProfile != null)
            commands.Add(new($"Default for Windows executables: {WindowsDefaultProfile.Name}"));
        
        if (LinuxDefaultProfile != null)
            commands.Add(new ($"Default for Linux executables: {LinuxDefaultProfile.Name}"));
        
        commands.Add(new());
        
        Profiles.ForEach(x =>
        {
            List<Command> subCommands = new()
            {
                new Command("Set as default", () => SetNewDefault(x))
            };
            subCommands.AddRange(x.CustomCommands());
            
            commands.Add(new(x.Name, subCommands));
        });
        
        commands.Add(new());
        commands.Add(new("New Profile", () => new CustomBootProfileGUI(_app).CreateProfileForm()));
        return commands;
    }

    public void Save()
    {
        string path = Path.Join(_app.ConfigDir, "custom_boot_profiles.json");
        File.WriteAllText(path, JsonConvert.SerializeObject(this));
    }

    public void Load()
    {
        string path = Path.Join(_app.ConfigDir, "custom_boot_profiles.json");
        if (File.Exists(path))
        {
            LauncherConfiguration config = JsonConvert.DeserializeObject<LauncherConfiguration>(File.ReadAllText(path));
            GameConfiguration = config!.GameConfiguration;
            CustomProfiles = config.CustomProfiles;
            UserDefault = config.UserDefault;
        }
    }

    public void Delete(LocalBootProfile profile)
    {
        CustomProfiles.Remove(profile);
        Profiles.Remove(profile);

        if (profile == LinuxDefaultProfile)
            LinuxDefaultProfile = new NativeLinuxProfile();

        if (profile == WindowsDefaultProfile)
            WindowsDefaultProfile = new NativeWindowsProfile();
        
        Save();
        _app.ReloadBootProfiles();
    }

    public void SetNewDefault(IBootProfile profile)
    {
        if (profile.CompatibleExecutable == Platform.Windows)
        {
            WindowsDefaultProfile = profile;
            UserDefault[0] = profile.Name;
        }
        else
        {
            LinuxDefaultProfile = profile;
            UserDefault[1] = profile.Name;
        }
        
        Save();
        _app.ReloadBootProfiles();
    }

    public void AddCustomProfile(LocalBootProfile profile)
    {
        CustomProfiles.Add(profile);
        Profiles.Add(profile);
    }

    public string? GetGameConfiguration(IGame game)
    {
        if (GameConfiguration.TryGetValue(game.Source.ShortServiceName, out Dictionary<string, string> value))
        {
            if (value.ContainsKey(game.InternalName))
                return value[game.InternalName];
        }

        return null;
    }

    public void SetGameConfiguration(IGame game, string value)
    {
        if (!GameConfiguration.ContainsKey(game.Source.ShortServiceName))
            GameConfiguration.Add(game.Source.ShortServiceName, new());
        
        GameConfiguration[game.Source.ShortServiceName][game.InternalName] = value;
        Save();
    }

    public void Launch(LaunchParams launchParams)
    {
        _app.Logger.Log($"Got request to launch {launchParams.Executable}");
        
        string? preferredProfile = GetGameConfiguration(launchParams.Game);
        preferredProfile ??= "";

        if (GameConfiguration.TryGetValue(launchParams.Game.Source.ShortServiceName, out Dictionary<string, string> value))
        {
            if (value.ContainsKey(launchParams.Game.InternalName))
                preferredProfile = value[launchParams.Game.InternalName];
        }

        IBootProfile? profile = Profiles.Find(x => x.Name == preferredProfile);

        if (launchParams.ForceBootProfile != null)
            profile = launchParams.ForceBootProfile;

        if (profile == null)
        {
            if (launchParams.Platform == Platform.Windows)
                profile = WindowsDefaultProfile;
            else
                profile = LinuxDefaultProfile;
        }
        
        if (profile == null)
            throw new Exception("Found no profile to launch given executable");

        _app.Logger.Log($"Launching {launchParams.Executable} using {profile.Name}");

        profile.OnGameLaunch += x =>
        {
            _app.Logger.Log($"Launched {x.Game.Name}");
            x.InvokeOnGameLaunch();
        };
        
        profile.OnGameClose += x =>
        {
            _app.Logger.Log($"{x.Game.Name} closed");
            x.InvokeOnGameClose();
        };

        profile.Launch(launchParams);
    }
}