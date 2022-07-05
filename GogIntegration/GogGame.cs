using GogIntegration.Requests;
using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;
using Newtonsoft.Json;

namespace GogIntegration;

public class GogGame : IGame
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string CoverUrl { get; set; }
    public string PageUrl { get; set; }
    public GogApiWorksOn Platforms { get; set; }
    public Platform InstalledPlatform = Platform.None;
    public long? Size { get; set; } = 0;

    [JsonIgnore]
    public string InternalName => Slug;
    [JsonIgnore]
    public IGameSource Source { get; }
    [JsonIgnore] 
    public Platform EstimatedGamePlatform => InstalledPlatform;
    [JsonIgnore]
    public InstalledStatus InstalledStatus =>
        InstalledPlatform != Platform.None ? InstalledStatus.Installed : InstalledStatus.NotInstalled;
    [JsonIgnore]
    public ProgressStatus? ProgressStatus { get; private set; }
    public event Action? OnUpdate;

    public GogGame(GogIntegration source, GogApiProduct product)
    {
        Source = source;
        Id = product.Id;
        Name = product.Name;
        Slug = product.Slug;
        CoverUrl = product.GetCoverUrl();
        PageUrl = product.GetPageUrl();
        Platforms = product.Platforms;
    }
    
    public async Task<byte[]?> CoverImage()
    {
        string cachePath = Path.Join(GogIntegration.IMAGECACHEDIR, CoverUrl.Split("/").Last());

        if (File.Exists(cachePath))
            return await File.ReadAllBytesAsync(cachePath);

        using HttpClient client = new();
        try
        {
            HttpResponseMessage response = await client.GetAsync(CoverUrl);
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
}