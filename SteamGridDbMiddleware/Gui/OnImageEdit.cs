using craftersmine.SteamGridDBNet;
using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;
using SteamGridDbMiddleware.Model;

namespace SteamGridDbMiddleware.Gui;

public class OnImageEdit
{
    public IGame Game { get; set; }
    public SteamGridDb Instance { get; set; }
    public ImageType Type { get; set; }
    private int _perRow = 1;
    private string _searchTerm;

    public OnImageEdit(IGame game, SteamGridDb instance, ImageType type)
    {
        Game = new GameOverride(game, instance);
        Instance = instance;
        Type = type;
        _searchTerm = Instance.CacheSearchTerm(game, game.Name);

        if (Type == ImageType.VerticalCover)
            _perRow = 2;
    }

    public async void ShowGui()
    {
        var games = await Instance.Api.SearchForGamesAsync(_searchTerm);
        SteamGridDbGame? game = null;
        string gameName = "???";
        List<Override> overrides = new();

        if (games.Length > 0)
        {
            game = games.First();
            gameName = game.Name;
            overrides = await Instance.GetOverridesForImageType(game, Type);
        }

        List<FormEntry> form = new();
        
        if (Game.HasImage(Type))
        {
            form.Add(Form.Image($"Current {Type.ToString()}", () => Game.GetImage(Type), alignment: FormAlignment.Center));
            form.Add(Form.Separator());
        }
        
        form.Add(Form.TextBox($"{Type.ToString()}(s) for {gameName}", FormAlignment.Center, "Bold"));
        form.Add(Form.Button("Back", _ => Instance.App.HideForm(), "Change search term", _ => NewSearchTerm(), $"Remove current {Type}", _ => Clear()));
        
        if (Instance.Storage.Data.GetOverride(Game, Type) == null)
            form.Last().ButtonList.Last().Action = null;

        List<List<Override>> imageGroups = Enumerable.Range(0, (overrides.Count / _perRow))
            .Select(x => overrides.Skip(x * _perRow).Take(_perRow).ToList()).ToList();

        if (imageGroups.Count * _perRow != overrides.Count)
            imageGroups.Add(overrides.Skip(imageGroups.Count * _perRow).ToList());
        
        foreach (var imageGroup in imageGroups)
        {
            List<FormEntry> i = imageGroup
                .Select(x => Form.Image($"By {x.Author}", () => Storage.ImageDownload(x.Url), _ => Set(x)))
                .ToList();

            if (i.Count == 1)
            {
                i[0].Alignment = FormAlignment.Center;
                form.Add(i[0]);
            }
            else
            {
                form.Add(Form.Horizontal(i, alignment: FormAlignment.Center, spacing: 15));
            }
        }
        
        Instance.App.ShowForm(form);
    }
    
    private void NewSearchTerm()
    {
        SearchTermEdit edit = new(Instance.App, _searchTerm);
        edit.OnSubmit += x =>
        {
            _searchTerm = x;
            Instance.SetSearchTermCache(Game, _searchTerm);
            ShowGui();
        };
        edit.ShowGui();
    }

    private void Clear() => Set(null);
    private void Set(Override? @override)
    {
        Instance.App.HideForm();
        Instance.Storage.Data.SetOverride(Game, Type, @override);
        Instance.Storage.Save();
        Instance.App.ReloadGames();
    }
}