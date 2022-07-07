using LauncherGamePlugin;
using LauncherGamePlugin.Forms;

namespace GogIntegration.Gui;

public class ConfigurationGui
{
    private GogIntegration _source;
    private GogGame _game;
    
    public ConfigurationGui(GogIntegration source, GogGame game)
    {
        _source = source;
        _game = game;
    }

    public void Show()
    {
        List<FormEntry> entries = new()
        {
            Form.TextBox($"Configuration for {_game.Name}", FormAlignment.Center, "Bold"),
            Form.TextBox($"Installed platform: {_game.InstalledPlatform}"),
            Form.ClickableLinkBox($"Install location: {_game.InstallPath}", x => Utils.OpenFolder(_game.InstallPath!)),
            Form.TextInput("Commandline args:", _game.ExtraArgs),
            Form.Button("Back", x => _source.App.HideForm(), "Save", x =>
            {
                string args = x.GetValue("Commandline args:")!;
                _game.ExtraArgs = args;
                _source.SaveConfig();
                _source.App.HideForm();
            })
        };
        
        _source.App.ShowForm(entries);
    }
}