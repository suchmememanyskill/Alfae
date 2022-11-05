using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Launcher.Configuration;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;
using Newtonsoft.Json;

namespace Launcher.Launcher;

public class LauncherConfiguration
{
    public List<IBootProfile> Profiles { get; private set; } = new();
    public IBootProfile? WindowsDefaultProfile { get; private set; }
    public IBootProfile? LinuxDefaultProfile { get; private set; }

    private Loader.App _app;

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
            Profiles.AddRange(await _app.Middleware.GetBootProfiles(appGameSource));
        }
        
        Profiles.AddRange(_app.Config.CustomProfiles);

        Profiles.RemoveAll(x => x.CompatiblePlatform != PlatformExtensions.CurrentPlatform);

        string windowsDefault = _app.Config.WindowsDefaultProfile;
        string linuxDefault = _app.Config.LinuxDefaultProfile;

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

    public void Delete(LocalBootProfile profile)
    {
        _app.Config.CustomProfiles.Remove(profile);
        _app.Config.Save(_app);
        Profiles.Remove(profile);

        if (profile == LinuxDefaultProfile)
            LinuxDefaultProfile = new NativeLinuxProfile();

        if (profile == WindowsDefaultProfile)
            WindowsDefaultProfile = new NativeWindowsProfile();
        
        _app.ReloadBootProfiles();
    }

    public void SetNewDefault(IBootProfile profile)
    {
        if (profile.CompatibleExecutable == Platform.Windows)
        {
            WindowsDefaultProfile = profile;
            _app.Config.WindowsDefaultProfile = profile.Name;
        }
        else
        {
            LinuxDefaultProfile = profile;
            _app.Config.LinuxDefaultProfile = profile.Name;
        }
        
        _app.Config.Save(_app);
        _app.ReloadBootProfiles();
    }

    public void AddCustomProfile(LocalBootProfile profile)
    {
        _app.Config.CustomProfiles.Add(profile);
        Profiles.Add(profile);
    }

    public void Launch(LaunchParams launchParams)
    {
        _app.Logger.Log($"Got request to launch {launchParams.Executable}");
        
        GameConfig gameConfig = _app.Config.GetGameConfig(launchParams.Game);
        IBootProfile? profile = Profiles.Find(x => x.Name == gameConfig.BootProfile);

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

        GameSession session = new();

        profile.OnGameLaunch += x =>
        {
            _app.Logger.Log($"Launched {x.Game.Name}");
            x.InvokeOnGameLaunch();
            
            session.StartTime = DateTime.Now;

            x.Game.IsRunning = true;
            x.Game.InvokeOnUpdate();
        };
        
        profile.OnGameClose += x =>
        {
            _app.Logger.Log($"{x.Game.Name} closed");
            x.InvokeOnGameClose();
            
            session.EndTime = DateTime.Now;
            session.CalcTimeSpent();
            _app.Config.GetGameConfig(x.Game).Sessions.Add(session);
            _app.Config.Save(_app);
            
            x.Game.IsRunning = false;
            x.Game.InvokeOnUpdate();
        };

        profile.Launch(launchParams);
    }
}