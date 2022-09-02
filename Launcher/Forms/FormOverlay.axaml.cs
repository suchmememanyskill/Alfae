using System.IO;
using Avalonia.Controls;
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

    public FormOverlay(Form form) : this() => SetNewForm(form);
    public void SetNewForm(Form form)
    {
        _form = form;
        StackPanel.Children.Clear();
        form.FormEntries.ForEach(x => StackPanel.Children.Add(x.ToTemplatedControl()));

        if (form.Background != null)
            Dispatcher.UIThread.Post(SetBackground);
        else
            Image.Source = null;
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