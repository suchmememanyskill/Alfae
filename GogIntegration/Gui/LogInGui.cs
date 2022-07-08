using GogIntegration.Requests;
using LauncherGamePlugin;
using LauncherGamePlugin.Forms;

namespace GogIntegration.Gui;

public class LogInGui
{
    private GogIntegration _source;

    public LogInGui(GogIntegration source)
    {
        _source = source;
    }

    public void Show(string errMessage = "")
    {
        List<FormEntry> entries = new()
        {
            Form.TextBox("Login to GOG", FormAlignment.Center, "Bold"),
            Form.TextBox("Step 1", FormAlignment.Center),
            Form.TextBox("Please log in on GOG using the button below", FormAlignment.Center),
            Form.Button("Login on GOG", _ => Open()),
            Form.Separator(),
            Form.TextBox("Step 2", FormAlignment.Center),
            Form.TextBox("After logging in, you'll be redirected to an empty page. Copy the URL of this page into the textbox below, then press Login"),
            Form.TextInput("Url:"),
            Form.Button("Back", x => _source.App.HideForm(), "Login", AttemptLogin)
        };
        
        if (errMessage != "")
            entries.Add(Form.TextBox(errMessage, FormAlignment.Center, "Bold"));
        
        _source.App.ShowForm(entries);
    }

    public async void AttemptLogin(Form form)
    {
        _source.App.ShowTextPrompt("Logging in...");
        string url = form.GetValue("Url:")!;
        string[] split = url.Split("code=");
        if (split.Length != 2)
        {
            Show("You seemingly did not put in the right URL");
            return;
        }

        GogApiAuth? auth = await GogApiAuth.Get(split[1]);

        if (auth == null || !await _source.Login(auth))
        {
            Show("Failed to log in");
            return;
        }
        
        _source.App.HideForm();
    }

    private void Open() => Utils.OpenUrl(
        "https://auth.gog.com/auth?client_id=46899977096215655&redirect_uri=https%3A%2F%2Fembed.gog.com%2Fon_login_success%3Forigin%3Dclient&response_type=code&layout=galaxy");
}