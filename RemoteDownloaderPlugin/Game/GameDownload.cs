using ICSharpCode.SharpZipLib.Zip;
using LauncherGamePlugin;
using LauncherGamePlugin.Interfaces;
using RemoteDownloaderPlugin.Utils;
using ZipFile = System.IO.Compression.ZipFile;

namespace RemoteDownloaderPlugin.Game;

public class GameDownload : ProgressStatus
{
    private OnlineGameDownload _entry;
    private int _lastSecond = 0;
    private readonly CancellationTokenSource _cts = new();
    private bool _doneDownloading = false;
    public long TotalSize { get; private set; }
    public string Version { get; private set; }
    public GameType Type { get; private set; }
    public string Filename { get; private set; }
    public string BasePath { get; private set; }
    public ContentTypes InstalledEntries { get; private set; }
    
    private DateTimeOffset _downloadStart = DateTimeOffset.Now;

    public GameDownload(OnlineGameDownload entry)
    {
        _entry = entry;
        InstalledEntries = new();
    }
    
    private void OnProgressUpdate(object? obj, float progress)
    {
        if (_doneDownloading || _lastSecond == DateTime.Now.Second) // Only make the UI respond once a second
            return;

        _lastSecond = DateTime.Now.Second;

        var timeBetweenNowAndStart = DateTimeOffset.Now - _downloadStart;
        var totalTime = timeBetweenNowAndStart * (1 / progress);
        var estimatedTime = totalTime - timeBetweenNowAndStart;
        var estimatedDisplay = LauncherGamePlugin.Utils.TimeSpanAsTimeEstimate(estimatedTime);
        
        progress *= 100;
        Line1 = $"Downloading: {progress:0}% {estimatedDisplay}";
        Percentage = progress;
        InvokeOnUpdate();
    }

    public async Task Download(IApp app)
    {
        _doneDownloading = false;

        if (_entry.Platform == "Pc")
            await DownloadPc(app);
        else
            await DownloadEmu(app);
    }

    private async Task DownloadEmu(IApp app)
    {
        Type = GameType.Emu;
        if (_entry.Files.Count(x => x.Type == DownloadType.Base) != 1)
        {
            throw new Exception("Multiple base images, impossible download");
        }

        Line2 = $"{_entry.Files.Count(x => x.Type == DownloadType.Base)} base, {_entry.Files.Count(x => x.Type == DownloadType.Update)} update, {_entry.Files.Count(x => x.Type == DownloadType.Dlc)} dlc";
        BasePath = Path.Join(app.GameDir, "Remote", _entry.Platform);
        string baseGamePath = null;
        var extraFilesPath = Path.Join(BasePath, _entry.Id);
        Directory.CreateDirectory(BasePath);
        Directory.CreateDirectory(extraFilesPath);
        
        using HttpClient client = new();

        for (int i = 0; i < _entry.Files.Count; i++)
        {
            Progress<float> localProcess = new();
            localProcess.ProgressChanged += (sender, f) =>
            {
                var part = (float)1 / _entry.Files.Count;
                var add = (float)i / _entry.Files.Count;
                OnProgressUpdate(null, part * f + add);
            };
            
            var fileEntry = _entry.Files[i];
            var destPath = Path.Join(fileEntry.Type == DownloadType.Base ? BasePath : extraFilesPath, fileEntry.Name);
            InstalledEntries.Add(fileEntry.Type);
            
            if (fileEntry.Type == DownloadType.Base)
            {
                baseGamePath = destPath;
                Filename = fileEntry.Name;
            }
            
            var fs = new FileStream(destPath, FileMode.Create);
            
            try
            {
                await client.DownloadAsync(fileEntry.Url, fs, localProcess, _cts.Token);
            }
            catch (TaskCanceledException e)
            {
                await Task.Run(() => fs.Dispose());

                if (baseGamePath != null && File.Exists(baseGamePath))
                {
                    File.Delete(baseGamePath);
                }
                
                Directory.Delete(extraFilesPath, true);
                
                throw;
            }
            
            Line1 = "Saving file...";
            InvokeOnUpdate();
            await Task.Run(() => fs.Dispose());
        }
        
        TotalSize = (await Task.Run(() => LauncherGamePlugin.Utils.DirSize(new(extraFilesPath)))) + (new FileInfo(baseGamePath!)).Length;
        Version = _entry.Files.Last(x => x.Type is DownloadType.Base or DownloadType.Update).Version;
    }

    private async Task DownloadPc(IApp app)
    {
        Type = GameType.Pc;
        BasePath = Path.Join(app.GameDir, "Remote", "Pc", _entry.Id);
        Directory.CreateDirectory(BasePath);

        using HttpClient client = new();
        Progress<float> progress = new();
        progress.ProgressChanged += OnProgressUpdate;

        try
        {
            // TODO: Fix security vuln, zips can have backwards paths
            using HttpResponseMessage response = await client.GetAsync(_entry.Files.First().Url, HttpCompletionOption.ResponseHeadersRead, _cts.Token);
            response.EnsureSuccessStatusCode();
            await using var responseStream = await response.Content.ReadAsStreamAsync();
            var interceptor =
                new StreamInterceptor(responseStream, progress, response.Content.Headers.ContentLength!.Value);
            await using var zipInputStream = new ZipInputStream(interceptor);

            while (zipInputStream.GetNextEntry() is { } zipEntry)
            {
                var destinationPath = Path.Combine(BasePath, zipEntry.Name);

                // Ensure the parent directory exists
                var directoryPath = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Skip directory entries
                if (zipEntry.IsDirectory)
                {
                    continue;
                }

                var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
                try
                {
                    await zipInputStream.CopyToAsync(fileStream, _cts.Token);
                }
                finally
                {
                    await Task.Run(() => fileStream.Dispose());
                }
            }
        }
        catch (Exception e)
        {
            app.Logger.Log($"Download failed: {e.Message}", LogType.Error, "Remote");
            Directory.Delete(BasePath, true);
            throw;
        }

        InstalledEntries.Base++;
        TotalSize = await Task.Run(() => LauncherGamePlugin.Utils.DirSize(new(BasePath)));
        Version = _entry.Files.First().Version;
    }
    
    public void Stop()
    {
        if (_doneDownloading)
            return;
        
        _cts.Cancel();
    }
}