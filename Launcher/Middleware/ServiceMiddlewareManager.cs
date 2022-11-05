using System.Collections.Generic;
using System.Threading.Tasks;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;

namespace Launcher.Middleware;

public class ServiceMiddlewareManager
{
    public List<IServiceMiddleware> Middlewares { get; } = new();

    public Task<List<IBootProfile>> GetBootProfiles(IGameSource service)
         => GetServicesInternal(service).GetBootProfiles();

    public Task<List<IGame>> GetGames(IGameSource service)
        => GetServicesInternal(service).GetGames();

    public List<Command> GetGameCommands(IGame game, IGameSource service)
        => GetServicesInternal(service).GetGameCommands(game);
    
    public List<Command> GetGlobalCommands(IGameSource service)
        => GetServicesInternal(service).GetGlobalCommands();

    private MiddlewareBridge GetServicesInternal(IGameSource service)
    {
        MiddlewareBridge firstBridge = new(service, null, null);
        MiddlewareBridge previousBridge = firstBridge;
        MiddlewareBridge currentBridge = firstBridge;

        foreach (var middleware in Middlewares)
        {
            currentBridge.NextMiddleware = middleware;
            previousBridge = currentBridge;
            currentBridge = new(service, null, null);
            previousBridge.NextBridge = currentBridge;
        }

        return firstBridge;
    }
}