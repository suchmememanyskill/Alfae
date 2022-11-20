using craftersmine.SteamGridDBNet;
using LauncherGamePlugin;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using SteamGridDbMiddleware.Model;

namespace SteamGridDbMiddleware.Gui;

public class OnBackgroundEdit
{
    public IGame Game { get; set; }
    public SteamGridDb Instance { get; set; }
    private string _searchTerm;

    public OnBackgroundEdit(IGame game, SteamGridDb instance)
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

        List<SteamGridDbHero> covers = new();
        string gameName = "???";

        if (games.Length > 0)
        {
            var game = games.First();
            gameName = game.Name;
            covers = (await Instance.Api.GetHeroesForGameAsync(game,
                dimensions: SteamGridDbDimensions.W1920H620 | SteamGridDbDimensions.W3840H1240, types: SteamGridDbTypes.Static))?.ToList() ?? new();
        }

        List<FormEntry> entries = new();

        entries.Add(Form.TextBox($"Backgrounds for {gameName}", FormAlignment.Center, "Bold"));
        entries.Add(Form.Button("Back", _ => Instance.App.HideForm(), "Change search term", _ => NewSearchTerm(), "Remove current background", _ => ClearBackground()));
        
        if (!HasBackground())
            entries.Last().ButtonList.Last().Action = null;
        
        foreach (var steamGridDbGrid in covers.Take(10))
        {
            entries.Add(Form.Image($"By {steamGridDbGrid.Author.Name}", () => Storage.ImageDownload(steamGridDbGrid.FullImageUrl), _ => SetBackground(steamGridDbGrid.Id.ToString(), steamGridDbGrid.FullImageUrl), FormAlignment.Center));
        }

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
    
    private void ClearBackground()
    {
        Instance.App.HideForm();
        Instance.Storage.Data.ClearBackground(Game);
        Instance.Storage.Save();
        Instance.App.ReloadGames();
    }

    private void SetBackground(string id, string url)
    {
        Instance.App.HideForm();
        Instance.Storage.Data.SetBackground(Game, id, url);
        Instance.Storage.Save();
        Instance.App.ReloadGames();
    }

    private bool HasBackground()
        => Instance.Storage.Data.HasBackground(Game);
}