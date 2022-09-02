using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using Launcher.Configuration;
using Launcher.Forms;
using Launcher.Launcher;
using Launcher.Utils;
using Launcher.Views;
using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;

namespace Launcher.Loader;

public class App : IApp
{
    public static string Version => $"v{NewVersionCheck.Version}";
    public string ConfigDir
    {
        get
        {
            string oldPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Launcher");
            string path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Alfae");
            
            if (Directory.Exists(oldPath) && !Directory.Exists(path))
                Directory.Move(oldPath, path);
            
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
    }

    public string GameDir
    {
        get
        {
            if (!Directory.Exists(Config.DownloadLocation))
                Directory.CreateDirectory(Config.DownloadLocation);

            return Config.DownloadLocation;
        }
        set
        {
            if (!Directory.Exists(value))
                throw new Exception("Not a valid directory");
            
            Config.DownloadLocation = value;
            Config.Save(this);
        }
    }

    public bool HeadlessMode { get; set; } = false;

    public Logger Logger { get; } = new();

    public List<IGameSource> GameSources { get; private set; } = new();
    public MainWindow MainWindow { get; set; }
    public MainView MainView { get; set; }
    public List<IGame> Games { get; set; }
    public LauncherConfiguration Launcher { get; private set; }
    public Config Config { get; set; }

    public List<IGame> InstalledGames =>
        Games.Where(x => x.InstalledStatus == InstalledStatus.Installed).ToList();

    public List<IGame> NotInstalledGames => Games.Where(x => x.InstalledStatus == InstalledStatus.NotInstalled).ToList();

    private bool _initialised = false;

    private async Task<IGameSource> InitialiseService(IGameSource source)
    {
        await source.Initialize(this);
        return source;
    }
    
    public async Task InitializeGameSources()
    {
        if (_initialised)
            return;
        
        List<IGameSource> sources = PluginLoader.GetGameSources(this);
        List<Task<IGameSource>> tasks = new();
        sources.ForEach(x =>
        {
            Logger.Log($"Initialising {x.ServiceName}...");
            tasks.Add(InitialiseService(x));
        });

        while (true)
        {
            try
            {
                await Task.WhenAll(tasks);
                break;
            }
            catch (Exception e)
            {
                tasks.RemoveAll(x => x.IsFaulted);
                Logger.Log("One or more plugins failed to initialize properly!", LogType.Error);
            }
        }        
        
        GameSources = tasks.Select(x => x.Result).ToList();
        
        await Launcher.GetProfiles();
        _initialised = true;
    }

    public void ShowForm(Form form)
    {
        if (HeadlessMode)
        {
            _startForm = form;
            
            Program.BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(Array.Empty<string>());
            
            return;
        }
        Dispatcher.UIThread.Post(() => ShowForm2(form));
    }

    private void ShowForm2(Form form)
    {
        foreach (var x in MainView.Overlay.Children)
        {
            if (x is FormOverlay overlay)
            {
                overlay.SetNewForm(form);
                MainView.Overlay.IsVisible = true;
                return;
            }
        }
        
        MainView.Overlay.Children.Add(new FormOverlay(form));
        MainView.Overlay.IsVisible = true;
    }

    private Form? _startForm = null;

    public bool HasStartForm() => _startForm != null;
    public void ShowPossibleStartForm()
    {
        if (_startForm != null)
            ShowForm(_startForm);
    }

    public void HideForm()
    {
        if (HeadlessMode)
            return;
        
        Dispatcher.UIThread.Post(HideOverlay2);
    }

    private void HideOverlay2()
    {
        MainView.Overlay.IsVisible = false;
        MainView.Overlay.Children.Clear();
    }

    public void ReloadGames()
    {
        if (HeadlessMode)
            return;
        
        Dispatcher.UIThread.Post(ReloadGames2);
    }
    public void ReloadGlobalCommands() => Dispatcher.UIThread.Post(() => MainView.UpdateView());
    public void ReloadBootProfiles() => ReloadGlobalCommands();

    public void Launch(LaunchParams launchParams)
    {
        try
        {
            Launcher.Launch(launchParams);
        }
        catch (Exception e)
        {
            this.ShowDismissibleTextPrompt($"Failed to launch game\n{e.Message}");
        }
    }

    public List<IGame> GetAllGames() => Games;
    public List<IGameSource> GetAllSources() => GameSources;

    public async Task<List<IGame>> GetGames()
    {
        List<IGame> games = new();
        GameViews.Clear();
        List<Task<List<IGame>>> tasks = new();
        GameSources.ForEach(x => tasks.Add(x.GetGames()));
        await Task.WhenAll(tasks);
        tasks.ForEach(x => games.AddRange(x.Result));
        return games;
    }
    
    public async Task ReloadGames2Task()
    {
        GameViews.ForEach(x => x.Destroy());
        Games = await GetGames();
        
        bool anyInstalledGames = InstalledGames.Count != 0;
        bool anyNotInstalledGames = NotInstalledGames.Count != 0;

        MainView.InstalledLabel.IsVisible = anyInstalledGames;
        MainView.InstalledListBox.IsVisible = anyInstalledGames;
        MainView.NotInstalledLabel.IsVisible = anyNotInstalledGames;
        MainView.NotInstalledListBox.IsVisible = anyNotInstalledGames;

        Games = Games.OrderBy(x => x.Name).ToList();

        if (anyInstalledGames)
        {
            _installedGameViews = InstalledGames.Select(x => new GameViewSmall(x, this)).ToList();
            MainView.InstalledListBox.Items = _installedGameViews;
            GameViews.AddRange(_installedGameViews);
        }

        if (anyNotInstalledGames)
        {
            _notInstalledGameViews = NotInstalledGames.Select(x => new GameViewSmall(x, this)).ToList();
            MainView.NotInstalledListBox.Items = _notInstalledGameViews;
            GameViews.AddRange(_notInstalledGameViews);
        }

        ReloadGlobalCommands();
        
        if (Games.Count <= 0)
            this.ShowDismissibleTextPrompt("Welcome to Alfae!\nTo get started, add some plugins and configure them in the top right under the 'plugins' tab");
        
        MainView.ApplySearch();
    }

    public List<GameViewSmall> GameViews { get; private set; } = new();
    private List<GameViewSmall> _installedGameViews = new();
    private List<GameViewSmall> _notInstalledGameViews = new();

    public async void ReloadGames2() => await ReloadGames2Task();

    private App()
    {
        Config = Config.Load(this);
        Launcher = new(this);
        Logger.Log($"Launcher {Version}");
    }

    private static App? _instance;
    public static App GetInstance()
    {
        return _instance ??= new();
    }
}