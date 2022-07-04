using System;
using Avalonia.Controls;
using Avalonia.Media;
using Launcher.Extensions;
using Launcher.Utils;
using LauncherGamePlugin.Forms;

namespace Launcher.Forms.FormTemplates;

public partial class ClickableLinkBox : UserControl
{
    private FormEntry _formEntry;
    
    public ClickableLinkBox()
    {
        InitializeComponent();
    }

    public ClickableLinkBox(FormEntry formEntry) : this()
    {
        _formEntry = formEntry;
        Button.Content = formEntry.Name;
        Button.Command = new LambdaCommand(x => formEntry.LinkClick(formEntry));
        Button.HorizontalAlignment = formEntry.Alignment.ToAvaloniaAlignment();
        
        if (!string.IsNullOrWhiteSpace(formEntry.Value))
            Button.FontWeight = (FontWeight)Enum.Parse(typeof(FontWeight), formEntry.Value);
    }
}