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
    public string? InstallPath { get; set; }
    public List<GogDlTask>? Tasks { get; set; }
    public string ExtraArgs { get; set; } = "";

    [JsonIgnore]
    public string InternalName => Slug;
    [JsonIgnore] 
    public IGameSource Source => GogSource;
    [JsonIgnore] 
    public Platform EstimatedGamePlatform => InstalledPlatform;
    [JsonIgnore]
    public InstalledStatus InstalledStatus =>
        InstalledPlatform != Platform.None ? InstalledStatus.Installed : InstalledStatus.NotInstalled;

    [JsonIgnore] 
    public ProgressStatus? ProgressStatus => DownloadStatus;
    [JsonIgnore]
    public GogDlInfo? DlInfo { get; private set; }
    [JsonIgnore] 
    public GogIntegration GogSource { get; set; }
    [JsonIgnore]
    public GogDlDownload? DownloadStatus { get; private set; }
    public event Action? OnUpdate;

    public GogGame()
    {
    }

    public GogGame(GogIntegration source, GogApiProduct product)
    {
        GogSource = source;
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

    public async Task GetDlInfo()
    {
        if (DlInfo != null)
            return;

        DlInfo = await GogDlInfo.Get((await GogSource.GetAuth())!, this, GogSource.App);

        if (DlInfo != null)
        {
            Size = DlInfo.DiskSize;
            OnUpdate?.Invoke();
        }
    }

    public async Task<bool> Download()
    {
        await GetDlInfo();

        if (DlInfo == null)
            throw new Exception("Failed to get download information for game");

        if (GogDlDownload.ActiveDownload)
            throw new Exception("You can only have one GOG download ongoing at a time");

        DownloadStatus = new();
        OnUpdate?.Invoke();
        await DownloadStatus.Download(GogSource.App, this, (await GogSource.GetAuth())!);
        bool ret = !DownloadStatus.Terminal!.Killed;

        if (ret)
        {
            InstallPath = DownloadStatus.InstallPath;
            InstalledPlatform = DownloadStatus.DownloadedPlatform;
            DownloadStatus = null;
            
            GogDlImport? import = await GogDlImport.Get(GogSource.App, this, (await GogSource.GetAuth())!);

            if (import == null)
            {
                InstallPath = null;
                InstalledPlatform = Platform.None;
                OnUpdate?.Invoke();
                throw new Exception("Could not import downloaded game");
            }
            
            Tasks = import.Tasks;
        }
        
        DownloadStatus = null;
        OnUpdate?.Invoke();
        return ret;
    }

    public void Play()
    {
        if (InstalledStatus == InstalledStatus.NotInstalled)
            throw new Exception("Game is not installed");

        GogDlTask? task = Tasks!.Find(x => x.IsPrimary);
        if (task == null)
            throw new Exception("Game does not have a primary task!");

        GogSource.App.Launch(task.ToLaunchParams(this));
    }

    public void Uninstall()
    {
        if (Directory.Exists(InstallPath!))
            Directory.Delete(InstallPath, true);
        InstallPath = null;
        InstalledPlatform = Platform.None;
        Tasks = null;
        OnUpdate?.Invoke();
    }

    public async Task<byte[]?> BackgroundImage() => null;
}