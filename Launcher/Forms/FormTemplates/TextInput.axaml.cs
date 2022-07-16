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
        TextBox.PropertyChanged += (_, _) => Update();
    }

    private void Update()
    {
        if (_formEntry.Value == TextBox.Text) 
            return;
        
        _formEntry.Value = TextBox.Text;
        _formEntry.InvokeOnChange();
    }
}