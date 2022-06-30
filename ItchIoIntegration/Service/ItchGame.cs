using ItchIoIntegration.Gui;
using ItchIoIntegration.Requests;
using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Extensions;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;
using Newtonsoft.Json;

namespace ItchIoIntegration.Service;

public class ItchGame : IGame
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long? Size { get; set; } = 0;
    public string? InstallPath { get; set; }
    public Uri? CoverUri { get; set; }
    public long DownloadKeyId { get; set; }
    public List<ItchApiLaunchTarget> Targets { get; set; } = new();
    public int PreferredTarget { get; set; } = -1;
    public string CommandlineArgs { get; set; } = "";

    [JsonIgnore] public ItchGameDownload? Download { get; private set; }
    [JsonIgnore] public InstalledStatus InstalledStatus { get; private set; }
    
    [JsonIgnore] public IGameSource Source => ItchSource;
    [JsonIgnore] public ProgressStatus? ProgressStatus => Download;

    [JsonIgnore] public ItchGameSource ItchSource { get; set; }
    public event Action? OnUpdate;

    public ItchGame()
    {
        InstalledStatus = InstalledStatus.Installed;
    }

    public ItchGame(ItchApiOwnedGameKey key, ItchGameSource itchSource)
    {
        InstalledStatus = InstalledStatus.NotInstalled;
        ItchSource = itchSource;
        Name = key.Game.Title;
        Id = key.GameId;
        CoverUri = key.Game.GetCoverUrl();
        DownloadKeyId = key.DownloadKeyId;
    }

    public async void DownloadGame(ItchApiUpload upload)
    {
        await ItchApiScannedArchive.Get(ItchSource.Profile!, this, upload);
        string url = upload.GetDownloadUrl(DownloadKeyId, ItchSource.Profile!);
        string path = Path.Join(ItchSource.App.GameDir, "Itch", Name.StripIllegalFsChars());
        string filename = upload.Filename;
        Size = upload.Size;
        Download = new(url, path, filename);
        OnUpdate?.Invoke();

        try
        {
            await Download.Download();
        }
        catch
        {
            Download = null;
            OnUpdate?.Invoke();
            return;
        }

        Download.Line1 = "Getting size...";
        Download.InvokeOnUpdate();
        Size = await Task.Run(() => Utils.DirSize(new(path)));

        Download.Line1 = "Getting executables...";
        Download.InvokeOnUpdate();
        var archive = await ItchApiScannedArchive.Get(ItchSource.Profile!, this, upload);

        if (archive == null)
        {
            // TODO: Handle gracefully
            throw new Exception("Failed to get executables");
        }
        
        if (archive.Targets == null)
        {
            while (archive.Targets == null)
            {
                await Task.Delay(5000);
                archive = await ItchApiScannedArchive.Get(ItchSource.Profile!, this, upload);
            }
        }
        
        InstallPath = path;
        PreferredTarget = -1;
        Targets = archive.Targets;
        InstalledStatus = InstalledStatus.Installed;
        Download = null;
        OnUpdate?.Invoke();
        ItchSource.AddToInstalled(this);
    }

    public async Task UninstallGame()
    {
        if (InstalledStatus == InstalledStatus.NotInstalled)
            throw new Exception("Not installed");

        InstalledStatus = InstalledStatus.NotInstalled;
        await Task.Run(() =>
        {
            if (Directory.Exists(InstallPath!)) Directory.Delete(InstallPath!, true);
        });
    }

    // TODO: implement args
    public void Play()
    {
        if (PreferredTarget < 0 && Targets.Count == 1)
            PreferredTarget = 0;

        if (PreferredTarget < 0 || PreferredTarget >= Targets.Count)
        {
            new GameOptionsGui(this).ShowGui("Current preferred boot entry is invalid, please reconfigure");
            return;
        }

        ItchApiLaunchTarget target = Targets[PreferredTarget];
        string path = Path.Join(InstallPath, target.Path);

        LaunchParams args = new(path, CommandlineArgs, Path.GetDirectoryName(path), this, target.GetPlatform());
        ItchSource.App.Launch(args);
    }
    
    public async Task<byte[]?> CoverImage()
    {
        if (CoverUri == null)
            return null;
        
        string cachePath = Path.Join(ItchGameSource.IMAGECACHEDIR, CoverUri.AbsoluteUri.Split("/").Last());

        if (File.Exists(cachePath))
            return await File.ReadAllBytesAsync(cachePath);

        using HttpClient client = new();
        try
        {
            HttpResponseMessage response = await client.GetAsync(CoverUri);
            response.EnsureSuccessStatusCode();
            byte[] bytes = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(cachePath, bytes);
            return bytes;
        }
        catch
        {
            return null;
        }
    }

    public async Task<byte[]?> BackgroundImage() => null;
    public void InvokeOnUpdate() => OnUpdate?.Invoke();
    public Task<ItchApiGameUploads?> GetUploads() => ItchApiGameUploads.Get(ItchSource.Profile!, this);
}