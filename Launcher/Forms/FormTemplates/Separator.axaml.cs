using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LauncherGamePlugin.Forms;

namespace Launcher.Forms.FormTemplates;

public partial class Separator : UserControl
{
    public Separator()
    {
        InitializeComponent();
    }

    public Separator(FormEntry formEntry) : this()
    {
        if (!string.IsNullOrWhiteSpace(formEntry.Value))
            Border.Height = int.Parse(formEntry.Value);
    }
}