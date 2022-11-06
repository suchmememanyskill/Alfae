using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Launcher.Extensions;
using LauncherGamePlugin.Forms;

namespace Launcher.Forms.FormTemplates;

public partial class ImageView : UserControl
{
    private FormEntry _formEntry;
    
    public ImageView()
    {
        InitializeComponent();
    }
    
    public ImageView(FormEntry formEntry) : this()
    {
        _formEntry = formEntry;
        
        StackPanel.HorizontalAlignment = formEntry.Alignment.ToAvaloniaAlignment();
        
        if (string.IsNullOrEmpty(formEntry.Name))
            Label.IsVisible = false;
        else 
            Label.Content = formEntry.Name;

        if (formEntry.LinkClick != null)
            StackPanel.PointerPressed += (sender, args) => formEntry.LinkClick(formEntry);
        
        SetImage();
    }

    public async void SetImage()
    {
        byte[]? image = await _formEntry.GetImage();

        if (image != null)
        {
            MemoryStream stream = new(image);
            Image.Source = Bitmap.DecodeToHeight(stream, 300);
        }
    }
}