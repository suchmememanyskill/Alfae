using LauncherGamePlugin.Interfaces;
using LegendaryIntegration.Service;
using LauncherGamePlugin.Forms;

namespace LegendaryIntegration.Gui;

public class ImportFileSelect
{
    private LegendaryGame _game;

    public ImportFileSelect(LegendaryGame game)
    {
        _game = game;
    }

    public void Show(IApp app)
        => app.ShowFolderPicker($"Import game '{_game.Name}'", "Game Path", "Import", s => Run(app, s));

    private async void Run(IApp app, string path)
    {
        try
        {
            app.ShowTextPrompt($"Importing {_game.Name}...");
            await _game.Import(path);
            app.HideForm();
        }
        catch (Exception e)
        {
            app.ShowDismissibleTextPrompt(e.Message);
        }
    }
}