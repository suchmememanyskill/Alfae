using Avalonia.Controls;
using Launcher.Extensions;
using Launcher.Utils;
using LauncherGamePlugin.Forms;

namespace Launcher.Forms.FormTemplates;

public partial class ButtonList : UserControl
{
    private FormEntry _formEntry;
    
    public ButtonList()
    {
        InitializeComponent();
    }

    public ButtonList(FormEntry formEntry) : this()
    {
        _formEntry = formEntry;

        foreach (var (key, value) in _formEntry.ButtonList)
        {
            Button b = new Button();
            b.Content = key;
            b.Command = new LambdaCommand(x => value.Invoke(_formEntry.ContainingForm));
            StackPanel.Children.Add(b);
        }

        StackPanel.HorizontalAlignment = formEntry.Alignment.ToAvaloniaAlignment();
    }
}