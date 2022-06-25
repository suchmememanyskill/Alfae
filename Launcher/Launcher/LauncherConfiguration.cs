using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Forms;

namespace Launcher.Launcher;

public class LauncherConfiguration
{
    public Dictionary<string, Dictionary<string, string>> GameConfiguration { get; set; } = new();
    public List<CustomBootProfile> CustomProfiles { get; set; } = new();
    public List<string> UserDefault { get; set; } = new() {"", ""};

    [JsonIgnore] public List<IBootProfile> Profiles { get; private set; } = new();
    [JsonIgnore] public IBootProfile? WindowsDefaultProfile { get; private set; }
    [JsonIgnore] public IBootProfile? LinuxDefaultProfile { get; private set; }

    [JsonIgnore] private Loader.App _app;

    public LauncherConfiguration(Loader.App app)
    {
        _app = app;
    }

    public void GetProfiles()
    {
        Profiles = new()
        {
            new NativeWindowsProfile(),
            new NativeLinuxProfile()
        };
        
        // TODO: Add proton profiles here
        
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
        commands.Add(new("New Profile", () => CreateProfileForm(new())));
        return commands;
    }

    public void Save()
    {
        // TODO: Save    
    }

    public void Load()
    {
        // TODO: Load
    }

    public void SetNewDefault(IBootProfile profile)
    {
        if (profile.CompatiblePlatform == Platform.Windows)
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

    private CustomBootProfile temp;
    private bool edit = false;
    
    public void CreateProfileForm(CustomBootProfile data, string warnMessage = "", bool edit = false)
    {
        temp = data;
        this.edit = edit;
        string createOrEdit = edit ? "Edit" : "Create";
        
        List<FormEntry> entries = new()
        {
            new(FormEntryType.TextBox, $"{createOrEdit} a custom app wrapper", "Bold", alignment: FormAlignment.Center),
            new(FormEntryType.TextInput, "Name:", data.Name),
            new(FormEntryType.TextInput, "Template:", data.TemplateString),
            new (FormEntryType.TextBox, "Template replaces:\n- {EXEC}: Gets replaced with the executable\n- {ARGS}: Gets replaced with the arguments passed to the executable\n- {WORKDIR}: Gets replaced with the working directory of the executable"),
            new(FormEntryType.TextInput, "Enviroment:", data.EnviromentVariables),
            new(FormEntryType.Dropdown, "Target Executable:",
                data.CompatibleExecutable == Platform.Windows ? "Windows" : "Linux",
                dropdownOptions: new() {"Windows", "Linux"}),
            new(FormEntryType.ButtonList, buttonList: new()
            {
                {"Back", x => _app.HideOverlay()},
                {"Save", x =>
                {
                    _app.HideOverlay();
                    CreateProfile(x.ContainingForm);
                }}
            })
        };
        
        if (warnMessage != "")
            entries.Add(new(FormEntryType.TextBox, warnMessage, "Bold", alignment: FormAlignment.Center));
        
        _app.ShowForm(new(entries));
    }

    public void CreateProfile(Form form)
    {
        temp.Name = form.GetValue("Name:")!;
        temp.TemplateString = form.GetValue("Template:")!;
        temp.EnviromentVariables = form.GetValue("Enviroment:")!;
        temp.CompatibleExecutable =
            form.GetValue("Target Executable:") == "Windows" ? Platform.Windows : Platform.Linux;

        string warn = "";

        if (string.IsNullOrWhiteSpace(temp.Name))
            warn = "Please enter a name";

        if (!edit && CustomProfiles.Any(x => x.Name == temp.Name))
            warn = "You already have a profile with this name";

        if (warn == "" && string.IsNullOrWhiteSpace(temp.TemplateString))
            warn = "Please enter a template";

        if (warn != "")
        {
            CreateProfileForm(temp, warn, edit);
            return;
        }

        if (!edit)
        {
            CustomProfiles.Add(temp);
            Profiles.Add(temp);
        }

        Save();
        _app.ReloadBootProfiles();
    }

    public void Launch(ExecLaunch launch)
    {
        _app.Logger.Log($"Got request to launch {launch.Executable}");

        string preferredProfile = "";

        if (GameConfiguration.TryGetValue(launch.Game.Source.ShortServiceName, out Dictionary<string, string> value))
        {
            if (value.ContainsKey(launch.Game.InternalName))
                preferredProfile = value[launch.Game.InternalName];
        }

        IBootProfile? profile = Profiles.Find(x => x.Name == preferredProfile);

        if (profile == null)
        {
            if (launch.Platform == Platform.Windows)
                profile = WindowsDefaultProfile ?? throw new Exception("Found no profile to launch given executable");
            else
                profile = LinuxDefaultProfile ?? throw new Exception("Found no profile to launch given executable");
        }

        _app.Logger.Log($"Launching {launch.Executable} using {profile.Name}");
        profile.Launch(launch);
    }
}