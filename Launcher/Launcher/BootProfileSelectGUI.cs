using System.Collections.Generic;
using System.Linq;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;

namespace Launcher.Launcher;

public class BootProfileSelectGUI
{
    private Loader.App _app;
    private IGame _game;
    private string _currentConfig;
    
    public BootProfileSelectGUI(Loader.App app, IGame game)
    {
        _app = app;
        _game = game;
        
        _currentConfig = _app.Launcher.GetGameConfiguration(_game);
        _currentConfig ??= "Default";
    }
    
    public void ShowGUI()
    {
        List<string> configs = new() {"Default"};
        configs.AddRange(_app.Launcher.Profiles.Where(x =>
        {
            if (_game.EstimatedGamePlatform == Platform.Unknown)
                return true;

            return (_game.EstimatedGamePlatform == x.CompatibleExecutable);
        }).Select(x => x.Name));

        FormEntry dropdown = Form.Dropdown("Boot Profile:", configs, _currentConfig);
        dropdown.OnChange += (x) =>
        {
            string config = x.ContainingForm.GetValue("Boot Profile:")!;
            _currentConfig = config;
            ShowGUI();
        };
        
        //IBootProfile profile = _app.Launcher.
        
        List<FormEntry> entries = new()
        {
            Form.TextBox($"Boot profile for {_game.Name}", FormAlignment.Center),
            Form.TextBox($"Estimated platform: {_game.EstimatedGamePlatform}"),
            dropdown,
        };

        if (_game.EstimatedGamePlatform is Platform.Windows or Platform.Linux)
        {
            IBootProfile? profile = (_game.EstimatedGamePlatform == Platform.Windows)
                ? _app.Launcher.WindowsDefaultProfile
                : _app.Launcher.LinuxDefaultProfile;

            if (_currentConfig != "Default")
            {
                profile = _app.Launcher.Profiles.Find(x => x.Name == _currentConfig);
            }

            if (profile != null)
            {
                entries.AddRange(profile.GameOptions(_game));
            }
        }
        
        entries.Add(Form.Button("Back", x => _app.HideForm(), "Save", x =>
        {
            string config = _currentConfig;
            if (config == "Default")
                config = "";
                    
            _app.Launcher.SetGameConfiguration(_game, config);
            _app.HideForm();
        }));
            
        _app.ShowForm(entries);
    }
}