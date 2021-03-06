using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LauncherGamePlugin.Interfaces;

namespace Launcher.Loader;

public static class PluginLoader
{
    public static List<string> AvailablePlugins()
    {
        string path = Path.Join(AppContext.BaseDirectory, "plugins");
        List<string> items = new();
        
        #if DEBUG
            items.Add(Path.GetFullPath(Path.Join(path, "..", "..", "..", "..", "..", "LocalGames", "bin", "Debug", "net6.0", "LocalGames.dll")));
            items.Add(Path.GetFullPath(Path.Join(path, "..", "..", "..", "..", "..", "LegendaryIntegration", "bin", "Debug", "net6.0", "LegendaryIntegration.dll")));
            items.Add(Path.GetFullPath(Path.Join(path, "..", "..", "..", "..", "..", "SteamExporterPlugin", "bin", "Debug", "net6.0", "SteamExporterPlugin.dll")));
            items.Add(Path.GetFullPath(Path.Join(path, "..", "..", "..", "..", "..", "BottlesPlugin", "bin", "Debug", "net6.0", "BottlesPlugin.dll")));
            items.Add(Path.GetFullPath(Path.Join(path, "..", "..", "..", "..", "..", "ItchIoIntegration", "bin", "Debug", "net6.0", "ItchIoIntegration.dll")));
            items.Add(Path.GetFullPath(Path.Join(path, "..", "..", "..", "..", "..", "GogIntegration", "bin", "Debug", "net6.0", "GogIntegration.dll")));
        #endif
        
        if (!Directory.Exists(path))
            return items;

        foreach (var x in Directory.EnumerateDirectories(path))
        {
            string a = Path.GetFileName(x);
            if (File.Exists(Path.Join(x, $"{a}.dll")))
            {
                items.Add(Path.Join(x, $"{a}.dll"));
            }
        }

        return items;
    }

    public static Assembly LoadPluginAssembly(string pluginPath, IApp app)
    {
        app.Logger.Log($"Reading {pluginPath}...");
        PluginLoadContext ctx = new(pluginPath);
        return ctx.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginPath)));
    }
    
    public static IEnumerable<IGameSource> GetGameSourcesFromAssembly(Assembly assembly)
    {
        int count = 0;

        foreach (Type type in assembly.GetTypes())
        {
            if (typeof(IGameSource).IsAssignableFrom(type))
            {
                IGameSource? result = Activator.CreateInstance(type) as IGameSource;
                if (result == null) continue;
                count++;
                yield return result;
            }
        }

        if (count == 0)
        {
            string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
            throw new ApplicationException(
                $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
                $"Available types: {availableTypes}");
        }
    }

    public static List<IGameSource> GetGameSources(IApp app)
    {
        List<IGameSource> sources = new();
        
        AvailablePlugins().ForEach(x =>
        {
            try
            {
                Assembly assembly = LoadPluginAssembly(x, app);
                foreach (var y in GetGameSourcesFromAssembly(assembly))
                {
                    sources.Add(y);
                }
            }
            catch (Exception e)
            {
                app.Logger.Log($"Exception during loading of {x}: {e.Message}");
            }
        });

        return sources;
    }
}