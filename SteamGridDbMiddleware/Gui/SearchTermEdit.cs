using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;

namespace SteamGridDbMiddleware.Gui;

public class SearchTermEdit
{
    public event Action<string>? OnSubmit;
    private IApp _app;
    private string _gameName;

    public SearchTermEdit(IApp app, string gameName)
    {
        _app = app;
        _gameName = gameName;
    }

    public void ShowGui(string err = "")
    {
        List<FormEntry> items = new()
        {
            Form.TextBox($"Enter new search term for '{_gameName}'", fontWeight: "Bold"),
            Form.TextInput("Search term: ", _gameName),
            Form.Button("Back", _ => Back(), "Search", x => Validate(x.GetValue("Search term: ")))
        };
        
        if (err != "")
            items.Add(Form.TextBox(err, FormAlignment.Center, "Bold"));
        
        _app.ShowForm(items);
    }

    private void Validate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            ShowGui("Search cannot be empty");
            return;
        }

        if (text == _gameName)
        {
            ShowGui("The search term is the same as the current one");
            return;
        }
        
        _app.HideForm();
        OnSubmit?.Invoke(text);
    }

    private void Back()
    {
        _app.HideForm();
        OnSubmit?.Invoke(_gameName);
    }
}