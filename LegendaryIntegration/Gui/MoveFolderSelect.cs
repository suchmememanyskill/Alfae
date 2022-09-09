using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LegendaryIntegration.Service;

namespace LegendaryIntegration.Gui;

public class MoveFolderSelect
{
    private LegendaryGame _game;

    public MoveFolderSelect(LegendaryGame game)
    {
        _game = game;
    }

    public void Show(IApp app)
        => app.ShowFolderPicker($"Move game '{_game.Name}'", "Game Path", "Move", s => Run(app, s));

    private async void Run(IApp app, string path)
    {
        try
        {
            await _game.Move(path);
        }
        catch (Exception e)
        {
            app.ShowDismissibleTextPrompt(e.Message);
        }
    }
}