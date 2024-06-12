using Avalonia.Controls;
using Launcher.Utils;
using LauncherGamePlugin.Forms;

namespace Launcher.Forms.FormTemplates;

public partial class Dropdown : UserControl
{
    public Dropdown()
    {
        InitializeComponent();
    }

    public Dropdown(DropdownElement formEntry) : this()
    {
        ComboBox.ItemsSource = formEntry.DropdownOptions;
        Label.Content = formEntry.Name;
        int idx = formEntry.DropdownOptions.FindIndex(x => x == formEntry.Value);
        if (idx < 0)
            idx = 0;
        ComboBox.SelectedIndex = idx;
        ComboBox.SelectionChanged += (_, _) =>
        {
            formEntry.Value = formEntry.DropdownOptions[ComboBox.SelectedIndex];
            formEntry.InvokeOnChange();
        };
        CycleButton.Command = new LambdaCommand(x =>
        {
            if (formEntry.DropdownOptions.Count > 0)
                ComboBox.SelectedIndex = (ComboBox.SelectedIndex + 1) % formEntry.DropdownOptions.Count;
        });
    }
}