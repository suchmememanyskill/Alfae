using ItchIoIntegration.Requests;
using LauncherGamePlugin;
using System.IO.Compression;
using ItchIoIntegration.Extensions;

namespace ItchIoIntegration.Service;

public class ItchGameDownload : ProgressStatus
{
    public event Action? OnCompletionOrCancel;
    
    private string _downloadUrl;
    private string _path;
    private string _filename;
    private readonly CancellationTokenSource _cts = new();
    private bool _doneDownloading = false;
    private int _lastSecond = 0;

    public ItchGameDownload(string url, string path, string filename)
    {
        _path = path;
        _downloadUrl = url;
        _filename = filename;
    }

    private void OnProgressUpdate(object? obj, float progress)
    {
        if (_doneDownloading || _lastSecond == DateTime.Now.Second) // Only make the UI respond once a second
            return;

        _lastSecond = DateTime.Now.Second;
        
        progress *= 100;
        Line1 = $"Downloading: {progress:0}%";
        Percentage = progress;
        InvokeOnUpdate();
    }
    
    public async Task Download()
    {
        _doneDownloading = false;
        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);
        
        using HttpClient client = new();
        string filePath = Path.Join(_path, _filename);
        var fs = new FileStream(filePath, FileMode.Create);

        Progress<float> progress = new();
        progress.ProgressChanged += OnProgressUpdate;

        try
        {
            await client.DownloadAsync(_downloadUrl, fs, progress, _cts.Token);
        }
        catch (TaskCanceledException e)
        {
            fs.Close();
            OnCompletionOrCancel?.Invoke();
            throw;
        }

        _doneDownloading = true;
        progress.ProgressChanged -= OnProgressUpdate;
        Percentage = 100;
        await fs.DisposeAsync();

        if (_filename.EndsWith(".zip"))
        {
            Line1 = "Unzipping...";
            InvokeOnUpdate();
            await Task.Run(() => ZipFile.ExtractToDirectory(filePath, _path), _cts.Token);
            File.Delete(filePath);
        }

        OnCompletionOrCancel?.Invoke();
    }

    public void Stop()
    {
        if (_doneDownloading)
            return;
        
        _cts.Cancel();
        OnCompletionOrCancel += () => Directory.Delete(_path, true);
    }
}