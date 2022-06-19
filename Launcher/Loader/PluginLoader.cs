using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        if (!Directory.Exists(path))
            return new();

        List<string> items = new();

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

    public static Assembly LoadPluginAssembly(string pluginPath)
    {
        Debug.WriteLine($"Reading {pluginPath}...");
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

    public static List<IGameSource> GetGameSources()
    {
        List<IGameSource> sources = new();
        
        AvailablePlugins().ForEach(x =>
        {
            try
            {
                Assembly assembly = LoadPluginAssembly(x);
                foreach (var y in GetGameSourcesFromAssembly(assembly))
                {
                    sources.Add(y);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception during loading of {x}: {e.Message}");
            }
        });

        return sources;
    }
}