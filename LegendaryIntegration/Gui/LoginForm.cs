using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Forms;

namespace LegendaryIntegration.Gui;

public class LoginForm
{
    private LegendaryGameSource _source;
    public static readonly string EPICLOGINLINK = "https://legendary.gl/epiclogin";

    public LoginForm(LegendaryGameSource source)
    {
        _source = source;
    }
    
    public void Show(string warningMessage = "")
    {
        List<FormEntry> entries = new();
        entries.Add(Form.TextBox("Log into Epic Games", alignment: FormAlignment.Center, fontWeight: "Bold"));

        if (PlatformExtensions.CurrentPlatform == Platform.Windows)
        {
            entries.AddRange(new List<FormEntry>()
            {
                Form.Separator(),
                Form.TextBox("Option 1: Login via embedded webpage", FormAlignment.Center),
                Form.TextBox("Click the button below to launch a new window with the Epic Game Store's login page"),
                Form.Button("Back", _ => _source.App.HideForm(), "Login using embedded page", _ => _source.Login()),
                Form.Separator(),
                Form.TextBox("Option 2: Login via authorizationCode", FormAlignment.Center),
            });
        }
        
        entries.AddRange(new List<FormEntry>()
        {
            Form.TextBox("Click the link below, log in, and copy the authorizationCode value into the field below"),
            Form.ClickableLinkBox(EPICLOGINLINK, _ => Utils.OpenUrl(EPICLOGINLINK)),
            Form.TextInput("authorizationCode:"),
            Form.Button("Back", _ => _source.App.HideForm(),
                "Login using authorizationCode", x =>
                {
                    string authcode = x.GetValue("authorizationCode:")!;

                    if (string.IsNullOrWhiteSpace(authcode))
                    {
                        Show("authorizationCode field was left blank");
                        return;
                    }
                    
                    _source.Login(authcode);
                })
        });

        if (warningMessage != "")
        {
            entries.Add(Form.Separator());
            entries.Add(Form.TextBox(warningMessage, FormAlignment.Center, "Bold"));
        }
            
        
        _source.App.ShowForm(entries);
    }
}