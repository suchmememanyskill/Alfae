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

    public ItchGameDownload(string url, string path, string filename)
    {
        _path = path;
        _downloadUrl = url;
        _filename = filename;
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
        progress.ProgressChanged += (_, val) =>
        {
            val *= 100;
            Line1 = $"Downloading: {val:0.00}%";
            Percentage = val;
            InvokeOnUpdate();
        };

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
        fs.Close();

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