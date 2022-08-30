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

    public void Show(IApp app, string errMessage = "")
    {
        List<FormEntry> entries = new()
        {
            Form.TextBox($"Import game '{_game.Name}'", fontWeight: "Bold"),
            Form.FolderPicker("Game Path"),
            Form.Button("Back", _ => app.HideForm(),
                "Import", x =>
                {
                    string path = x.GetValue("Game Path")!;
                    if (!Directory.Exists(path))
                    {
                        Show(app, "Invalid path!");
                        return;
                    }
                    
                    Run(app, path);
                })
        };
        
        if (errMessage != "")
            entries.Add(Form.TextBox(errMessage, FormAlignment.Center));
        
        app.ShowForm(entries);
    }

    public async void Run(IApp app, string? path = null)
    {
        try
        {
            if (path != null)
            {
                app.ShowTextPrompt($"Importing {_game.Name}...");
                await _game.Import(path);
            }
            app.ShowTextPrompt($"Verifying {_game.Name}...");
            await _game.Repair();
            LegendaryGameSource.Source.ReloadGames();
        }
        catch (Exception e)
        {
            app.ShowDismissibleTextPrompt(e.Message);
        }
    }
}