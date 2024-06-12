using LauncherGamePlugin;
using LauncherGamePlugin.Forms;
using LegendaryIntegration.Service;

namespace LegendaryIntegration.Extensions;

public static class LegendaryGameForm
{
    public static Form? ToForm(this LegendaryGame game)
    {
        if (!game.IsInstalled)
            return null;
        
        Form f = new(new()
        {
            Form.TextBox("Legendary game configuration and info", FormAlignment.Center, "Bold"),
            Form.TextBox($"{game.Name} by {game.Developer}"),
            Form.TextBox($"AppId: {game.InternalName}"),
            Form.TextBox($"Current Version: {game.InstalledVersion}"),
            Form.ClickableLinkBox($"Location on disk: {game.InstallPath}", _ => Utils.OpenFolder(game.InstallPath)),
            Form.TextBox("\nConfig", FormAlignment.Center, "Bold"),
            Form.Toggle("Always launch offline", game.ConfigAlwaysOffline),
            Form.Toggle("Always skip version check", game.ConfigAlwaysSkipUpdateCheck),
            Form.TextInput("Additional game arguments", game.ConfigAdditionalGameArgs),
            Form.Button("Back", _ => LegendaryGameSource.Source.App.HideForm(),
                "Save", x =>
                {
                    Form localForm = x;

                    string launchOffline = localForm.GetValue("Always launch offline")!;
                    string skipVersionCheck = localForm.GetValue("Always skip version check")!;
                    string args = localForm.GetValue("Additional game arguments")!;
                    
                    LegendaryGame legendaryGame = localForm.Game as LegendaryGame;

                    legendaryGame!.ConfigAlwaysOffline = launchOffline == "1";
                    legendaryGame.ConfigAlwaysSkipUpdateCheck = skipVersionCheck == "1";
                    legendaryGame.ConfigAdditionalGameArgs = args;
                    legendaryGame.Parser.SaveConfig();
                    
                    LegendaryGameSource.Source.App.HideForm();
                })
        });
        
        f.Game = game;
        return f;
    }
}