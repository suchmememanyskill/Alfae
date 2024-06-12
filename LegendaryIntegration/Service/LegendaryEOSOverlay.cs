using LauncherGamePlugin;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LegendaryIntegration.Extensions;

namespace LegendaryIntegration.Service;

public class LegendaryEOSOverlay
{
    private IApp _app;
    private bool installed = false;
    public LegendaryEOSOverlay(IApp app) => _app = app;

    public async void OpenGUI()
    {
        _app.ShowTextPrompt("Loading...");
        installed = File.Exists(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "legendary", "overlay_install.json"));

        List<FormEntry> entries = new()
            {Form.TextBox("EOS Overlay", alignment: FormAlignment.Center, "Bold")};
        
        if (installed)
        {
            Terminal t = new(LegendaryGameSource.Source.App);
            await t.ExecLegendary("eos-overlay info");

            if (t.ExitCode == 0)
            {
                entries.Add(Form.TextBox(string.Join("\n", t.StdErr)));
            }
        }


        List<ButtonEntry> buttons = new()
        {
            new("Back", _ => _app.HideForm())
        };
        
        if (installed)
        {
            buttons.Add(new("Uninstall Overlay", x => Uninstall()));
            buttons.Add(new("Update Overlay", x => Install()));
        }
        else
        {
            buttons.Add(new("Install Overlay", x => Install()));
        }
            
        entries.Add(Form.ButtonList(buttons: buttons));
        _app.ShowForm(new(entries));
    }

    public async void Uninstall()
    {
        _app.ShowTextPrompt("Uninstalling...");
        Terminal t = new(LegendaryGameSource.Source.App);
        await t.ExecLegendary("eos-overlay remove -y");
        _app.HideForm();
    }

    public async void Install()
    {
        _app.ShowTextPrompt("Installing/Updating...");
        string path = Path.Join(LegendaryGameSource.Source.App.GameDir, "legendary", "EOS-Overlay");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        
        Terminal t = new(LegendaryGameSource.Source.App);
        await t.ExecLegendary($"eos-overlay --path \"{path}\" install -y");
        _app.HideForm();
    }
}