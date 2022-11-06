namespace LauncherGamePlugin.Forms;

public enum FormEntryType
{
    TextInput, // Displays Name on the left side. The rest of the space is filled with a text input. Value is the current value of the input, and can be set beforehand for a default value
    TextBox, // Displays Name. Value gets used as the font weight enum. Respects FormAlignment
    ClickableLinkBox, // Displays Name. Runs LinkClick when pressed. Respects FormAlignment
    Toggle, // Displays Name on the left side of the toggle. Sets "1" in Value if the toggle is toggled, otherwise "0". Respects FormAlignment
    FilePicker, // Works the same as the TextInput, but with a browse button
    FolderPicker, // Works the same as the TextInput, but with a browse button
    Dropdown, // Displays Name on the left side. Makes a dropdown out of DropdownOptions. Value is the current value of the dropdown. Can be set beforehand to set a default value
    ButtonList, // Displays a horizontal list of buttons. Uses ButtonList to give names and Actions to the buttons. Respects FormAlignment
    Separator,
    Image,
}

public enum FormAlignment
{
    Center,
    Left,
    Right,
    Default,
}

public class ButtonEntry
{
    public string Name { get; set; }
    public Action<Form> Action { get; set; }

    public ButtonEntry(string name, Action<Form> action)
    {
        Name = name;
        Action = action;
    }
}

public class FormEntry
{
    public string Name { get; set; }
    public string Value { get; set; }
    public FormEntryType Type { get; set; }
    public Action<FormEntry> LinkClick { get; set; }
    public List<string> DropdownOptions { get; set; }
    public List<ButtonEntry> ButtonList { get; set; }
    public Form ContainingForm { get; set; }
    public Func<Task<byte[]?>> GetImage { get; set; }
    public FormAlignment Alignment;
    public event Action<FormEntry>? OnChange;
    public void InvokeOnChange() => OnChange?.Invoke(this);
    
    public FormEntry(FormEntryType type, string name = "", string value = "", List<string> dropdownOptions = null,
        List<ButtonEntry> buttonList = null, Action<FormEntry> linkClick = null, Func<Task<byte[]?>> image = null, FormAlignment alignment = FormAlignment.Default)
    {
        Type = type;
        Name = name;
        Value = value;
        DropdownOptions = dropdownOptions;
        ButtonList = buttonList;
        LinkClick = linkClick;
        Alignment = alignment;
        GetImage = image;

        if (Type == FormEntryType.ButtonList && Alignment == FormAlignment.Default)
            Alignment = FormAlignment.Center;
    }
}