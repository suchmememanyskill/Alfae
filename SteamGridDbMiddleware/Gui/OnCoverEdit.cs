using craftersmine.SteamGridDBNet;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using SteamGridDbMiddleware.Model;

namespace SteamGridDbMiddleware.Gui;

public class OnCoverEdit
{
    public IGame Game { get; set; }
    public SteamGridDb Instance { get; set; }

    public OnCoverEdit(IGame game, SteamGridDb instance)
    {
        Game = game;
        Instance = instance;
    }

    public async void OnGui()
    {
        var games = await Instance.Api.SearchForGamesAsync(Game.Name);
        var game = games.First();

        var covers = await Instance.Api.GetGridsForGameAsync(game, dimensions: SteamGridDbDimensions.W600H900);

        List<FormEntry> entries = new();
        entries.Add(Form.Button("Back", _ => Instance.App.HideForm(), "Remove current background", _ => ClearCover()));

        if (!HasCover())
            entries.Last().ButtonList.Last().Action = null;
        
        foreach (var steamGridDbGrid in covers.Take(10))
        {
            entries.Add(Form.Image($"By {steamGridDbGrid.Author.Name}", () => GetImage(steamGridDbGrid.FullImageUrl), _ => SetCover(steamGridDbGrid.Id.ToString(), steamGridDbGrid.FullImageUrl), FormAlignment.Center));
        }
        
        Instance.App.ShowForm(entries);
    }

    private async Task<byte[]?> GetImage(string url)
    {
        using HttpClient client = new();
        try
        {
            HttpResponseMessage response = await client.GetAsync(new Uri(url));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch
        {
            return null;
        }
    }

    private void ClearCover()
    {
        Instance.App.HideForm();
        Override? x = Instance.Storage.Data.Covers.Find(x => x.GameName == Game.Name && x.GameSource == Game.Source.ServiceName);
        if (x != null)
        {
            Instance.Storage.Data.Covers.Remove(x);
            Instance.Storage.Save();
            Instance.App.ReloadGames();
        }
    }

    private void SetCover(string id, string url)
    {
        Instance.App.HideForm();
        Override? x = Instance.Storage.Data.Covers.Find(x => x.GameName == Game.Name && x.GameSource == Game.Source.ServiceName);
        x ??= new(Game.Name, Game.Source.ServiceName, url, id);

        x.Url = url;
        x.Id = id;

        if (!Instance.Storage.Data.Covers.Contains(x))
            Instance.Storage.Data.Covers.Add(x);
        
        Instance.Storage.Save();
        Instance.App.ReloadGames();
    }

    private bool HasCover()
        => Instance.Storage.Data.Covers.Find(x => x.GameName == Game.Name && x.GameSource == Game.Source.ServiceName) != null;
}