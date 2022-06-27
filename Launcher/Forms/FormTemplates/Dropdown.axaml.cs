using Avalonia.Controls;
using LauncherGamePlugin.Forms;

namespace Launcher.Forms.FormTemplates;

public partial class Dropdown : UserControl
{
    public Dropdown()
    {
        InitializeComponent();
    }

    public Dropdown(FormEntry formEntry) : this()
    {
        ComboBox.Items = formEntry.DropdownOptions;
        Label.Content = formEntry.Name;
        int idx = formEntry.DropdownOptions.FindIndex(x => x == formEntry.Value);
        if (idx < 0)
            idx = 0;
        ComboBox.SelectedIndex = idx;
        ComboBox.SelectionChanged += (_, _) => formEntry.Value = formEntry.DropdownOptions[ComboBox.SelectedIndex];
    }
}