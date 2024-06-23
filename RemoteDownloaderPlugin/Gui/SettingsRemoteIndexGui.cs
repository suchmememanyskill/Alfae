using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;

namespace RemoteDownloaderPlugin.Gui;

public class SettingsRemoteIndexGui
{
    private IApp _app;
    private Plugin _instance;

    private string _indexUrl;
    private string _authUser;
    private string _authPass;

    public SettingsRemoteIndexGui(IApp app, Plugin instance)
    {
        _app = app;
        _instance = instance;
    }

    public void ShowGui()
    {
        List<FormEntry> formEntries = new()
        {
            Form.TextBox("Enter remote index URL", FormAlignment.Left, "Bold"),
            Form.TextInput("Index URL:", _instance.Storage.Data.IndexUrl).NotEmpty(),
            Form.TextInput("User:", _instance.Storage.Data.IndexUser).WhenNotEmpty(),
            Form.TextInput("Pass:", _instance.Storage.Data.IndexPass).WhenNotEmpty(),
            Form.Button("Cancel", _ => _app.HideForm(), "Save", Process)
        };
        
        var errorEntry = Form.TextBox("", FormAlignment.Center);
        formEntries.Add(errorEntry);
        var form = new Form(formEntries)
        {
            ValidationFailureField = errorEntry
        };
        
        _app.ShowForm(form);
    }

    private async void Process(Form form)
    {
        if (!form.Validate(_app))
        {
            return;
        }
        
        _app.ShowTextPrompt("Loading");
        var newUrl = form.GetValue("Index URL:")!;
        var newUser = form.GetValue("User:")!;
        var newPass = form.GetValue("Pass:")!;
        var origIndexUrl = _instance.Storage.Data.IndexUrl;
        var origUser = _instance.Storage.Data.IndexUser;
        var origPass = _instance.Storage.Data.IndexPass;
        
        if (newUrl != origIndexUrl)
        {
            _instance.Storage.Data.IndexUrl = newUrl;
            _instance.Storage.Data.IndexUser = newUser;
            _instance.Storage.Data.IndexPass = newPass;
            if (!await _instance.FetchRemote())
            {
                _instance.Storage.Data.IndexUrl = origIndexUrl;
                _instance.Storage.Data.IndexUser = origUser;
                _instance.Storage.Data.IndexPass = origPass;
                form.ValidationFailureField!.Name = "Failed to fetch data from given URL";
                _app.ShowForm(form);
                return;
            }
            
            _instance.Storage.Save();
        }
        
        _app.HideForm();
        _app.ReloadGames();
    }
}