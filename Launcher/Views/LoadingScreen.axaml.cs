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
        app.OnPluginInitialised += (g, t) => AddText($"Initialised plugin {g.ServiceName} in {(((float)t) / 1000):0.0}s");
    }

    public void AddText(string text)
    {
        Overlay.StackPanel.Children.Add(Form.TextBox(text, FormAlignment.Center).ToTemplatedControl());
    }
}