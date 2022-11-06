using LauncherGamePlugin;
using LauncherGamePlugin.Commands;
using LauncherGamePlugin.Interfaces;
using SteamGridDbMiddleware.Gui;
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

        await CheckLoggedInStatus(Storage.Data.ApiKey);
        
        App = app;

        return new InitResult()
        {
            Middlewares = new()
            {
                new Middleware(this)
            }
        };
    }

    public async Task<bool> CheckLoggedInStatus(string? key)
    {
        Api = null;
        if (string.IsNullOrEmpty(key))
            return false;
        
        craftersmine.SteamGridDBNet.SteamGridDb api = new(key);
        try
        {
            var game = await api.GetGameByIdAsync(1226);
            Api = api;
            return true;
        }
        catch (Exception e)
        {
            App.Logger.Log($"Failed to check SteamGridDb login status: {e.Message}. Treating as bad login");
            return false;
        }
    }

    public List<Command> GetGlobalCommands()
    {
        if (Api == null)
        {
            return new()
            {
                new("Not logged in"),
                new(),
                new("Log in", () => new Authenticate(this).ShowGui())
            };
        }
        else
        {
            return new()
            {
                new("Logged in"),
                new(),
                new("Log out", Logout)
            };
        }
    }

    private async void Logout()
    {
        Storage.Data.ApiKey = "";
        Storage.Save();
        await CheckLoggedInStatus("");
        App.ReloadGames();
    }
}