using System.Collections.Generic;
using System.Threading.Tasks;
using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;

namespace Launcher.Middleware;

public class MiddlewareBridge : IGameSource
{
    public IGameSource Service { get; set; }
    public IServiceMiddleware? NextMiddleware { get; set; }
    public MiddlewareBridge? NextBridge { get; set; }
    public PluginType Type => PluginType.Middleware;

    public MiddlewareBridge(IGameSource service, IServiceMiddleware? nextMiddleware, MiddlewareBridge? nextBridge)
    {
        Service = service;
        NextMiddleware = nextMiddleware;
        NextBridge = nextBridge;
    }

    public string ServiceName => Service.ServiceName;
    public string Version => Service.Version;
    public string SlugServiceName => Service.SlugServiceName;
    public string ShortServiceName => Service.ShortServiceName;
    public Task<InitResult?> Initialize(IApp app)
    {
        throw new System.NotImplementedException();
    }

    public async Task<List<IBootProfile>> GetBootProfiles()
    {
        if (NextMiddleware == null)
            return await Service.GetBootProfiles();
        
        if (NextBridge == null)
            return await NextMiddleware.GetBootProfiles(Service);
        
        return await NextMiddleware.GetBootProfiles(NextBridge);
    }

    public async Task<List<IGame>> GetGames()
    {
        if (NextMiddleware == null)
            return await Service.GetGames();
        
        if (NextBridge == null)
            return await NextMiddleware.GetGames(Service);
        
        return await NextMiddleware.GetGames(NextBridge);
    }

    public List<Command> GetGameCommands(IGame game)
    {
        if (NextMiddleware == null)
            return Service.GetGameCommands(game);
        
        if (NextBridge == null)
            return NextMiddleware.GetGameCommands(game, Service);
        
        return NextMiddleware.GetGameCommands(game, NextBridge);
    }

    public List<Command> GetGlobalCommands()
    {
        if (NextMiddleware == null)
            return Service.GetGlobalCommands();
        
        if (NextBridge == null)
            return NextMiddleware.GetGlobalCommands(Service);
        
        return NextMiddleware.GetGlobalCommands(NextBridge);
    }
}