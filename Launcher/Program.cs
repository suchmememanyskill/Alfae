using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Extensions;
using LauncherGamePlugin.Interfaces;

namespace Launcher
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length <= 2)
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            else
                Start(args);
        }

        public static void Start(string[] args)
        {
            Loader.App app = Loader.App.GetInstance();
            app.HeadlessMode = true;
            app.InitializeGameSources().GetAwaiter().GetResult();
            List<IGame> allGames = app.GetGames().GetAwaiter().GetResult();
            IGame? target = allGames.Find(x => x.Source.SlugServiceName == args[0] && x.InternalName == args[1]);
            if (target == null)
            {
                app.Logger.Log("Could not determine game given by commandline", LogType.Info, "Headless");
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args); // Give up and boot the GUI
                return;
            }

            List<Command> commands = target.GetCommands();
            Command? command = commands.Find(x => x.Text == args[2]);

            if (command == null)
            {
                app.Logger.Log("Could not determine command given for game by commmandline", LogType.Info, "Headless");
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args); // Give up and boot the GUI
                return;
            }
            
            command.Action?.Invoke();
            Thread.Sleep(20000);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
