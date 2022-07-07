using System.Collections.Generic;
using System.Linq;
using LauncherGamePlugin.Enums;
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
        configs.AddRange(_app.Launcher.Profiles.Where(x =>
        {
            if (_game.EstimatedGamePlatform == Platform.Unknown)
                return true;

            return (_game.EstimatedGamePlatform == x.CompatibleExecutable);
        }).Select(x => x.Name));
        
        
        _app.ShowForm(new(new()
        {
            Form.TextBox($"Boot profile for {_game.Name}", FormAlignment.Center),
            Form.Dropdown("Boot Profile:", configs, currentConfig),
            Form.Button("Back", x => _app.HideForm(), "Save", x =>
            {
                string config = x.GetValue("Boot Profile:")!;
                if (config == "Default")
                    config = "";
                    
                _app.Launcher.SetGameConfiguration(_game, config);
                _app.HideForm();
            })
        }));
    }
}