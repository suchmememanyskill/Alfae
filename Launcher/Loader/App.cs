using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Launcher.Extensions;
using Launcher.Forms;
using Launcher.Utils;
using Launcher.Views;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin;
using LauncherGamePlugin.Forms;

namespace Launcher.Loader;

public class App : IApp
{
    public string ConfigDir
    {
        get
        {
            string path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Launcher");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
    }

    public string GameDir
    {
        get
        {
            if (!Directory.Exists(_gameDir))
                Directory.CreateDirectory(_gameDir);

            return _gameDir;
        }
        set
        {
            if (!Directory.Exists(value))
                throw new Exception("Not a valid directory");
            
            _gameDir = value;
            File.WriteAllText(Path.Join(ConfigDir, "dlloc.txt"), _gameDir);
        }
    }

    private string _gameDir;

    public Logger Logger { get; } = new();

    public List<IGameSource> GameSources { get; private set; } = new();
    public MainWindow MainWindow { get; set; }
    public MainView MainView { get; set; }
    public List<IGame> Games { get; set; }

    public List<IGame> InstalledGames =>
        Games.Where(x => x.InstalledStatus == InstalledStatus.Installed).ToList();

    public List<IGame> NotInstalledGames => Games.Where(x => x.InstalledStatus == InstalledStatus.NotInstalled).ToList();

    public async Task InitializeGameSources()
    {
        List<IGameSource> sources = PluginLoader.GetGameSources(this);
        List<Task> tasks = new();
        sources.ForEach(x =>
        {
            Logger.Log($"Initialising {x.ServiceName}...");
            tasks.Add(x.Initialize(this));
        });

        await Task.WhenAll(tasks);
        GameSources = sources;
    }

    public void ShowForm(Form form) => Dispatcher.UIThread.Post(() => ShowForm2(form));

    private void ShowForm2(Form form)
    {
        MainView.Overlay.Children.Clear();
        MainView.Overlay.Children.Add(new FormOverlay(form));
        MainView.Overlay.IsVisible = true;
    }

    public void HideOverlay() => Dispatcher.UIThread.Post(HideOverlay2);

    private void HideOverlay2()
    {
        MainView.Overlay.IsVisible = false;
        MainView.Overlay.Children.Clear();
    }

    public void ReloadGames() => Dispatcher.UIThread.Post(ReloadGames2);
    public void ReloadGlobalCommands() => Dispatcher.UIThread.Post(() => MainView.UpdateView());

    public void Launch(ExecLaunch launch)
    {
        Launcher.Launcher l = new();
        try
        {
            l.Launch(launch);
        }
        catch (Exception e)
        {
            ShowForm(new Form(new()
            {
                new FormEntry(FormEntryType.TextBox, $"Failed to launch game\n{e.Message}"),
                new FormEntry(FormEntryType.ButtonList, "", buttonList: new()
                {
                    {"Back", x => HideOverlay()}
                })
            }));
        }
    }

    public async Task ReloadGames2Task()
    {
        GameViews.ForEach(x => x.Destroy());
        Games = new();
        GameViews.Clear();
        List<Task<List<IGame>>> tasks = new();
        GameSources.ForEach(x => tasks.Add(x.GetGames()));
        await Task.WhenAll(tasks);
        tasks.ForEach(x => Games.AddRange(x.Result));
        
        bool anyInstalledGames = InstalledGames.Count != 0;
        bool anyNotInstalledGames = NotInstalledGames.Count != 0;

        MainView.InstalledLabel.IsVisible = anyInstalledGames;
        MainView.InstalledListBox.IsVisible = anyInstalledGames;
        MainView.NotInstalledLabel.IsVisible = anyNotInstalledGames;
        MainView.NotInstalledListBox.IsVisible = anyNotInstalledGames;

        Games = Games.OrderBy(x => x.Name).ToList();

        if (anyInstalledGames)
        {
            _installedGameViews = InstalledGames.Select(x => new GameViewSmall(x)).ToList();
            MainView.InstalledListBox.Items = _installedGameViews;
            GameViews.AddRange(_installedGameViews);
        }

        if (anyNotInstalledGames)
        {
            _notInstalledGameViews = NotInstalledGames.Select(x => new GameViewSmall(x)).ToList();
            MainView.NotInstalledListBox.Items = _notInstalledGameViews;
            GameViews.AddRange(_notInstalledGameViews);
        }

        ReloadGlobalCommands();
    }

    public List<GameViewSmall> GameViews { get; private set; } = new();
    private List<GameViewSmall> _installedGameViews = new();
    private List<GameViewSmall> _notInstalledGameViews = new();

    public async void ReloadGames2() => await ReloadGames2Task();

    private App()
    {
        _gameDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Games");

        if (File.Exists(Path.Join(ConfigDir, "dlloc.txt")))
            _gameDir = File.ReadAllText(Path.Join(ConfigDir, "dlloc.txt"));
    }

    private static App? _instance;
    public static App GetInstance()
    {
        return _instance ??= new();
    }
}