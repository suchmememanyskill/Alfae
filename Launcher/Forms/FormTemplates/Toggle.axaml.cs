using Avalonia.Controls;
using Launcher.Extensions;
using LauncherGamePlugin.Forms;

namespace Launcher.Forms.FormTemplates;

public partial class Toggle : UserControl
{
    public Toggle()
    {
        InitializeComponent();
    }

    public Toggle(FormEntry entry) : this()
    {
        ToggleSwitch.OffContent = entry.Name;
        ToggleSwitch.OnContent = entry.Name;
        ToggleSwitch.IsChecked = int.Parse(entry.Value) != 0;
        ToggleSwitch.Checked += (_, _) => entry.Value = "1";
        ToggleSwitch.Unchecked += (_, _) => entry.Value = "0";
        ToggleSwitch.HorizontalAlignment = entry.Alignment.ToAvaloniaAlignment();
    }
}