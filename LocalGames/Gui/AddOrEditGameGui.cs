using LauncherGamePlugin;
using LauncherGamePlugin.Extensions;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LocalGames.Data;

namespace LocalGames.Gui;

public class AddOrEditGameGui
{
    private IApp _app;
    private LocalGameSource _instance;

    public AddOrEditGameGui(IApp app, LocalGameSource instance)
    {
        _app = app;
        _instance = instance;
    }
    
    public void ShowGui(string possibleWarn = "", string gameName = "", string execPath = "", string coverImage = "", string backgroundImage = "", string args = "", string workingDirectory = "", LocalGame? game = null)
    {
        string addOrEdit = game == null ? "Add" : "Edit";

        if (game != null)
        {
            if (gameName == "")
                gameName = game.Name;

            if (execPath == "")
                execPath = game.ExecPath;

            if (coverImage == "")
                coverImage = game.CoverImagePath ?? "";

            if (backgroundImage == "")
                backgroundImage = game.BackgroundImagePath ?? "";

            if (args == "")
                args = game.LaunchArgs ?? "";

            if (workingDirectory == "")
                workingDirectory = game.WorkingDirectory ?? "";
        }

        List<FormEntry> entries = new()
        {
            Form.TextBox($"{addOrEdit} a local game", fontWeight: "Bold"),
            Form.TextInput("Game name:", gameName),
            Form.FilePicker("Game executable:", execPath),
            Form.TextBox("\nOptional", fontWeight: "Bold"),
            Form.FolderPicker("Working Directory:", workingDirectory),
            Form.FilePicker("Cover Image:", coverImage),
            Form.FilePicker("Background Image:", backgroundImage),
            Form.TextInput("CLI Arguments:", args),
            Form.Button("Cancel", _ => _app.HideForm(), addOrEdit, entry =>
            {
                new Thread(() => AddGame(entry)).Start();
            })
        };
        
        if (possibleWarn != "")
            entries.Add(Form.TextBox(possibleWarn, fontWeight: "Bold"));
        
        Form form = new(entries);
        if (game != null)
        {
            form.Game = game;
        }
        _app.ShowForm(form);
    }
    
    private void AddGame(Form form)
    {
        string? gameName = form.GetValue("Game name:");
        string? execPath = form.GetValue("Game executable:");
        string? coverImage = form.GetValue("Cover Image:");
        string? backgroundImage = form.GetValue("Background Image:");
        string? args = form.GetValue("CLI Arguments:");
        string? workingDirectory = form.GetValue("Working Directory:")?.Trim();
        string errMessage = "";
        
        if (string.IsNullOrWhiteSpace(gameName))
            errMessage = "Please fill in the game name";

        if (string.IsNullOrWhiteSpace(execPath) && errMessage == "")
            errMessage = "Please fill in the executable path";

        if (!File.Exists(execPath) && errMessage == "")
            errMessage = "Executable path does not exist!";

        if (errMessage == "" && coverImage != "" && !File.Exists(coverImage))
            errMessage = "Cover image path does not exist!";

        if (errMessage == "" && coverImage != "" && !File.Exists(backgroundImage))
            errMessage = "Background image path does not exist!";

        if (errMessage == "" && !string.IsNullOrWhiteSpace(workingDirectory) && !Directory.Exists(workingDirectory))
            errMessage = "Working directory does not exist";

        if (errMessage != "")
        {
            ShowGui(errMessage, gameName, execPath,coverImage, backgroundImage, args, workingDirectory, form.Game as LocalGame);
            return;
        }
        
        _instance.Log($"Calculating game {gameName} size at path {execPath}");
        _app.ShowTextPrompt($"Processing {gameName}...");
        LocalGame localGame;

        if (form.Game == null)
        {
            localGame = new LocalGame();
            localGame.InternalName = Guid.NewGuid().ToString();
        }
        else
            localGame = (form.Game as LocalGame)!;

        localGame.Name = gameName;
        localGame.ExecPath = execPath;
        localGame.WorkingDirectory = workingDirectory;
        localGame.Size = 0;
        
        try
        {
            localGame.Size = Utils.DirSize(new DirectoryInfo(localGame.InstalledPath));
        }
        catch (Exception e)
        {
            _app.Logger.Log($"Failed to retrieve folder size for new local game: {e.Message}", service: "LocalGames", type: LogType.Warn);
        }
        
        localGame.CoverImagePath = coverImage;
        localGame.BackgroundImagePath = backgroundImage;
        localGame.LaunchArgs = args;
        _instance.Log($"{gameName}'s size is {localGame.ReadableSize()}");
        
        if (form.Game == null)
        {
            _instance.Games.Add(localGame);
            _instance.Log($"Added game {gameName}");
        }

        _app.ReloadGames();
        _instance.Save();
        _app.HideForm();
    }
}