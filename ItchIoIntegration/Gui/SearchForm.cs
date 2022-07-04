using ItchIoIntegration.Requests;
using ItchIoIntegration.Service;
using LauncherGamePlugin;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;

namespace ItchIoIntegration.Gui;

public class SearchForm
{
    private IApp _app;
    private ItchApiProfile _profile;
    private ItchGameSource _source;

    public SearchForm(IApp app, ItchApiProfile profile, ItchGameSource source)
    {
        _app = app;
        _profile = profile;
        _source = source;
    }

    public async void ShowForm(string search = "")
    {
        List<ItchApiGame> games = new();

        if (!string.IsNullOrWhiteSpace(search))
        {
            _app.ShowTextPrompt("Searching...");
            var response = await ItchApiSearch.Get(_profile, search);
            if (response != null)
            {
                games.AddRange(response.Games.Where(x => x.Classification == "game" && ((x.HasDemo() && x.IsPaid()) || !x.IsPaid())));
            }
        }

        List<FormEntry> entries = new()
        {
            Form.TextBox("Search for a game on itch.io", FormAlignment.Center, "Bold"),
            Form.TextBox("Note: paid games are hidden. They are already displayed in your library",
                FormAlignment.Center),
            Form.TextInput("Search:", search),
            Form.Button("Back", x => _app.HideForm(), "Search", x =>
            {
                string s = x.GetValue("Search:")!;
                ShowForm(s);
            })
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            entries.Add(Form.TextBox($"Found {games.Count} results", FormAlignment.Center));
        }
        
        foreach (var itchApiGame in games)
        {
            entries.Add(Form.TextBox(""));
            string text = itchApiGame.Title;
            
            if (!string.IsNullOrWhiteSpace(itchApiGame.ShortText))
                text += $": {itchApiGame.ShortText}";
            
            entries.Add(Form.ClickableLinkBox(text, x => Utils.OpenUrl(itchApiGame.Url.AbsoluteUri), FormAlignment.Center, "Bold"));
            entries.Add(Form.Button("Download", x => ProcessDownload(itchApiGame), FormAlignment.Center));
        }
        
        _app.ShowForm(entries);
    }

    private void ProcessDownload(ItchApiGame game)
    {
        ItchGame fake = new(game, _source);
        _source.AddFakeGameToGames(fake);
        new DownloadSelectForm(fake, _app, _source).InitiateForm();
    }
}