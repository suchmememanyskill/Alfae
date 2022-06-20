using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Launcher.Loader;
using Launcher.Views;
using LauncherGamePlugin.Interfaces;

namespace Launcher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loader.App app = Loader.App.GetInstance();
            app.MainWindow = this;
            Dispatcher.UIThread.Post(Initialize);
            Content = new LoadingScreen();
        }

        private async void Initialize()
        {
            Loader.App app = Loader.App.GetInstance();
            await app.InitializeGameSources();
            app.MainView = new MainView();
            Content = app.MainView;
            await app.ReloadGames2Task();
        }
    }
}
