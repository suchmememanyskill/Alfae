﻿using ItchIoIntegration.Gui;
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
    public long? DownloadKeyId { get; set; }
    public List<ItchApiLaunchTarget> Targets { get; set; } = new();
    public int PreferredTarget { get; set; } = -1;
    public string CommandlineArgs { get; set; } = "";
    public Uri? GameUrl { get; set; }
    
    [JsonIgnore] public bool IsRunning { get; set; }
    [JsonIgnore] public ItchGameDownload? Download { get; private set; }
    [JsonIgnore] public InstalledStatus InstalledStatus { get; private set; }
    [JsonIgnore] public Platform EstimatedGamePlatform => EstimateGamePlatform();
    [JsonIgnore] public IGameSource Source => ItchSource;
    [JsonIgnore] public ProgressStatus? ProgressStatus => Download;
    [JsonIgnore] public ItchGameSource ItchSource { get; set; }
    public event Action? OnUpdate;

    public ItchGame()
    {
        InstalledStatus = InstalledStatus.Installed;
    }

    public ItchGame(ItchApiOwnedGameKey key, ItchGameSource itchSource)
        : this(key.Game, itchSource)
    {
        DownloadKeyId = key.DownloadKeyId;
    }

    public ItchGame(ItchApiGame game, ItchGameSource itchSource)
    {
        InstalledStatus = InstalledStatus.NotInstalled;
        ItchSource = itchSource;
        Name = game.Title;
        Id = game.Id;
        CoverUri = game.GetCoverUrl();
        GameUrl = game.Url;
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
            await Download.Download(ItchSource.App);
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
        
        foreach (var itchApiLaunchTarget in archive.Targets)
        {
            if (itchApiLaunchTarget.GetPlatform() == Platform.Linux && PlatformExtensions.CurrentPlatform == Platform.Linux)
            {
                string targetPath = Path.Join(path, itchApiLaunchTarget.Path);
                Terminal t = new(ItchSource.App);
                await t.Exec("chmod", $"u+x \"{targetPath}\"");
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

    private ItchApiLaunchTarget? GetLaunchTarget()
    {
        if (PreferredTarget < 0 && Targets.Count == 1)
            PreferredTarget = 0;
        
        if (Targets.Count > 1 && Targets.Count(x => !ItchSource.IgnoredExecutables.Any(y => x.Path.Contains(y))) == 1)
        {
            ItchApiLaunchTarget target = Targets.Find(x => !ItchSource.IgnoredExecutables.Any(y => x.Path.Contains(y)))!;
            PreferredTarget = Targets.IndexOf(target);
        }

        if (PreferredTarget < 0 || PreferredTarget >= Targets.Count)
            return null;
        
        return Targets[PreferredTarget];
    }
    
    public void Play()
    {
        ItchApiLaunchTarget? target = GetLaunchTarget();

        if (target == null)
        {
            new GameOptionsGui(this).ShowGui("Current preferred boot entry is invalid, please reconfigure");
            return;
        }
        
        string path = Path.Join(InstallPath, target.Path);

        LaunchParams args = new(path, CommandlineArgs, Path.GetDirectoryName(path), this, target.GetPlatform());
        ItchSource.App.Launch(args);
    }
    
    public void InvokeOnUpdate() => OnUpdate?.Invoke();
    public Task<ItchApiGameUploads?> GetUploads() => ItchApiGameUploads.Get(ItchSource.Profile!, this);

    public Platform EstimateGamePlatform()
    {
        ItchApiLaunchTarget? target = GetLaunchTarget();

        if (target == null)
            return Platform.Unknown;

        return target.GetPlatform();
    }
    
    public bool HasImage(ImageType type) => (type == ImageType.VerticalCover);
    public async Task<byte[]?> GetImage(ImageType type) => (type == ImageType.VerticalCover) ? await CoverImage() : null;
    public Task<byte[]?> CoverImage() =>
        Storage.Cache(CoverUri?.AbsoluteUri.Split("/").Last() ?? "", () => Storage.ImageDownload(CoverUri));
}