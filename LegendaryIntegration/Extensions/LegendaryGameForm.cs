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
            new FormEntry(FormEntryType.TextBox, "Legendary game configuration and info", "Bold", alignment: FormAlignment.Center),
            new FormEntry(FormEntryType.TextBox, $"{game.Name} by {game.Developer}"),
            new FormEntry(FormEntryType.TextBox, $"AppId: {game.InternalName}"),
            new FormEntry(FormEntryType.TextBox, $"Current Version: {game.InstalledVersion}"),
            new FormEntry(FormEntryType.ClickableLinkBox, $"Location on disk: {game.InstallPath}", game.InstallPath, linkClick: x => Utils.OpenFolder(x.Value)),
            new FormEntry(FormEntryType.TextBox, "\nConfig", "Bold", alignment: FormAlignment.Center),
            new FormEntry(FormEntryType.Toggle, "Always launch offline", game.ConfigAlwaysOffline ? "1" : "0"),
            new FormEntry(FormEntryType.Toggle, "Always skip version check", game.ConfigAlwaysSkipUpdateCheck ? "1" : "0"),
            new FormEntry(FormEntryType.TextInput, "Additional game arguments", game.ConfigAdditionalGameArgs),
            new FormEntry(FormEntryType.ButtonList, buttonList: new()
            {
                {"Back", x => LegendaryGameSource.Source.App.HideForm()},
                {"Save", x =>
                {
                    Form localForm = x.ContainingForm;

                    string launchOffline = localForm.GetValue("Always launch offline")!;
                    string skipVersionCheck = localForm.GetValue("Always skip version check")!;
                    string args = localForm.GetValue("Additional game arguments")!;
                    
                    LegendaryGame legendaryGame = localForm.Game as LegendaryGame;

                    legendaryGame!.ConfigAlwaysOffline = launchOffline == "1";
                    legendaryGame.ConfigAlwaysSkipUpdateCheck = skipVersionCheck == "1";
                    legendaryGame.ConfigAdditionalGameArgs = args;
                    legendaryGame.Parser.SaveConfig();
                    
                    LegendaryGameSource.Source.App.HideForm();
                }}
            })
        });

        f.Background = game.BackgroundImage;
        f.Game = game;
        return f;
    }
}