﻿using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;

namespace RemoteDownloaderPlugin.Game;

public class OnlineGame : IGame
{
    public string InternalName => Entry.GameId;
    public string Name => $"{Entry.GameName} ({Platform})";
    public bool IsRunning { get; set; } = false;
    public IGameSource Source => _plugin;
    public long? Size => Entry.GameSize;
    public string Platform { get; private set; }

    public bool HasImage(ImageType type)
        => ImageTypeToUri(type) != null;
    
    public Task<byte[]?> GetImage(ImageType type)
    {
        Uri? url = ImageTypeToUri(type);

        if (url == null)
            return Task.FromResult<byte[]?>(null);

        return Storage.Cache($"{Entry.GameId}_{type}", () => Storage.ImageDownload(url));
    }

    public InstalledStatus InstalledStatus => InstalledStatus.NotInstalled;
    public Platform EstimatedGamePlatform => LauncherGamePlugin.Enums.Platform.Unknown;
    public ProgressStatus? ProgressStatus => _download;
    public event Action? OnUpdate;
    public void InvokeOnUpdate() => OnUpdate?.Invoke();

    public IEntry Entry { get; }
    private Plugin _plugin;
    private GameDownload? _download = null;

    public OnlineGame(IEntry entry, Plugin plugin)
    {
        Entry = entry;
        _plugin = plugin;
        Platform = (entry is EmuEntry emuEntry)
            ? emuEntry.Emu
            : "Pc";
    }

    public async Task Download()
    {
        _download = new GameDownload(Entry);
        OnUpdate?.Invoke();
        
        try
        {
            await _download.Download(_plugin.App);
        }
        catch
        {
            _download = null;
            OnUpdate?.Invoke();
            return;
        }

        if (_download.Type == GameType.Emu)
        {
            var entry = Entry as EmuEntry;
            _plugin.Storage.Data.EmuGames.Add(new()
            {
                Id = Entry.GameId,
                Name = Entry.GameName,
                Emu = entry!.Emu,
                GameSize = _download.TotalSize,
                Version = _download.Version,
                BaseFilename = _download.BaseFileName,
                Images = Entry.Img
            });
        }
        else
        {
            _plugin.Storage.Data.PcGames.Add(new()
            {
                Id = Entry.GameId,
                Name = Entry.GameName,
                GameSize = _download.TotalSize,
                Version = _download.Version,
                Images = Entry.Img
            });
        }
        
        _plugin.Storage.Save();
        _plugin.App.ReloadGames();
        _download = null;
        OnUpdate?.Invoke();
    }

    public void Stop()
    {
        _download?.Stop();
        _download = null;
    }
    
    private Uri? ImageTypeToUri(ImageType type)
        => type switch
        {
            ImageType.Background => Entry.Img.Background,
            ImageType.VerticalCover => Entry.Img.VerticalCover,
            ImageType.HorizontalCover => Entry.Img.HorizontalCover,
            ImageType.Logo => Entry.Img.Logo,
            ImageType.Icon => Entry.Img.Icon,
            _ => null
        };
}