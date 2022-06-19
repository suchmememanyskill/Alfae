using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using Launcher.Loader;
using LauncherGamePlugin.Interfaces;

namespace Launcher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            List<IGameSource> sources = PluginLoader.GetGameSources();
            Loader.App app = new Loader.App();
            sources.ForEach(x =>
            {
                Debug.WriteLine($"Initialising {x.ServiceName}...");
                x.Initialize(app);
            });
        }
    }
}
