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
    public string ConfigDir =>
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Launcher");
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
        Panel panel = new();
        panel.Background = new SolidColorBrush(new Color(128, 0, 0, 0));
        MainView.Overlay.Children.Add(panel);
        Border border = new Border();
        border.CornerRadius = new CornerRadius(5);
        border.Background = new SolidColorBrush(new Color(255, 34, 34, 34));
        border.Margin = new Thickness(20);
        border.Width = 600;
        border.HorizontalAlignment = HorizontalAlignment.Center;
        border.VerticalAlignment = VerticalAlignment.Center;
        ScrollViewer scrollViewer = new ScrollViewer();
        scrollViewer.Padding = new Thickness(10);
        StackPanel stackPanel = new StackPanel();
        stackPanel.Spacing = 10;
        scrollViewer.Content = stackPanel;
        scrollViewer.VerticalAlignment = VerticalAlignment.Center;
        border.Child = scrollViewer;
        form.FormEntries.ForEach(x => stackPanel.Children.Add(x.ToTemplatedControl()));
        MainView.Overlay.Children.Add(border);
        MainView.Overlay.IsVisible = true;
    }

    public void HideOverlay() => Dispatcher.UIThread.Post(HideOverlay2);

    private void HideOverlay2()
    {
        MainView.Overlay.IsVisible = false;
        MainView.Overlay.Children.Clear();
    }

    public void ReloadGames() => Dispatcher.UIThread.Post(ReloadGames2);

    public async Task ReloadGames2Task()
    {
        Games = new();
        List<Task<List<IGame>>> tasks = new();
        GameSources.ForEach(x => tasks.Add(x.GetGames()));
        await Task.WhenAll(tasks);
        tasks.ForEach(x => Games.AddRange(x.Result));
        GameViews = Games.Select(x => new GameViewSmall(x)).ToList();
        MainView.ListBox.Items = GameViews;
    }
    
    public List<GameViewSmall> GameViews { get; private set; }

    public async void ReloadGames2() => await ReloadGames2Task();

    private App()
    { }

    private static App? _instance;
    public static App GetInstance()
    {
        return _instance ??= new();
    }
}