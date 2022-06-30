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
            new(FormEntryType.TextBox, text, alignment: FormAlignment.Center)
        });
    }

    public static Form CreateDismissibleTextPrompt(string text, IApp app)
    {
        return new(new()
        {
            new(FormEntryType.TextBox, text, alignment: FormAlignment.Center),
            new (FormEntryType.ButtonList, buttonList: new()
            {
                {"Back", x => app.HideForm()}
            })
        });
    }

    public static FormEntry TextInput(string label, string value = "") => new(FormEntryType.TextInput, label, value);

    public static FormEntry TextBox(string text, FormAlignment alignment = FormAlignment.Default,
        string fontWeight = "")
        => new(FormEntryType.TextBox, text, fontWeight, alignment: alignment);

    public static FormEntry ClickableLinkBox(string text, Action<Form> action,
        FormAlignment alignment = FormAlignment.Default)
        => new(FormEntryType.ClickableLinkBox, text, alignment: alignment, linkClick: x => action(x.ContainingForm));

    public static FormEntry Toggle(string label, bool value, FormAlignment alignment = FormAlignment.Default)
        => new(FormEntryType.Toggle, label, value ? "1" : "0", alignment: alignment);

    public static FormEntry FilePicker(string label, string value = "")
        => new(FormEntryType.FilePicker, label, value);

    public static FormEntry FolderPicker(string label, string value = "")
        => new(FormEntryType.FolderPicker, label, value);

    public static FormEntry Dropdown(string label, List<string> dropdownOptions, string value = "")
        => new(FormEntryType.Dropdown, label, value, dropdownOptions: dropdownOptions);

    public static FormEntry ButtonList(Dictionary<string, Action<Form>> buttons)
    {
        Dictionary<string, Action<FormEntry>> forms = new();

        foreach (var (key, value) in buttons)
        {
            forms.Add(key, x => value(x.ContainingForm));
        }

        return new(FormEntryType.ButtonList, buttonList: forms);
    }

    public static FormEntry Button(string label, Action<Form> action)
        => ButtonList(new() {{label, action}});

    public static FormEntry Button(string label1, Action<Form> action1, string label2, Action<Form> action2)
        => ButtonList(new() {{label1, action1}, {label2, action2}});
    
    public static FormEntry Button(string label1, Action<Form> action1, string label2, Action<Form> action2, string label3, Action<Form> action3)
        => ButtonList(new() {{label1, action1}, {label2, action2}, {label3, action3}});
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
    
    public static void ShowDismissibleTextPrompt(this IApp app, string text) => app.ShowForm(Form.CreateDismissibleTextPrompt(text, app));

    public static void ShowTextPrompt(this IApp app, string text) => app.ShowForm(Form.CreateTextPrompt(text));

    public static void ShowForm(this IApp app, List<FormEntry> entries) => app.ShowForm(new(entries));
}