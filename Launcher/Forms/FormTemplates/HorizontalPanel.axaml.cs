using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Launcher.Extensions;
using LauncherGamePlugin.Forms;

namespace Launcher.Forms.FormTemplates;

public partial class HorizontalPanel : UserControl
{
    public HorizontalPanel()
    {
        InitializeComponent();
    }

    public HorizontalPanel(FormEntry formEntry) : this()
    {
        Core.HorizontalAlignment = formEntry.Alignment.ToAvaloniaAlignment();
        StackPanel.Spacing = int.Parse(formEntry.Value);

        foreach (var entry in formEntry.HorizontalPanel)
        {
            StackPanel.Children.Add(entry.ToTemplatedControl());
        }
    }
}