using System;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;

namespace Launcher.Launcher;

public class ProfileWrapper<T> : IBootProfile where T : IBootProfile, new()
{
    public string Name { get; }
    public Platform CompatiblePlatform { get; }
    public Platform CompatibleExecutable { get; }
    public void Launch(LaunchParams launchParams, IApp? app)
    {
        T instance = new T();
        instance.OnGameLaunch += x => OnGameLaunch?.Invoke(x);
        instance.OnGameClose += x => OnGameClose?.Invoke(x);
        instance.Launch(launchParams, app);
    }

    public event Action<LaunchParams>? OnGameLaunch;
    public event Action<LaunchParams>? OnGameClose;

    public ProfileWrapper()
    {
        T instance = new T();
        Name = instance.Name;
        CompatiblePlatform = instance.CompatiblePlatform;
        CompatibleExecutable = instance.CompatibleExecutable;
    }
}