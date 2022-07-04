using Avalonia.Controls;
using Avalonia.Threading;
using Launcher.Utils;
using Launcher.Views;
using LauncherGamePlugin.Forms;

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
            if (app.HasStartForm())
            {
                app.ShowPossibleStartForm();
            }
            else
            {
                string? gitVersion = await NewVersionCheck.GetGitVersion();
                if (gitVersion != null && gitVersion != NewVersionCheck.Version)
                {
                    app.Show2ButtonTextPrompt($"Launcher has an update available, v{gitVersion}. Would you like to update?", "Back", "Update", x => app.HideForm(), x => LauncherGamePlugin.Utils.OpenUrl("https://github.com/suchmememanyskill/Launcher/releases"));
                }
            }
        }
    }
}
