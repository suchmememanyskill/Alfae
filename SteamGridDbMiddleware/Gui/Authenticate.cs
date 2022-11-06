using LauncherGamePlugin;
using LauncherGamePlugin.Forms;

namespace SteamGridDbMiddleware.Gui;

public class Authenticate
{
    private SteamGridDb _instance;

    public Authenticate(SteamGridDb instance)
        => _instance = instance;

    public void ShowGui(string errMessage = "")
    {
        List<FormEntry> entries = new()
        {
            Form.TextBox("SteamGridDb Login", FormAlignment.Center, "Bold"),
            Form.TextBox("Log in using an api key, acquired in the SteamGridDb settings page."),
            Form.ClickableLinkBox("Take me to the settings page",
                _ => Utils.OpenUrl("https://www.steamgriddb.com/profile/preferences/api")),
            Form.TextInput("Api Key:"),
            Form.Button("Back", _ => _instance.App.HideForm(), "Login", AttemptLogin)
        };
        
        if (errMessage != "")
            entries.Add(Form.TextBox(errMessage, FormAlignment.Center, "Bold"));

        _instance.App.ShowForm(new(entries));
    }

    private async void AttemptLogin(Form form)
    {
        _instance.App.ShowTextPrompt("Logging in...");
        
        string key = form.GetValue("Api Key:")!;
        bool result = await _instance.CheckLoggedInStatus(key);
        if (result)
        {
            _instance.Storage.Data.ApiKey = key;
            _instance.Storage.Save();
            _instance.App.ReloadGlobalCommands();
            _instance.App.HideForm();
        }
        else
        {
            ShowGui("Failed to log in");
        }
    }
}