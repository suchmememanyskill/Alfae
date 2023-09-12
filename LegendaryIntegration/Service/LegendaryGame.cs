using System.Text;
using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;
using LauncherGamePlugin.Launcher;
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
    [JsonIgnore] public bool IsRunning { get; set; }
    
    public InstalledStatus InstalledStatus => IsInstalled ? InstalledStatus.Installed : InstalledStatus.NotInstalled;
    public Platform EstimatedGamePlatform => Platform.Windows;
    public ProgressStatus? ProgressStatus => Download;

    public bool FromOrigin
    {
        get
        {
            if (Metadata?.Metadata?.CustomAttributes?.ContainsKey("ThirdPartyManagedApp") ?? false)
                return Metadata.Metadata.CustomAttributes["ThirdPartyManagedApp"].Value == "Origin";

            return false;
        }
    }

    public LegendaryDownload? Download { get; set; }
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

    public async Task<byte[]?> GenerateCover()
    {
        MetaImage? banner = GetGameImage("DieselGameBoxTall");
        MetaImage? logo = GetGameImage("DieselGameBoxLogo");
        
        if (banner == null)
            return null;

        if (logo == null)
            return await Storage.ImageDownload(banner.Url);

        byte[]? rawBanner = await Storage.ImageDownload(banner.Url);
        byte[]? rawLogo = await Storage.ImageDownload(logo.Url);

        if (rawBanner == null || rawLogo == null)
            return null;
        
        Image<Rgba32> bannerImg = await Task.Run(() => Image.Load<Rgba32>(rawBanner));
        
        Image<Rgba32> logoImg = await Task.Run(() => Image.Load<Rgba32>(rawLogo));
        Image<Rgba32> output = new Image<Rgba32>(bannerImg.Width, bannerImg.Height);

        // Steam's horizontal height is about 1.5x the vertical height
        float newWidth = bannerImg.Height / 1.5f;
        float newHeight = (newWidth / logoImg.Width) * logoImg.Height;
        await Task.Run(() => logoImg.Mutate(x => x.Resize(new Size((int) newWidth, (int) newHeight))));

        float centerX = bannerImg.Width / 2f;
        float centerY = bannerImg.Height / 2f;
        float logoPosX = centerX - logoImg.Width / 2f;
        float logoPosY = centerY - logoImg.Height / 2f;
        await Task.Run(() => output.Mutate(x => x
            .DrawImage(bannerImg, new Point(0, 0), 1f)
            .DrawImage(logoImg, new Point((int) logoPosX, (int) logoPosY), 1f)
        ));

        bannerImg.Dispose();
        logoImg.Dispose();

        using var ms = new MemoryStream();
        await output.SaveAsync(ms, new JpegEncoder());
        return ms.ToArray();
    }
    
    public Task<byte[]?> CoverImage() => Storage.Cache(GetGameImage("DieselGameBoxTall")?.FileName ?? "", GenerateCover);
    public Task<byte[]?> BackgroundImage() => Storage.Cache(GetGameImage("DieselGameBox")?.FileName ?? "", () => Storage.ImageDownload(GetGameImage("DieselGameBox")?.Url ?? null));
    public Task<byte[]?> LogoImage() => Storage.Cache(GetGameImage("DieselGameBoxLogo")?.FileName ?? "", () => Storage.ImageDownload(GetGameImage("DieselGameBoxLogo")?.Url ?? null));

    public async Task<LegendaryInfoResponse?> GetInfo()
    {
        if (localInfo != null)
            return localInfo;
        
        if (Parser.Auth.OfflineLogin)
            throw new Exception("Cannot get info while offline");

        Terminal t = new(LegendaryGameSource.Source.App);
        await t.ExecLegendary($"info {InternalName} --json");

        if (t.ExitCode == 0)
        {
            localInfo = JsonConvert.DeserializeObject<LegendaryInfoResponse>(t.StdOut.First());
            _localSize = localInfo!.Manifest.DiskSize;
        }

        return localInfo;
    }

    public async Task StartDownload(LegendaryStatusType type = LegendaryStatusType.Download, string? path = null, List<string>? tags = null)
    {
        await GetInfo();
        
        try
        {
            ReattachDownload(new(this, type, path, tags));
            Parser.AddDownload(Download!);
        }
        catch
        {
        }
    }

    public void ReattachDownload(LegendaryDownload download)
    {
        download.Game = this;
        Download = download;
        Download.OnCompletionOrCancel += _ =>
        {
            Download = null;
            InvokeOnUpdate();
        };
        Download.OnPauseOrContinue += _ => InvokeOnUpdate();
    }

    public async Task<LaunchParams?> Launch(bool ignoreUpdate = false)
    {
        Terminal t = new(LegendaryGameSource.Source.App);

        if (FromOrigin)
        {
           return new(LegendaryAuth.LegendaryPath, $"launch --origin {InternalName}", ".", this, Platform.Windows);
        }

        if (!IsInstalled)
            throw new Exception("Game is not installed");

        List<string> args = new() {"--json"};
        
        if (PlatformExtensions.CurrentPlatform == Platform.Linux)
            args.Add("--no-wine");
        
        bool offline = false;
        bool skipUpdateCheck = false;

        if (Parser.Auth.OfflineLogin || ConfigAlwaysOffline)
        {
            offline = true;
            args.Add("--offline");
        }

        if (!offline && (ConfigAlwaysSkipUpdateCheck || ignoreUpdate))
        {
            skipUpdateCheck = true;
            args.Add("--skip-version-check");
        }

        if (UpdateAvailable && !(skipUpdateCheck || offline))
            throw new Exception("Game has an update available");

        await t.ExecLegendary($"launch {InternalName} {string.Join(" ", args)} {ConfigAdditionalGameArgs}");
        
        if (t.ExitCode == 0)
        {
            LegendaryGameSource.Source.Log($"Launch returned {t.StdOut.First()}");
            LaunchDryRun data = JsonConvert.DeserializeObject<LaunchDryRun>(t.StdOut.First())!;
            LaunchParams launchParams = data.toLaunch(this);
            return launchParams;
        }
        
        return null;
    }

    public async Task Import(string path)
    {
        if (Parser.Auth.OfflineLogin)
            throw new Exception("You need to be online to import a game");
        
        Terminal t = new(LegendaryGameSource.Source.App);
        await t.ExecLegendary($"import {InternalName} \"{path}\"");

        if (!t.StdErr.Last().EndsWith("has been imported."))
            throw new Exception("Failed to import game");
        
        await Repair();
        LegendaryGameSource.Source.App.ReloadGames(); // Invalid state, reloading all
    }

    public async Task Repair()
    {
        if (Parser.Auth.OfflineLogin)
            throw new Exception("You need to be online to repair a game");

        await StartDownload(LegendaryStatusType.Repair);
    }

    public async Task Move(string dstDir)
    {
        await StartDownload(LegendaryStatusType.Move, dstDir);
        Download!.OnCompletionOrCancel += _ => LegendaryGameSource.Source.App.ReloadGames();
    }

    public async Task Uninstall()
    {
        if (!IsInstalled)
            throw new Exception("Game is not installed");
        
        Terminal t = new(LegendaryGameSource.Source.App);
        await t.ExecLegendary($"uninstall {InternalName} -y");
    }

    public async void ShowInBrowser()
    {
        if (Metadata == null || Metadata.Metadata == null || Metadata.Metadata.Namespace == null)
            return;

        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpContent content = new StringContent("{\"query\":\"{Catalog{catalogOffers( namespace:\\\"" +
                                                        Metadata.Metadata.Namespace +
                                                        "\\\"){elements {productSlug}}}}\"}", Encoding.Default, "application/json");
                HttpResponseMessage response = await client.PostAsync(new Uri("https://www.epicgames.com/graphql"), content);
                response.EnsureSuccessStatusCode();
                string textResponse = await response.Content.ReadAsStringAsync();
                    
                EpicProductSlugResponse parsedResponse = JsonConvert.DeserializeObject<EpicProductSlugResponse>(textResponse);
                Element slug = parsedResponse?.Data?.Catalog?.CatalogOffers?.Elements?.FirstOrDefault(x => x?.ProductSlug != null);
                if (slug == null)
                    return;
                
                Utils.OpenUrl($"https://store.epicgames.com/en-US/p/{slug.ProductSlug.Split("/").First()}");
            }
        }
        catch
        {
        }
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
    
    public bool ConfigAlwaysOffline { get => GetConfigItem().AlwaysOffline; set { ConfigItem item = GetConfigItem(); item.AlwaysOffline = value; SetConfigItem(item); } }
    public bool ConfigAlwaysSkipUpdateCheck { get => GetConfigItem().AlwaysSkipUpdate; set { ConfigItem item = GetConfigItem(); item.AlwaysSkipUpdate = value; SetConfigItem(item); } }
    public string ConfigAdditionalGameArgs { get => GetConfigItem().AdditionalArgs; set { ConfigItem item = GetConfigItem(); item.AdditionalArgs = value; SetConfigItem(item); } }
    
    private ConfigItem GetConfigItem()
    {
        if (Parser.Config.GameConfigs.TryGetValue(InternalName, out ConfigItem? item))
            return item;

        return new();
    }

    private void SetConfigItem(ConfigItem item)
    {
        if (Parser.Config.GameConfigs.ContainsKey(InternalName))
            Parser.Config.GameConfigs[InternalName] = item;
        else
            Parser.Config.GameConfigs.Add(InternalName, item);
    }

    public bool HasImage(ImageType type)
    {
        if (type == ImageType.VerticalCover)
            return GetGameImage("DieselGameBoxTall")?.Url != null;
        if (type == ImageType.Background)
            return GetGameImage("DieselGameBox")?.Url != null;
        if (type == ImageType.Logo)
            return GetGameImage("DieselGameBoxLogo")?.Url != null;

        return false;
    }

    public async Task<byte[]?> GetImage(ImageType type)
    {
        if (type == ImageType.VerticalCover)
            return await CoverImage();
        if (type == ImageType.Background)
            return await BackgroundImage();
        if (type == ImageType.Logo)
            return await LogoImage();

        return null;
    }
}