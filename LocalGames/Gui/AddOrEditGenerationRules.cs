using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LocalGames.Data;

namespace LocalGames.Gui;

public class AddOrEditGenerationRules
{
    private IApp _app;
    private LocalGameSource _instance;

    public AddOrEditGenerationRules(IApp app, LocalGameSource instance)
    {
        _app = app;
        _instance = instance;
    }

    public void ShowGui(string name = "", List<string>? extensions = null, string localGameName = "", string additionalCliArgs = "", string path = "", bool? drillDown = null, GenerationRules? rules = null, string errMessage = "")
    {
        extensions ??= new();

        if (rules != null)
        {
            if (name == "")
                name = rules.Name;

            if (extensions.Count <= 0)
                extensions = rules.Extensions;

            if (localGameName == "")
                localGameName = rules.LocalGameName;

            if (additionalCliArgs == "")
                additionalCliArgs = rules.AdditionalCliArgs;

            if (path == "")
                path = rules.Path;

            if (drillDown == null)
                drillDown = rules.DrillDown;
        }

        drillDown ??= false;

        List<FormEntry> elements = new()
        {
            Form.TextBox((rules == null) ? "Add generation rule" : $"Edit generation rule '{name}'", FormAlignment.Center,
                "Bold"),
            Form.TextBox(
                "Generation rules is a feature that scans a folder for files with specific extensions, and generates playable applications out of them using an already existing local game. Useful for emulation"),
            Form.Separator(),
            Form.TextInput("Name: ", name),
            Form.FolderPicker("Folder: ", path),
            Form.Toggle("Recursively search folder", drillDown!.Value),
            Form.Dropdown("Base game: ", _instance.Games.Select(x => x.Name).ToList(), localGameName),
            Form.Separator(),
            Form.TextBox("Extensions are provided in a csv format. Example: '.png, .jpg, .gif'"),
            Form.TextInput("Valid Extensions: ", string.Join(", ", extensions)),
            Form.Separator(),
            Form.TextBox(
                "Additional CLI args will be put after the base game's cli args, with a space in between. '{EXEC}' will be replaced with a file path."),
            Form.TextInput("Additional CLI Args: ", additionalCliArgs),
            Form.Separator(),
            Form.Button("Back", _ => _app.HideForm(), "Save", x => Save(x, rules))
        };

        if (rules != null)
        {
            elements.Last().ButtonList.Add(new($"Delete {name}", _ => Delete(rules)));
        }
        
        if (errMessage != "")
            elements.Add(Form.TextBox(errMessage, FormAlignment.Center, "Bold"));
        
        _app.ShowForm(elements);
    }

    private void Save(Form form, GenerationRules? rules)
    {
        _app.ShowTextPrompt("Adding generation rule...");
        string name = form.GetValue("Name: ")!;
        string folder = form.GetValue("Folder: ")!;
        string baseGame = form.GetValue("Base game: ")!;
        string extensions = form.GetValue("Valid Extensions: ")!;
        string cliArgs = form.GetValue("Additional CLI Args: ")!;
        string drillDownStr = form.GetValue("Recursively search folder")!;

        bool drillDown = drillDownStr == "1";

        if (string.IsNullOrWhiteSpace(extensions))
        {
            ShowGui(name, new(), baseGame, cliArgs, folder, drillDown, rules, "Extensions list cannot be empty!");
            return;
        }
        
        List<string> splitExtensions = extensions.Split(",").Select(x => x.Trim()).ToList();
        
        if (string.IsNullOrWhiteSpace(name))
        {
            ShowGui(name, splitExtensions, baseGame, cliArgs, folder, drillDown, rules, "Name cannot be empty!");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            ShowGui(name, splitExtensions, baseGame, cliArgs, folder, drillDown, rules, "Folder cannot be empty and must exist!");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(baseGame) || _instance.Games.All(x => x.Name != baseGame))
        {
            ShowGui(name, splitExtensions, baseGame, cliArgs, folder, drillDown, rules, "Base Game is invalid!");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(cliArgs) || !cliArgs.Contains("{EXEC}"))
        {
            ShowGui(name, splitExtensions, baseGame, cliArgs, folder, drillDown, rules, "Cli Args cannot be empty and must contain '{EXEC}'!");
            return;
        }

        if ((rules == null || rules.Name != name) && _instance.Rules.Any(x => x.Name == name))
        {
            ShowGui(name, splitExtensions, baseGame, cliArgs, folder, drillDown, rules, "Name already exists!");
        }

        GenerationRules newRules = rules ?? new();

        newRules.Name = name;
        newRules.Extensions = splitExtensions;
        newRules.Path = folder;
        newRules.AdditionalCliArgs = cliArgs;
        newRules.LocalGameName = baseGame;
        newRules.DrillDown = drillDown;
        
        if (!_instance.Rules.Contains(newRules))
            _instance.Rules.Add(newRules);
        
        _instance.Save();
        _app.HideForm();
        _app.ReloadGames();
    }

    private void Delete(GenerationRules rules)
    {
        if (_instance.Rules.Contains(rules))
            _instance.Rules.Remove(rules);
        
        _app.HideForm();
        _instance.Save();
        _app.ReloadGames();
    }
}