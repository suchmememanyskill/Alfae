using LauncherGamePlugin;
using LauncherGamePlugin.Interfaces;
using LegendaryIntegration.Extensions;
using LegendaryIntegration.Model;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LegendaryIntegration.Service;

public class LegendaryGame : IGame
{
    public InstalledGame? InstalledData { get; set; } = null;
    public GameMetadata? Metadata { get; private set; }
    public LegendaryGameManager Parser { get; private set; }
    public List<LegendaryGame> Dlc { get; private set; } = new();
    public bool IsDlc { get; set; }
    public string InstalledVersion { get => InstalledData.Version; }
    public string AvailableVersion { get {
            if (Metadata.AssetInfo != null)
                return Metadata.AssetInfo.BuildVersion;
            if (Metadata.AssetInfos != null)
                return Metadata.AssetInfos["Windows"].BuildVersion;

            return null;
        }
    }
    public bool UpdateAvailable { get { if (AvailableVersion != null) return InstalledVersion != AvailableVersion; else return false; } }
    public string Developer { get { if (Metadata != null && Metadata.Metadata != null && Metadata.Metadata.Developer != null) return Metadata.Metadata.Developer; else return ""; } }
    public string InstallPath { get => InstalledData.InstallPath; }
    public bool IsInstalled { get => InstalledData != null; }
    public bool HasCloudSave { get { if (Metadata != null && Metadata.Metadata != null && Metadata.Metadata.CustomAttributes != null) return Metadata.Metadata.CustomAttributes.ContainsKey("CloudSaveFolder"); return false; } }
    
    public InstalledStatus InstalledStatus => IsInstalled ? InstalledStatus.Installed : InstalledStatus.NotInstalled;
    public ProgressStatus? ProgressStatus { get; }
    public event Action? OnUpdate;
    public void InvokeOnUpdate() => OnUpdate?.Invoke();
    
    public LegendaryGame(GameMetadata meta, LegendaryGameManager parser)
    {
        Metadata = meta;
        Parser = parser;
    }
    
    public void SetInstalledData(InstalledGame installed) => InstalledData = installed;
    public string Name => Metadata.AppTitle;
    public string InternalName => Metadata.AppName;
    public IGameSource Source => LegendaryGameSource.Source;
    public long? Size
    {
        get
        {
            if (IsInstalled)
                return InstalledData.InstallSize;

            return _localSize;
        }
    }

    private long _localSize = 0;
    private LegendaryInfoResponse? localInfo = null;

    private static string IMAGECACHEDIR
    {
        get
        {
            string path = Path.Join(Path.GetTempPath(), "LegendaryPluginImageCache");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    public async Task<byte[]?> CoverImage()
    {
        MetaImage? banner = GetGameImage("DieselGameBoxTall");
        MetaImage? logo = GetGameImage("DieselGameBoxLogo");
        
        if (banner == null)
            return null;
        
        string cachePath = Path.Join(IMAGECACHEDIR, banner.FileName);

        if (File.Exists(cachePath))
            return await File.ReadAllBytesAsync(cachePath);

        if (logo == null)
        {
            byte[]? bytes = await banner.GetImageAsync();
            if (bytes == null)
                return null;

            await File.WriteAllBytesAsync(cachePath, bytes);
            return bytes;
        }

        byte[]? rawBanner = await banner.GetImageAsync();

        if (rawBanner == null)
            return null;
            
        Image<Rgba32> bannerImg = Image.Load<Rgba32>(rawBanner);

        byte[]? rawLogo = await logo.GetImageAsync();

        if (rawLogo == null)
            return null;

        Image<Rgba32> logoImg = Image.Load<Rgba32>(rawLogo);
        Image<Rgba32> output = new Image<Rgba32>(bannerImg.Width, bannerImg.Height);

        // Steam's horizontal height is about 1.5x the vertical height
        float newWidth = bannerImg.Height / 1.5f;
        float newHeight = (newWidth / logoImg.Width) * logoImg.Height;
        await Task.Run(() => logoImg.Mutate(x => x.Resize(new Size((int) newWidth, (int) newHeight))));

        float centerX = bannerImg.Width / 2;
        float centerY = bannerImg.Height / 2;
        float logoPosX = centerX - logoImg.Width / 2;
        float logoPosY = centerY - logoImg.Height / 2;
        await Task.Run(() => output.Mutate(x => x
            .DrawImage(bannerImg, new Point(0, 0), 1f)
            .DrawImage(logoImg, new Point((int) logoPosX, (int) logoPosY), 1f)
        ));

        bannerImg.Dispose();
        logoImg.Dispose();
        await output.SaveAsync(cachePath, new JpegEncoder());
        return await File.ReadAllBytesAsync(cachePath);
    }

    public async Task<byte[]?> BackgroundImage()
    {
        MetaImage? background = GetGameImage("DieselGameBox");

        if (background == null)
            return null;
        
        string cachePath = Path.Join(IMAGECACHEDIR, background.FileName);
        
        if (File.Exists(cachePath))
            return await File.ReadAllBytesAsync(cachePath);

        byte[]? bytes = await background.GetImageAsync();
        if (bytes == null)
            return null;

        await File.WriteAllBytesAsync(cachePath, bytes);
        return bytes;
    }

    public async Task<LegendaryInfoResponse?> GetInfo()
    {
        if (localInfo != null)
            return localInfo;
        
        if (Parser.Auth.OfflineLogin)
            throw new Exception("Cannot get info while offline");

        Terminal t = new();
        await t.ExecLegendary($"info {InternalName} --json");

        if (t.ExitCode == 0)
        {
            localInfo = JsonConvert.DeserializeObject<LegendaryInfoResponse>(t.StdOut.First());
            _localSize = localInfo!.Manifest.DiskSize;
        }

        return localInfo;
    }
    
    private MetaImage? GetGameImage(string type)
    {
        if (Metadata == null || Metadata.Metadata == null || Metadata.Metadata.KeyImages == null)
            return null;

        if (!Metadata.Metadata.KeyImages.Any(x => x.Type == type))
            return null;
        else
            return Metadata.Metadata.KeyImages.Find(x => x.Type == type);
    }
}