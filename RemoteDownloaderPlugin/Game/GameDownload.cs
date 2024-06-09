using System.IO.Compression;
using LauncherGamePlugin;
using LauncherGamePlugin.Interfaces;
using RemoteDownloaderPlugin.Utils;

namespace RemoteDownloaderPlugin.Game;

public class GameDownload : ProgressStatus
{
    private IEntry _entry;
    private int _lastSecond = 0;
    private readonly CancellationTokenSource _cts = new();
    private bool _doneDownloading = false;
    public long TotalSize { get; private set; }
    public string Version { get; private set; }
    public GameType Type { get; private set; }
    public string BaseFileName { get; private set; }
    public ContentTypes InstalledEntries { get; private set; }
    
    private DateTimeOffset _downloadStart = DateTimeOffset.Now;

    public GameDownload(IEntry entry)
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
        var estimatedDisplay = "";

        if (estimatedTime.TotalMinutes < 60)
        {
            estimatedDisplay = $"{estimatedTime.Minutes}m";
        }
        else if (estimatedTime.TotalMinutes < 1440)
        {
            estimatedDisplay = $"{estimatedTime.Hours}h{estimatedTime.Minutes}m";
        }
        else
        {
            estimatedDisplay = $"{estimatedTime.Days}d{estimatedTime.Hours}h{estimatedTime.Minutes}m";
        }
        
        progress *= 100;
        Line1 = $"Downloading: {progress:0}% {estimatedDisplay}";
        Percentage = progress;
        InvokeOnUpdate();
    }

    public async Task Download(IApp app)
    {
        _doneDownloading = false;

        if (_entry is EmuEntry emuEntry)
            await DownloadEmu(app, emuEntry);
        else if (_entry is PcEntry pcEntry)
            await DownloadPc(app, pcEntry);
        else
            throw new Exception("Download failed: Unknown type");
    }

    private async Task DownloadEmu(IApp app, EmuEntry entry)
    {
        Type = GameType.Emu;
        if (entry.Files.Count(x => x.Type == "base") != 1)
        {
            throw new Exception("Multiple base images, impossible download");
        }

        Line2 = $"{entry.Files.Count(x => x.Type == "base")} base, {entry.Files.Count(x => x.Type == "update")} update, {entry.Files.Count(x => x.Type == "dlc")} dlc";
        var basePath = Path.Join(app.GameDir, "Remote", entry.Emu);
        string baseGamePath = null;
        var extraFilesPath = Path.Join(app.GameDir, "Remote", entry.Emu, entry.GameId);
        Directory.CreateDirectory(basePath);
        Directory.CreateDirectory(extraFilesPath);
        
        using HttpClient client = new();

        for (int i = 0; i < entry.Files.Count; i++)
        {
            
            Progress<float> localProcess = new();
            localProcess.ProgressChanged += (sender, f) =>
            {
                var part = (float)1 / entry.Files.Count;
                var add = (float)i / entry.Files.Count;
                OnProgressUpdate(null, part * f + add);
            };
            
            var fileEntry = entry.Files[i];
            var destPath = Path.Join(fileEntry.Type == "base" ? basePath : extraFilesPath, fileEntry.Name);
            InstalledEntries.Add(fileEntry.Type);
            
            if (fileEntry.Type == "base")
            {
                baseGamePath = destPath;
                BaseFileName = fileEntry.Name;
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
        Version = entry.Files.Last(x => x.Type is "base" or "update").Version;
    }

    private async Task DownloadPc(IApp app, PcEntry entry)
    {
        Type = GameType.Pc;
        var basePath = Path.Join(app.GameDir, "Remote", "Pc", entry.GameId);
        Directory.CreateDirectory(basePath);
        var zipFilePath = Path.Join(basePath, "__game__.zip");

        using HttpClient client = new();
        var fs = new FileStream(zipFilePath, FileMode.Create);

        Progress<float> progress = new();
        progress.ProgressChanged += OnProgressUpdate;

        try
        {
            await client.DownloadAsync(entry.Url, fs, progress, _cts.Token);
        }
        catch (TaskCanceledException e)
        {
            await Task.Run(() => fs.Dispose());

            Directory.Delete(basePath);
            
            throw;
        }

        
        Percentage = 100;
        Line1 = "Saving...";
        InvokeOnUpdate();
        await Task.Run(() => fs.Dispose());
        Line1 = "Unzipping...";
        InvokeOnUpdate();
        await Task.Run(() => ZipFile.ExtractToDirectory(zipFilePath, basePath));
        File.Delete(zipFilePath);

        if (_cts.IsCancellationRequested)
        {
            Directory.Delete(basePath);
        }
        
        TotalSize = await Task.Run(() => LauncherGamePlugin.Utils.DirSize(new(basePath)));
        Version = entry.Version;
    }
    
    public void Stop()
    {
        if (_doneDownloading)
            return;
        
        _cts.Cancel();
    }
}