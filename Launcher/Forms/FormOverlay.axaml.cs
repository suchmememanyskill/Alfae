using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Launcher.Extensions;
using LauncherGamePlugin.Forms;

namespace Launcher.Forms;

public partial class FormOverlay : UserControl
{
    private Form _form;
    
    public FormOverlay()
    {
        InitializeComponent();
    }

    public FormOverlay(Form form) : this()
    {
        _form = form;
        form.FormEntries.ForEach(x => StackPanel.Children.Add(x.ToTemplatedControl()));
        
        if (form.Background != null)
            Dispatcher.UIThread.Post(SetBackground);
    }

    private async void SetBackground()
    {
        try
        {
            byte[]? bytes = (await _form.Background?.Invoke());
            if (bytes == null)
                throw new InvalidDataException();
            
            Stream stream = new MemoryStream(bytes);
            Image.Source = new Bitmap(stream);
        }
        catch
        {
            Loader.App.GetInstance().Logger.Log("Failed to get background image for form");
        }
    }
}