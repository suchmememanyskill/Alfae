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
    private string _searchTerm;

    public OnCoverEdit(IGame game, SteamGridDb instance)
    {
        Game = game;
        Instance = instance;
    }

    public void ShowGui()
    {
        _searchTerm = Game.Name;
        ShowGuiInternal();
    }
    
    private async void ShowGuiInternal()
    {
        var games = await Instance.Api.SearchForGamesAsync(_searchTerm);

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
        entries.Add(Form.Button("Back", _ => Instance.App.HideForm(), "Change search term", _ => NewSearchTerm(), "Remove current Cover", _ => ClearCover()));
        
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
    
    private void NewSearchTerm()
    {
        SearchTermEdit edit = new(Instance.App, _searchTerm);
        edit.OnSubmit += x =>
        {
            _searchTerm = x;
            ShowGuiInternal();
        };
        edit.ShowGui();
    }
    
    private void ClearCover()
    {
        Instance.App.HideForm();
        Instance.Storage.Data.ClearCover(Game);
        Instance.Storage.Save();
        Instance.App.ReloadGames();
    }

    private void SetCover(string id, string url)
    {
        Instance.App.HideForm();
        Instance.Storage.Data.SetCover(Game, id, url);
        Instance.Storage.Save();
        Instance.App.ReloadGames();
    }

    private bool HasCover()
        => Instance.Storage.Data.HasCover(Game);
}