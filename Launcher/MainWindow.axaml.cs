using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Threading;
using Launcher.Utils;
using Launcher.Views;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;

namespace Launcher
{
    public partial class MainWindow : Window
    {
        private Loader.App _app;
        
        public MainWindow()
        {
            InitializeComponent();
            _app = Loader.App.GetInstance();
            _app.MainWindow = this;
            Dispatcher.UIThread.Post(Initialize);
            Content = new LoadingScreen();
            Title = $"Alfae {Loader.App.Version}: A Launcher For Almost Everything";
        }

        private async void CheckForUpdate()
        {
            string? gitVersion = await NewVersionCheck.GetGitVersion();
            if (gitVersion != null && gitVersion != NewVersionCheck.Version)
            {
                string ignoreVersion = _app.Config.IgnoreVersion;
                if (ignoreVersion != gitVersion)
                {
                    _app.ShowForm(new List<FormEntry>()
                    {
                        Form.TextBox($"Alfae has an update available, v{gitVersion}. Would you like to update?", FormAlignment.Center),
                        Form.Button(
                            "Back", x => _app.HideForm(),
                            "Ignore this update", x =>
                            {
                                _app.Config.IgnoreVersion = gitVersion;
                                _app.Config.Save(_app);
                                _app.HideForm();
                            },
                            "Open webpage to new version", x => LauncherGamePlugin.Utils.OpenUrl("https://github.com/suchmememanyskill/Alfae/releases")
                        )
                    });
                }
            }
        }
        
        private async void Initialize()
        {
            _app.HeadlessMode = false;
            await _app.InitializeGameSources();
            _app.MainView = new MainView();
            Content = _app.MainView;
            await _app.ReloadGames2Task();
            if (_app.HasStartForm())
            {
                _app.ShowPossibleStartForm();
            }
            else
            {
               CheckForUpdate();
            }
        }
    }
}
