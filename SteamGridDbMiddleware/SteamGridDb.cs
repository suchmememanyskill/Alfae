using LauncherGamePlugin;
using LauncherGamePlugin.Interfaces;
using SteamGridDbMiddleware.Model;

namespace SteamGridDbMiddleware;

public class SteamGridDb : IGameSource
{
    public string ServiceName => "SteamGridDb";
    public string Version => "v1.0.0";
    public string SlugServiceName => "steam-grid-db";
    public string ShortServiceName => "steamgriddb";
    public IApp App { get; set; }
    public craftersmine.SteamGridDBNet.SteamGridDb? Api { get; set; }
    public Storage<Store> Storage { get; set; }
    public async Task<InitResult?> Initialize(IApp app)
    {
        Storage = new(app, "steamgriddb.json");

        if (!string.IsNullOrEmpty(Storage.Data.ApiKey))
            Api = new(Storage.Data.ApiKey);
        
        App = app;

        return new InitResult()
        {
            Middlewares = new()
            {
                new Middleware(this)
            }
        };
    }
}