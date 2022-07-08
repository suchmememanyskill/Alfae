using System.Globalization;
using GogIntegration.Extensions;
using GogIntegration.Requests;
using LauncherGamePlugin;
using LauncherGamePlugin.Enums;
using LauncherGamePlugin.Interfaces;
using System.IO;

namespace GogIntegration;

public class GogDlDownload : ProgressStatus
{
    public static bool ActiveDownload { get; private set; } = false;
    public Terminal? Terminal { get; set; }
    public Action? OnCompletionOrCancel;
    public Platform DownloadedPlatform { get; private set; }
    public string InstallPath { get; private set; } = "";
    private GogGame _game;
    
    private void UpdateDownload(Terminal t, string last)
    {
        if (!last.StartsWith("[PROGRESS] INFO: = Progress: "))
            return;

        string percentage = last[29..].Split(" ").First();

        double result = 0;
        if (!double.TryParse(percentage.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            return;

        Percentage = result;
        Line1 = $"Downloading: {Percentage:0}%";
        InvokeOnUpdate();
    }

    public async Task Download(IApp app, GogGame game, GogApiAuth auth)
    {
        InstallPath = Path.Join(app.GameDir, "GOG");
        _game = game;

        if (!Directory.Exists(InstallPath))
            Directory.CreateDirectory(InstallPath);

        if (game.DlInfo == null)
            throw new Exception("No dl info");
        
        Terminal = new(app);
        Terminal.OnNewErrLine += UpdateDownload;
        DownloadedPlatform = game.Platforms.GetIdealPlatform();

        ActiveDownload = true;
        await Terminal.ExecGog(
            $"download {game.Id} --platform {DownloadedPlatform.GetGogDlString()} --path=\"{InstallPath}\" --skip-dlcs --lang={game.DlInfo.UsedLanguage} --token {auth.AccessToken}");
        ActiveDownload = false;
        
        InstallPath = Path.Join(InstallPath, game.DlInfo.FolderName);
        OnCompletionOrCancel?.Invoke();
    }

    public void Stop()
    {
        if (!Terminal!.IsActive)
            return;
        
        if (_game.InstalledStatus == InstalledStatus.NotInstalled)
            OnCompletionOrCancel += () =>
            {
                Directory.Delete(InstallPath, true);
            };
        
        Terminal.Kill();
    }
}