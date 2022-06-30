using ItchIoIntegration.Service;
using LauncherGamePlugin.Forms;

namespace ItchIoIntegration.Gui;

public class ChangePreferredTargetGui
{
    private ItchGame _game;

    public ChangePreferredTargetGui(ItchGame game) => _game = game;

    public void ShowGui(string errMessage = "")
    {
        int choice = _game.PreferredTarget;
        if (choice < 0 || choice >= _game.Targets.Count)
            choice = 0;

        List<string> options = _game.Targets.Select(x => $"{x.Path} | {x.Flavour}").ToList();

        string currentChoice =  options[choice];

        List<FormEntry> entries = new()
        {
            Form.TextBox($"Boot entries for {_game.Name}", FormAlignment.Center, "Bold"),
            Form.Dropdown("Boot entry:", options, currentChoice),
            Form.Button("Back", x => _game.ItchSource.App.HideOverlay(), "Save", x =>
            {
                string choice = x.GetValue("Boot entry:")!;
                int choiceIdx = options.IndexOf(choice);
                _game.PreferredTarget = choiceIdx;
                _game.ItchSource.SaveConfig();
                _game.ItchSource.App.HideOverlay();
            })
        };

        if (errMessage != "")
        {
            entries.Add(Form.TextBox(errMessage, FormAlignment.Center, "Bold"));
        }
        
        _game.ItchSource.App.ShowForm(entries);
    }
}