using LauncherGamePlugin.Interfaces;

namespace LauncherGamePlugin.Forms;

public class Form
{
    public List<FormEntry> FormEntries { get; set; }
    public void SetContainingForm() => FormEntries.ForEach(x => x.ContainingForm = this);

    public Func<Task<byte[]?>>? Background { get; set; } = null;
    public IGame? Game { get; set; } = null;

    public Form(List<FormEntry> entries)
    {
        FormEntries = entries;
        SetContainingForm();
    }

    public string? GetValue(string name) => FormEntries.Find(x => x.Name == name)?.Value;

    public static Form Create2ButtonTextPrompt(string text, string button1, string button2,
        Action<FormEntry> buttonAction1, Action<FormEntry> buttonAction2)
    {
        return new(new()
        {
            new(FormEntryType.TextBox, text, alignment: FormAlignment.Center),
            new(FormEntryType.ButtonList, buttonList: new()
            {
                {button1, buttonAction1},
                {button2, buttonAction2}
            })
        });
    }

    public static Form CreateTextPrompt(string text)
    {
        return new(new()
        {
            new(FormEntryType.TextBox, text)
        });
    }
}

public static class FormExtensions
{
    public static void Show2ButtonTextPrompt(this IApp app, string text, string button1, string button2,
        Action<FormEntry> buttonAction1, Action<FormEntry> buttonAction2, IGame? game = null)
    {
        Form f = Form.Create2ButtonTextPrompt(text, button1, button2, buttonAction1, buttonAction2);
        if (game != null)
            f.Game = game;

        app.ShowForm(f);
    }

    public static void ShowTextPrompt(this IApp app, string text) => app.ShowForm(Form.CreateTextPrompt(text));
}