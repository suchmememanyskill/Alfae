using Avalonia.Controls;
using Avalonia.Threading;
using Launcher.Views;

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
            app.HeadlessMode = false;
            await app.InitializeGameSources();
            app.MainView = new MainView();
            Content = app.MainView;
            await app.ReloadGames2Task();
            app.ShowPossibleStartForm();
        }
    }
}
