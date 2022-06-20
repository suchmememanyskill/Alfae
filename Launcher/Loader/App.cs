using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Launcher.Forms;
using Launcher.Utils;
using Launcher.Views;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin;

namespace Launcher.Loader;

public class App : IApp
{
    public string ConfigDir => throw new NotImplementedException();
    public Logger Logger { get; } = new();

    public List<IGameSource> GameSources { get; private set; } = new();
    public MainWindow MainWindow { get; set; }
    public MainView MainView { get; set; }

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

    public void ShowForm(string name, List<IFormElement> elements)
    {
        
    }

    private App()
    { }

    private static App? _instance;
    public static App GetInstance()
    {
        return _instance ??= new();
    }
}