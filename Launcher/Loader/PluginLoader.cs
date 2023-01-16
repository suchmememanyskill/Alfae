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
            items.AddRange(
                File.ReadAllLines(Path.GetFullPath(Path.Join(path, "..", "..", "..", "..", "..", "Plugins.txt")))
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => Path.GetFullPath(Path.Join(path, "..", "..", "..", "..", "..", x, "bin", "Debug", "net7.0", $"{x}.dll"))));
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
            if (!File.Exists(x))
                return;
            
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