using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;

namespace RemoteDownloaderPlugin.Gui;

public class AddOrEditEmuProfileGui
{
    private IApp _app;
    private Plugin _instance;
    private string _platform;
    private string _path;
    private string _args;
    private string _workDir;
    private readonly bool _addOrUpdate;
    private string _error;

    public AddOrEditEmuProfileGui(IApp app, Plugin instance)
    {
        _app = app;
        _instance = instance;

        _platform = "";
        _path = "";
        _args = "";
        _workDir = "";
        _addOrUpdate = false;
        _error = "";
    }

    public AddOrEditEmuProfileGui(IApp app, Plugin instance, EmuProfile profile)
        : this(app, instance)
    {
        _platform = profile.Platform;
        _path = profile.ExecPath;
        _args = profile.CliArgs;
        _workDir = profile.WorkingDirectory;
        _addOrUpdate = true;
    }
    
    public void ShowGui()
    {
        List<FormEntry> formEntries = new()
        {
            Form.TextBox(_addOrUpdate ? "Add new emulation platform" : "Edit emulation platform", FormAlignment.Left, "Bold"),
            Form.TextInput("Platform:", _platform),
            Form.FilePicker("Executable Path:", _path),
            Form.TextInput("CLI Args:", _args),
            Form.FolderPicker("Working Directory:", _workDir),
            Form.Button("Cancel", _ => _app.HideForm(), "Save", Process)
        };
        
        if (!string.IsNullOrWhiteSpace(_error))
            formEntries.Add(Form.TextBox(_error, fontWeight: "Bold"));
        
        _app.ShowForm(formEntries);
    }

    public void Process(Form form)
    {
        _platform = form.GetValue("Platform:");
        _path = form.GetValue("Executable Path:");
        _args = form.GetValue("CLI Args:");
        _workDir = form.GetValue("Working Directory:");

        if (string.IsNullOrWhiteSpace(_platform) || 
            string.IsNullOrWhiteSpace(_path) ||
            string.IsNullOrWhiteSpace(_args) ||
            string.IsNullOrWhiteSpace(_workDir))
        {
            _error = "Not all fields are filled";
            ShowGui();
            return;
        }

        if (!File.Exists(_path))
        {
            _error = "Executable path does not exist";
            ShowGui();
            return;
        }

        if (!Directory.Exists(_workDir))
        {
            _error = "Working directory does not exist";
            ShowGui();
            return;
        }

        if (!_args.Contains("{EXEC}"))
        {
            _error = "Args does not specify an {EXEC} param";
            ShowGui();
            return;
        }

        EmuProfile? existingProfile = _instance.Storage.Data.EmuProfiles.FirstOrDefault(x => x.Platform == _platform);
        if (existingProfile == null)
        {
            _instance.Storage.Data.EmuProfiles.Add(new()
            {
                Platform = _platform,
                ExecPath = _path,
                CliArgs = _args,
                WorkingDirectory = _workDir
            });
        }
        else
        {
            existingProfile.CliArgs = _args;
            existingProfile.ExecPath = _path;
            existingProfile.WorkingDirectory = _workDir;
        }
        
        _instance.Storage.Save();
        _app.HideForm();
        _app.ReloadGlobalCommands();
    }
}