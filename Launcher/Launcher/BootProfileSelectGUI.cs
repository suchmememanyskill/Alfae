using System.Collections.Generic;
using System.Linq;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;

namespace Launcher.Launcher;

public class BootProfileSelectGUI
{
    private Loader.App _app;
    private IGame _game;
    
    public BootProfileSelectGUI(Loader.App app, IGame game)
    {
        _app = app;
        _game = game;
    }

    public void ShwoGUI()
    {
        string? currentConfig = _app.Launcher.GetGameConfiguration(_game);
        currentConfig ??= "Default";

        List<string> configs = new() {"Default"};
        configs.AddRange(_app.Launcher.Profiles.Select(x => x.Name));
        
        _app.ShowForm(new(new()
        {
            new FormEntry(FormEntryType.TextBox, $"Boot profile for {_game.Name}", alignment: FormAlignment.Center),
            new FormEntry(FormEntryType.Dropdown, "Boot Profile:", currentConfig, configs),
            new FormEntry(FormEntryType.ButtonList, buttonList: new()
            {
                {"Back", x => _app.HideOverlay()},
                {"Save", x =>
                {
                    string config = x.ContainingForm.GetValue("Boot Profile:")!;
                    if (config == "Default")
                        config = "";
                    
                    _app.Launcher.SetGameConfiguration(_game, config);
                    _app.HideOverlay();
                }}
            })
        }));
    }
}