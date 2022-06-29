using ItchIoIntegration.Requests;
using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;

namespace ItchIoIntegration.Service;

public class ItchGame : IGame
{
    public string Name => _key.Game.Title;
    public IGameSource Source { get; }
    public long? Size { get; } = 0;
    public async Task<byte[]?> CoverImage()
    {
        if (_key.Game.GetCoverUrl() == null)
            return null;
        
        string cachePath = Path.Join(IMAGECACHEDIR, _key.Game.GetCoverUrlFilename());

        if (File.Exists(cachePath))
            return await File.ReadAllBytesAsync(cachePath);

        using HttpClient client = new();
        try
        {
            HttpResponseMessage response = await client.GetAsync(_key.Game.GetCoverUrl());
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

    public InstalledStatus InstalledStatus { get; } = InstalledStatus.NotInstalled;
    public ProgressStatus? ProgressStatus { get; } = null;
    public event Action? OnUpdate;

    public bool IsGame => _key.Game.Classification == "game";

    private ItchApiOwnedGameKey _key;

    public ItchGame(ItchApiOwnedGameKey key, ItchGameSource source)
    { 
        _key = key;
        Source = source;
    }
    
    private static string IMAGECACHEDIR
    {
        get
        {
            string path = Path.Join(Path.GetTempPath(), "ItchIoPluginImageCache");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

}