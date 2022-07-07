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
        Action<Form> buttonAction1, Action<Form> buttonAction2)
    {
        return new(new()
        {
            TextBox(text, FormAlignment.Center),
            Button(button1, buttonAction1, button2, buttonAction2)
        });
    }

    public static Form CreateTextPrompt(string text)
    {
        return new(new()
        {
            TextBox(text, FormAlignment.Center)
        });
    }

    public static Form CreateDismissibleTextPrompt(string text, IApp app)
    {
        return new(new()
        {
            TextBox(text, FormAlignment.Center),
            Button("Back", x => app.HideForm())
        });
    }

    public static FormEntry TextInput(string label, string value = "") => new(FormEntryType.TextInput, label, value);

    public static FormEntry TextBox(string text, FormAlignment alignment = FormAlignment.Default,
        string fontWeight = "")
        => new(FormEntryType.TextBox, text, fontWeight, alignment: alignment);

    public static FormEntry ClickableLinkBox(string text, Action<Form> action,
        FormAlignment alignment = FormAlignment.Default, string fontWeight = "")
        => new(FormEntryType.ClickableLinkBox, text, alignment: alignment, linkClick: x => action(x.ContainingForm), value: fontWeight);

    public static FormEntry Toggle(string label, bool value, FormAlignment alignment = FormAlignment.Default)
        => new(FormEntryType.Toggle, label, value ? "1" : "0", alignment: alignment);

    public static FormEntry FilePicker(string label, string value = "")
        => new(FormEntryType.FilePicker, label, value);

    public static FormEntry FolderPicker(string label, string value = "")
        => new(FormEntryType.FolderPicker, label, value);

    public static FormEntry Dropdown(string label, List<string> dropdownOptions, string value = "")
        => new(FormEntryType.Dropdown, label, value, dropdownOptions: dropdownOptions);

    public static FormEntry ButtonList(List<ButtonEntry> buttons, FormAlignment alignment = FormAlignment.Default)
        => new(FormEntryType.ButtonList, buttonList: buttons, alignment: alignment);

    public static FormEntry Button(string label, Action<Form> action, FormAlignment alignment = FormAlignment.Default)
        => ButtonList(new() {new(label, action)}, alignment);

    public static FormEntry Button(string label1, Action<Form> action1, string label2, Action<Form> action2, FormAlignment alignment = FormAlignment.Default)
        => ButtonList(new() {new(label1, action1), new(label2, action2)}, alignment);
    
    public static FormEntry Button(string label1, Action<Form> action1, string label2, Action<Form> action2, string label3, Action<Form> action3, FormAlignment alignment = FormAlignment.Default)
        => ButtonList(new() {new(label1, action1), new(label2, action2), new(label3, action3)}, alignment);
}

public static class FormExtensions
{
    public static void Show2ButtonTextPrompt(this IApp app, string text, string button1, string button2,
        Action<Form> buttonAction1, Action<Form> buttonAction2, IGame? game = null)
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