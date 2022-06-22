namespace LauncherGamePlugin.Forms;

public enum FormEntryType
{
    TextInput,
    TextBox,
    ClickableLinkBox,
    Toggle,
    FilePicker,
    FolderPicker,
    Dropdown,
    ButtonList,
}

public enum FormAlignment
{
    Center,
    Left,
    Right,
    Default,
}

public class FormEntry
{
    public string Name { get; set; }
    public string Value { get; set; }
    public FormEntryType Type { get; set; }
    public Action<FormEntry> LinkClick { get; set; }
    public List<string> DropdownOptions { get; set; }
    public Dictionary<string, Action<FormEntry>> ButtonList { get; set; }
    public Form ContainingForm { get; set; }
    public FormAlignment Alignment;

    public FormEntry(FormEntryType type, string name = "", string value = "", List<string> dropdownOptions = null,
        Dictionary<string, Action<FormEntry>> buttonList = null, Action<FormEntry> linkClick = null, FormAlignment alignment = FormAlignment.Default)
    {
        Type = type;
        Name = name;
        Value = value;
        DropdownOptions = dropdownOptions;
        ButtonList = buttonList;
        LinkClick = linkClick;
        Alignment = alignment;

        if (Type == FormEntryType.ButtonList && Alignment == FormAlignment.Default)
            Alignment = FormAlignment.Center;
    }
}