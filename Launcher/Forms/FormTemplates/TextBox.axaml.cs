using System;
using Avalonia.Controls;
using Avalonia.Media;
using Launcher.Extensions;
using LauncherGamePlugin.Forms;

namespace Launcher.Forms.FormTemplates;

public partial class TextBox : UserControl
{
    public TextBox()
    {
        InitializeComponent();
    }

    public TextBox(FormEntry entry) : this()
    {
        TextBlock.Text = entry.Name;
        TextBlock.HorizontalAlignment = entry.Alignment.ToAvaloniaAlignment();
        if (!string.IsNullOrWhiteSpace(entry.Value))
            TextBlock.FontWeight = (FontWeight)Enum.Parse(typeof(FontWeight), entry.Value);
    }
    
}