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
    HorizontalPanel,
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

public class TextInputElement : FormEntry
{
    public TextInputElement(string name = "", string value = "", FormAlignment alignment = FormAlignment.Default,
        bool enabled = true) : base(name, value, alignment, enabled)
    {
        
    }   
}

public class TextBoxElement : FormEntry
{
    public TextBoxElement(string name = "", string fontWeight = "", FormAlignment alignment = FormAlignment.Default,
        bool enabled = true) : base(name, fontWeight, alignment, enabled)
    {
        
    }
}

public class ClickableLinkBoxElement : FormEntry
{
    public Action<FormEntry> LinkClick { get; set; }
    
    public ClickableLinkBoxElement(string name = "", string value = "", Action<FormEntry> linkClick = null, FormAlignment alignment = FormAlignment.Default,
        bool enabled = true)
        : base(name, value, alignment, enabled)
    {
        LinkClick = linkClick;
    }
}

public class ToggleElement : FormEntry
{
    public ToggleElement(string name = "", string value = "", FormAlignment alignment = FormAlignment.Default,
        bool enabled = true) : base(name, value, alignment, enabled)
    {
        
    }
}

public class FilePickerElement : FormEntry
{
    public FilePickerElement(string name = "", string value = "", FormAlignment alignment = FormAlignment.Default,
        bool enabled = true) : base(name, value, alignment, enabled)
    {
        
    }   
}

public class FolderPickerElement : FormEntry
{
    public FolderPickerElement(string name = "", string value = "", FormAlignment alignment = FormAlignment.Default,
        bool enabled = true) : base(name, value, alignment, enabled)
    {
        
    }
}

public class DropdownElement : FormEntry
{
    public List<string> DropdownOptions { get; set; }
    
    public DropdownElement(string name = "", string value = "", List<string> dropdownOptions = null, FormAlignment alignment = FormAlignment.Default,
        bool enabled = true)
        : base(name, value, alignment, enabled)
    {
        DropdownOptions = dropdownOptions ?? new();
    }
}

public class ButtonListElement : FormEntry
{
    public List<ButtonEntry> Buttons { get; set; }
    
    public ButtonListElement(List<ButtonEntry> buttons = null, FormAlignment alignment = FormAlignment.Default,
        bool enabled = true)
        : base(null, null, alignment, enabled)
    {
        Buttons = buttons ?? new();
        
        if (Alignment == FormAlignment.Default)
            Alignment = FormAlignment.Center;
    }
}

public class SeperatorElement : FormEntry
{
    public SeperatorElement(string name = "", string value = "", FormAlignment alignment = FormAlignment.Default,
        bool enabled = true) : base(name, value, alignment, enabled)
    {
        
    }
}

public class ImageElement : FormEntry
{
    public Func<Task<byte[]?>> GetImage { get; set; }
    public Action<FormEntry> Click { get; set; }
    
    public ImageElement(string name = "", string value = "", Func<Task<byte[]?>> getImage = null, Action<FormEntry> click = null, FormAlignment alignment = FormAlignment.Default,
        bool enabled = true)
        : base(name, value, alignment, enabled)
    {
        GetImage = getImage;
        Click = click;
    }
}

public class HorizontalPanelElement : FormEntry
{
    public List<FormEntry> Entries { get; set; }

    public HorizontalPanelElement(string name = "", string value = "", List<FormEntry> entries = null, FormAlignment alignment = FormAlignment.Default,
        bool enabled = true)
        : base(name, value, alignment, enabled)
    {
        Entries = entries ?? new();
    }
}

public abstract class FormEntry
{
    public string Name { get; set; }
    public string Value { get; set; }
    public bool Enabled { get; set; }
    public Form ContainingForm { get; set; }
    public FormAlignment Alignment;
    public event Action<FormEntry>? OnChange;
    public void InvokeOnChange() => OnChange?.Invoke(this);

    public FormEntry(string name = "", string value = "", FormAlignment alignment = FormAlignment.Default, bool enabled = true)
    {
        Name = name;
        Value = value;
        Enabled = enabled;
        Alignment = alignment;
    }
}