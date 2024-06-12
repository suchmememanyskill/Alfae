using System.Collections.Generic;
using System.Linq;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Forms;

namespace Launcher.Launcher;

public class CustomBootProfileGUI
{
    private Loader.App _app;
    private LocalBootProfile _profile;
    private bool edit = false;

    public CustomBootProfileGUI(Loader.App app)
    {
        _app = app;
        _profile = new LocalBootProfile();
    }

    public CustomBootProfileGUI(Loader.App app, LocalBootProfile profile) : this(app)
    {
        edit = true;
        _profile = profile;
    }
    
    public void CreateProfileForm(string warnMessage = "")
    {
        string createOrEdit = edit ? "Edit" : "Create";
        
        List<FormEntry> entries = new()
        {
            Form.TextBox($"{createOrEdit} a custom app wrapper", FormAlignment.Center, "Bold"),
            Form.TextInput("Name:", _profile.Name),
            Form.TextInput("Executable:", _profile.Executable),
            Form.TextInput("Args:", _profile.Args),
            Form.TextBox("Template replaces:\n- {EXEC}: Gets replaced with the executable\n- {ARGS}: Gets replaced with the arguments passed to the executable\n- {WORKDIR}: Gets replaced with the working directory of the executable"),
            Form.TextInput("Environment:", _profile.EnviromentVariables),
            Form.Dropdown("Target Executable:",
                dropdownOptions: new() {"Windows", "Linux"}, _profile.CompatibleExecutable == Platform.Windows ? "Windows" : "Linux"),
            Form.Toggle("Escape special characters (Linux only)", _profile.EscapeReplaceables),
            Form.Button("Back", _ => _app.HideForm(), "Save", x =>
            {
                _app.HideForm();
                CreateProfile(x);
            })
        };
        
        if (warnMessage != "")
            entries.Add(Form.TextBox(warnMessage, alignment: FormAlignment.Center, "Bold"));
        
        _app.ShowForm(new(entries));
    }

    public void CreateProfile(Form form)
    {
        _profile.Name = form.GetValue("Name:")!;
        _profile.Executable = form.GetValue("Executable:")!;
        _profile.Args = form.GetValue("Args:")!;
        _profile.EnviromentVariables = form.GetValue("Environment:")!;
        _profile.CompatibleExecutable =
            form.GetValue("Target Executable:") == "Windows" ? Platform.Windows : Platform.Linux;
        _profile.EscapeReplaceables = form.GetValue("Escape special characters (Linux only)") == "1";

        string warn = "";

        if (string.IsNullOrWhiteSpace(_profile.Name))
            warn = "Please enter a name";

        if (!edit && _app.Config.CustomProfiles.Any(x => x.Name == _profile.Name))
            warn = "You already have a profile with this name";

        if (warn == "" && string.IsNullOrWhiteSpace(_profile.Executable))
            warn = "Please enter an executable";

        if (warn != "")
        {
            CreateProfileForm(warn);
            return;
        }

        if (!edit)
        {
            _app.Launcher.AddCustomProfile(_profile);
        }

        _app.Config.Save(_app);
        _app.ReloadBootProfiles();
    }
}