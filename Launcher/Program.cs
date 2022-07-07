using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Avalonia;
using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Extensions;
using LauncherGamePlugin.Forms;
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
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            
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
                app.ShowDismissibleTextPrompt($"Could not find game {args[1]} from service {args[0]}");
                return;
            }

            List<Command> commands = target.GetCommands();
            Command? command = commands.Find(x => x.Text == args[2]);

            if (command == null)
            {
                app.Logger.Log("Could not determine command given for game by commmandline", LogType.Info, "Headless");
                app.ShowDismissibleTextPrompt($"Game {target.Name} does not have the command {args[2]} available");
                return;
            }
            
            command.Action?.Invoke();
            Thread.Sleep(10000);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
        
        private static void OnUnhandledException (object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;
                File.WriteAllText("crash.log", ex.ToString());
            }
            catch { }
        }
    }
}
