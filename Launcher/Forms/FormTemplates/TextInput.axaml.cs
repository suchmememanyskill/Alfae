using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
        TextBox.TextInput += (_, _) => _formEntry.Value = TextBox.Text;
    }
}