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
            new(FormEntryType.TextBox, "Itch.io login", "Bold", alignment: FormAlignment.Center),
            new(FormEntryType.TextBox, "Log in using an api key, generated via the itch.io site."),
            new(FormEntryType.ClickableLinkBox, "Take me to the api page", "https://itch.io/user/settings/api-keys",
                linkClick: x => Utils.OpenUrl(x.Value)),
            new(FormEntryType.TextInput, "Api Key:"),
            new(FormEntryType.ButtonList, buttonList: new()
            {
                {"Back", x => _app.HideOverlay()},
                {"Login", x => AttemptLogin(x.ContainingForm)}
            })
        };
        
        if (errMessage != "")
            entries.Add(new(FormEntryType.TextBox, errMessage, "Bold", alignment: FormAlignment.Center));
        
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