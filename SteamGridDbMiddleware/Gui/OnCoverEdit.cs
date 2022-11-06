using craftersmine.SteamGridDBNet;
using LauncherGamePlugin;
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

    public async void ShowGui()
    {
        var games = await Instance.Api.SearchForGamesAsync(Game.Name);

        List<SteamGridDbGrid> covers = new();
        string gameName = "???";

        if (games.Length > 0)
        {
            var game = games.First();
            gameName = game.Name;
            covers = (await Instance.Api.GetGridsForGameAsync(game, dimensions: SteamGridDbDimensions.W600H900, types: SteamGridDbTypes.Static))?.ToList() ?? new();
        }

        List<FormEntry> images = new();
        List<FormEntry> entries = new();

        foreach (var steamGridDbGrid in covers.Take(10))
        {
            images.Add(Form.Image($"By {steamGridDbGrid.Author.Name}", () => Storage.ImageDownload(steamGridDbGrid.FullImageUrl), _ => SetCover(steamGridDbGrid.Id.ToString(), steamGridDbGrid.FullImageUrl), FormAlignment.Center));
        }

        entries.Add(Form.TextBox($"Covers for {gameName}", FormAlignment.Center, "Bold"));
        entries.Add(Form.Button("Back", _ => Instance.App.HideForm(), "Remove current background", _ => ClearCover()));
        
        if (!HasCover())
            entries.Last().ButtonList.Last().Action = null;
        
        int i = 0;
        List<FormEntry> current = new();

        foreach (var x in images)
        {
            current.Add(x);

            if (current.Count >= 2)
            {
                entries.Add(Form.Horizontal(current, alignment: FormAlignment.Center, spacing: 15));
                current = new();
            }
        }
        
        if (current.Count > 0)
            entries.Add(Form.Horizontal(current, alignment: FormAlignment.Center));

        Instance.App.ShowForm(entries);
    }
    
    private void ClearCover()
    {
        Instance.App.HideForm();
        Override? x = Instance.Storage.Data.GetCover(Game);
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
        Override? x = Instance.Storage.Data.GetCover(Game);
        x ??= new(Game.Name, Game.Source.ServiceName, url, id);

        x.Url = url;
        x.Id = id;

        if (!Instance.Storage.Data.Covers.Contains(x))
            Instance.Storage.Data.Covers.Add(x);
        
        Instance.Storage.Save();
        Instance.App.ReloadGames();
    }

    private bool HasCover()
        => Instance.Storage.Data.HasCover(Game);
}