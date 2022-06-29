using ItchIoIntegration.Requests;
using LauncherGamePlugin;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;

namespace ItchIoIntegration.Gui;

public class LoginForm
{
    private ItchGameSource _source;
    private IApp _app;

    public LoginForm(ItchGameSource source, IApp app)
    {
        _source = source;
        _app = app;
    }

    public void ShowForm(string errMessage = "")
    {
        List<FormEntry> entries = new()
        {
            Form.TextBox("Itch.io login", FormAlignment.Center, "Bold"),
            Form.TextBox("Log in using an api key, generated via the itch.io site."),
            Form.ClickableLinkBox("Take me to the api page",
                _ => Utils.OpenUrl("https://itch.io/user/settings/api-keys")),
            Form.TextInput("Api Key:"),
            Form.Button("Back", _ => _app.HideOverlay(), "Login", AttemptLogin)
        };
        
        if (errMessage != "")
            entries.Add(Form.TextBox(errMessage, FormAlignment.Center, "Bold"));

        _app.ShowForm(new(entries));
    }

    public async void AttemptLogin(Form form)
    {
        string apiKey = form.GetValue("Api Key:")!;

        _app.ShowTextPrompt("Logging in...");
        
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            ShowForm("Please enter an api key");
            return;
        }

        ItchApiProfile? profile = await ItchApiProfile.Get(apiKey);
        if (profile != null)
        {
            _source.SetNewApiKey(apiKey);
        }
        else
        {
            ShowForm("Failed to log in");
        }
    }
}