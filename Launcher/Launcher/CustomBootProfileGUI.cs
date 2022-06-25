using System.Collections.Generic;
using System.Linq;
using LauncherGamePlugin;
using LauncherGamePlugin.Forms;

namespace Launcher.Launcher;

public class CustomBootProfileGUI
{
    private Loader.App _app;
    private CustomBootProfile _profile;
    private bool edit = false;

    public CustomBootProfileGUI(Loader.App app)
    {
        _app = app;
        _profile = new();
    }

    public CustomBootProfileGUI(Loader.App app, CustomBootProfile profile) : this(app)
    {
        edit = true;
        _profile = profile;
    }
    
    public void CreateProfileForm(string warnMessage = "")
    {
        string createOrEdit = edit ? "Edit" : "Create";
        
        List<FormEntry> entries = new()
        {
            new(FormEntryType.TextBox, $"{createOrEdit} a custom app wrapper", "Bold", alignment: FormAlignment.Center),
            new(FormEntryType.TextInput, "Name:", _profile.Name),
            new(FormEntryType.TextInput, "Template:", _profile.TemplateString),
            new (FormEntryType.TextBox, "Template replaces:\n- {EXEC}: Gets replaced with the executable\n- {ARGS}: Gets replaced with the arguments passed to the executable\n- {WORKDIR}: Gets replaced with the working directory of the executable"),
            new(FormEntryType.TextInput, "Enviroment:", _profile.EnviromentVariables),
            new(FormEntryType.Dropdown, "Target Executable:",
                _profile.CompatibleExecutable == Platform.Windows ? "Windows" : "Linux",
                dropdownOptions: new() {"Windows", "Linux"}),
            new (FormEntryType.Toggle, "Escape special characters (Linux only)", _profile.EscapeReplaceables ? "1" : "0"),
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
        _profile.Name = form.GetValue("Name:")!;
        _profile.TemplateString = form.GetValue("Template:")!;
        _profile.EnviromentVariables = form.GetValue("Enviroment:")!;
        _profile.CompatibleExecutable =
            form.GetValue("Target Executable:") == "Windows" ? Platform.Windows : Platform.Linux;
        _profile.EscapeReplaceables = form.GetValue("Escape special characters (Linux only)") == "1";

        string warn = "";

        if (string.IsNullOrWhiteSpace(_profile.Name))
            warn = "Please enter a name";

        if (!edit && _app.Launcher.CustomProfiles.Any(x => x.Name == _profile.Name))
            warn = "You already have a profile with this name";

        if (warn == "" && string.IsNullOrWhiteSpace(_profile.TemplateString))
            warn = "Please enter a template";

        if (warn != "")
        {
            CreateProfileForm(warn);
            return;
        }

        if (!edit)
        {
            _app.Launcher.AddCustomProfile(_profile);
        }

        _app.Launcher.Save();
        _app.ReloadBootProfiles();
    }
}