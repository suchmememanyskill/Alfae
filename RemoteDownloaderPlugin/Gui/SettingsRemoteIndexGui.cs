using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;

namespace RemoteDownloaderPlugin.Gui;

public class SettingsRemoteIndexGui
{
    private IApp _app;
    private Plugin _instance;

    public SettingsRemoteIndexGui(IApp app, Plugin instance)
    {
        _app = app;
        _instance = instance;
    }

    public void ShowGui(string possibleError = "")
    {
        List<FormEntry> formEntries = new()
        {
            Form.TextBox("Enter remote index URL", FormAlignment.Left, "Bold"),
            Form.TextInput("Index URL:", _instance.Storage.Data.IndexUrl),
            Form.Button("Cancel", _ => _app.HideForm(), "Save", Process)
        };
        
        if (!string.IsNullOrWhiteSpace(possibleError))
            formEntries.Add(Form.TextBox(possibleError, fontWeight: "Bold"));
        
        _app.ShowForm(formEntries);
    }

    private async void Process(Form form)
    {
        _app.ShowTextPrompt("Loading");
        var newUrl = form.GetValue("Index URL:");
        var origIndexUrl = _instance.Storage.Data.IndexUrl;

        if (string.IsNullOrWhiteSpace(newUrl))
        {
            ShowGui("Index URL is empty");
            return;
        }
        
        if (newUrl != origIndexUrl)
        {
            _instance.Storage.Data.IndexUrl = newUrl;
            if (!await _instance.FetchRemote())
            {
                _instance.Storage.Data.IndexUrl = origIndexUrl;
                ShowGui("Failed to fetch data from given URL");
                return;
            }
            
            _instance.Storage.Save();
        }
        
        _app.HideForm();
        _app.ReloadGames();
    }
}