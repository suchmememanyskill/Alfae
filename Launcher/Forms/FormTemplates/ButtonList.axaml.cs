using Avalonia.Controls;
using Launcher.Extensions;
using Launcher.Utils;
using LauncherGamePlugin.Forms;

namespace Launcher.Forms.FormTemplates;

public partial class ButtonList : UserControl
{
    private ButtonListElement _formEntry;
    
    public ButtonList()
    {
        InitializeComponent();
    }

    public ButtonList(ButtonListElement formEntry) : this()
    {
        _formEntry = formEntry;

        foreach (var x in _formEntry.Buttons)
        {
            Button b = new();
            b.Content = x.Name;
            if (x.Action == null)
                b.IsEnabled = false;
            else
                b.Command = new LambdaCommand(_ => x.Action.Invoke(_formEntry.ContainingForm));
            StackPanel.Children.Add(b);
        }

        StackPanel.HorizontalAlignment = formEntry.Alignment.ToAvaloniaAlignment();
    }
}