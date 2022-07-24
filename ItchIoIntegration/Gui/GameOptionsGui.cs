using ItchIoIntegration.Service;
using LauncherGamePlugin;
using LauncherGamePlugin.Forms;

namespace ItchIoIntegration.Gui;

public class GameOptionsGui
{
    private ItchGame _game;

    public GameOptionsGui(ItchGame game) => _game = game;

    public void ShowGui(string errMessage = "")
    {
        int choice = _game.PreferredTarget;
        if (choice < 0 || choice >= _game.Targets.Count)
            choice = 0;

        List<string> options = _game.Targets.Select(x => $"{x.Path} | {x.Flavour}").ToList();

        string currentChoice =  options.Count > 0 ? options[choice] : "";

        List<FormEntry> entries = new()
        {
            Form.TextBox($"Configuration for {_game.Name}", FormAlignment.Center, "Bold"),
            Form.ClickableLinkBox($"Install location: {_game.InstallPath}", x => Utils.OpenFolder(_game.InstallPath!)),
            Form.TextInput("Commandline args:", _game.CommandlineArgs),
            Form.Dropdown("Boot entry:", options, currentChoice),
            Form.Button("Back", x => _game.ItchSource.App.HideForm(), "Save", x =>
            {
                string choice = x.GetValue("Boot entry:")!;
                string args = x.GetValue("Commandline args:")!;
                int choiceIdx = options.IndexOf(choice);
                _game.PreferredTarget = choiceIdx;
                _game.CommandlineArgs = args;
                _game.ItchSource.SaveConfig();
                _game.ItchSource.App.HideForm();
            })
        };

        if (errMessage != "")
        {
            entries.Add(Form.TextBox(errMessage, FormAlignment.Center, "Bold"));
        }
        
        _game.ItchSource.App.ShowForm(entries);
    }
}