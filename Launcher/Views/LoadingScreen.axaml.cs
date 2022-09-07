using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Launcher.Extensions;
using LauncherGamePlugin.Forms;

namespace Launcher.Views;

public partial class LoadingScreen : UserControl
{
    public LoadingScreen()
    {
        InitializeComponent();
        var app = Loader.App.GetInstance();
        AddText("Initialising...");
        app.OnPluginInitialised += source => AddText($"Initialised plugin {source.ServiceName}");
    }

    public void AddText(string text)
    {
        Overlay.StackPanel.Children.Add(Form.TextBox(text, FormAlignment.Center).ToTemplatedControl());
    }
}