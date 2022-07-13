using Avalonia.Controls;
using LauncherGamePlugin.Forms;

namespace Launcher.Forms.FormTemplates;

public partial class TextInput : UserControl
{
    private FormEntry _formEntry;

    public TextInput()
    {
        InitializeComponent();
    }
    
    public TextInput(FormEntry formEntry) : this()
    {
        _formEntry = formEntry;
        Label.Content = _formEntry.Name;
        TextBox.Text = _formEntry.Value;
        TextBox.KeyUp += (_, _) =>
        {
            _formEntry.Value = TextBox.Text;
            _formEntry.InvokeOnChange();
        };
    }
}