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
        ToggleSwitch.IsEnabled = entry.Enabled;
        ToggleSwitch.IsChecked = int.Parse(entry.Value) != 0;
        ToggleSwitch.Checked += (_, _) =>
        {
            entry.Value = "1";
            entry.InvokeOnChange();
        };
        ToggleSwitch.Unchecked += (_, _) =>
        {
            entry.Value = "0";
            entry.InvokeOnChange();
        };
        ToggleSwitch.HorizontalAlignment = entry.Alignment.ToAvaloniaAlignment();
    }
}