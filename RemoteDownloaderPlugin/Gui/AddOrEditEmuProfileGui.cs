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

    public AddOrEditEmuProfileGui(IApp app, Plugin instance)
    {
        _app = app;
        _instance = instance;

        _platform = "";
        _path = "";
        _args = "";
        _workDir = "";
        _addOrUpdate = false;
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
            Form.TextInput("Platform:", _platform).NotEmpty(),
            Form.FilePicker("Executable Path:", _path).NotEmpty().Exists(),
            Form.TextInput("CLI Args:", _args).NotEmpty().Contains("{EXEC}"),
            Form.FolderPicker("Working Directory:", _workDir).NotEmpty().Exists(),
        };

        if (_addOrUpdate)
        {
            formEntries.Add(Form.Button( "Remove", _ => {
                    EmuProfile? existingProfile = _instance.Storage.Data.EmuProfiles.FirstOrDefault(x => x.Platform == _platform);

                    if (existingProfile == null)
                    {
                        return;
                    }
                    
                    _app.Show2ButtonTextPrompt($"Do you want to remove platform {existingProfile.Platform}?", "No", "Yes",
                        _ => _app.HideForm(),
                        _ =>
                        {
                            _instance.Storage.Data.EmuProfiles.Remove(existingProfile);
                            _instance.Storage.Save();
                            _app.ReloadGlobalCommands();
                            _app.HideForm();
                        });
            },
            "Cancel", _ => _app.HideForm(), "Save", Process));
        }
        else
        {
            formEntries.Add(Form.Button("Cancel", _ => _app.HideForm(), "Save", Process));
        }

        var errorEntry = Form.TextBox("", FormAlignment.Center);
        formEntries.Add(errorEntry);
        var form = new Form(formEntries)
        {
            ValidationFailureField = errorEntry
        };

        _app.ShowForm(form);
    }

    public void Process(Form form)
    {
        if (!form.Validate(_app))
        {
            return;
        }
        
        _platform = form.GetValue("Platform:");
        _path = form.GetValue("Executable Path:");
        _args = form.GetValue("CLI Args:");
        _workDir = form.GetValue("Working Directory:");
        
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