using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;

namespace GogIntegration.Gui;

public class DownloadPickGui
{
    private GogGame _game;
    private IApp _app;

    public DownloadPickGui(GogGame game, IApp app)
    {
        _game = game;
        _app = app;
    }

    public void Show()
    {
        var list = new List<ButtonEntry>()
        {
            new("Back", _ => _app.HideForm())
        };
        
        list.AddRange(_game.Platforms.GetAvailablePlatforms().Select(x => new ButtonEntry(x.ToString(), _ =>
        {
            _app.HideForm();
            _game.Download(x);
        })));
        
        _app.ShowForm(new List<FormEntry>()
        {
            Form.TextBox($"Multiple platforms are available for '{_game.Name}'! Please pick a platform", FormAlignment.Center),
            Form.ButtonList(list)
        });
    }
}