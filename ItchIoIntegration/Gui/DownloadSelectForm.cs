using ItchIoIntegration.Requests;
using ItchIoIntegration.Service;
using LauncherGamePlugin;
using LauncherGamePlugin.Forms;
using LauncherGamePlugin.Interfaces;

namespace ItchIoIntegration.Gui;

public class DownloadSelectForm
{
    private ItchGame _game;
    private IApp _app;
    private ItchGameSource _source;

    public DownloadSelectForm(ItchGame game, IApp app, ItchGameSource source)
    {
        _game = game;
        _app = app;
        _source = source;
    }

    public async void InitiateForm()
    {
        _app.ShowTextPrompt("Getting game files...");
        ItchApiGameUploads? uploads = await _game.GetUploads();
        if (uploads == null)
        {
            _source.Log("Failed to get game files", LogType.Error);
            _app.HideForm();
            return;
        }
        
            
        _source.Log($"Got {uploads.Uploads.Count} upload(s)");
        ShowForm(uploads);
    }

    private void ShowForm(ItchApiGameUploads uploads)
    {
        List<FormEntry> entries = new()
        {
            Form.TextBox($"Available downloads for {_game.Name}", FormAlignment.Center, "Bold")
        };

        List<ItchApiUpload> shownUploads = uploads.Uploads;

        if (_game.DownloadKeyId != null)
            shownUploads = shownUploads.Where(x => !x.IsDemo()).ToList();

        entries.AddRange(shownUploads.Select(x => Form.ClickableLinkBox( GenerateName(x), y => ContinueDownload(x))));
        entries.Add(Form.Button("Back", _ => _app.HideForm()));
        
        _app.ShowForm(entries);
    }

    private async void ContinueDownload(ItchApiUpload upload)
    {
        _source.Log($"Selected download {upload.DisplayName}");
        _app.HideForm();
        _game.DownloadGame(upload);
    }

    private string GenerateName(ItchApiUpload upload)
    {
        string text = "";
        
        if (upload.Build != null && !string.IsNullOrWhiteSpace(upload.Build.UserVersion))
            text += $"[Version {upload.Build.UserVersion}] ";

        if (!string.IsNullOrWhiteSpace(upload.DisplayName))
            text += $"{upload.DisplayName} ({upload.Filename})";
        else
            text += upload.Filename;
        
        return text;
    }
}