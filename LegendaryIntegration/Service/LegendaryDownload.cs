using System.Globalization;
using LauncherGamePlugin;
using LegendaryIntegration.Extensions;

namespace LegendaryIntegration.Service;

public class LegendaryDownload : ProgressStatus
{
    public event Action<LegendaryDownload> OnCompletionOrCancel;
    public event Action<LegendaryDownload> OnPauseOrContinue;
    public LegendaryGame Game { get; set; }
    public bool Active => _terminal.IsActive;
    private Terminal _terminal = new(LegendaryGameSource.Source.App);
    private string _path;
    private string _downloadSize = "";
    private string _remainingTime = "";

    public override string Line1 => $"Download: {_downloadSize}";
    public override string Line2 => $"Remaining: {_remainingTime}";

    public LegendaryDownload(LegendaryGame game)
    {
        Game = game;
        if (Game.Download != null)
            throw new Exception("Game already has a download active");

        if (Game.IsInstalled && !Game.UpdateAvailable)
            throw new Exception("Game is installed and up to date?");

        _path = Path.Join(LegendaryGameSource.Source.App.GameDir, "legendary", Game.InternalName);
        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);

        _terminal.OnNewErrLine += DownloadTracker;
    }
    
    public void DownloadTracker(Terminal t, string last)
    {
        if (last.StartsWith("[cli] INFO: Download size: "))
            _downloadSize = last.Substring(27).Split('(')[0].Trim();

        else if (last.StartsWith("[DLManager] INFO: = Progress: "))
        {
            last = last.Substring(30);
            string[] temp = last.Split('%');
            double a = 0;
            double.TryParse(temp[0].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out a);
            Percentage = a;
            temp = last.Split(',');
            _remainingTime = temp[2].Substring(6);
        }
        else return;

        InvokeOnUpdate();
    }
    
    public async void Start()
    {
        Game.Parser.PauseAllDownloads();
        OnPauseOrContinue?.Invoke(this);
        await _terminal.ExecLegendary($"-y install {Game.InternalName} --skip-sdl --game-folder \"{_path}\"");
        if (!_terminal.Killed)
            OnCompletionOrCancel?.Invoke(this);
    }

    public void Pause()
    {
        if (Active)
            _terminal.Kill();

        OnPauseOrContinue?.Invoke(this);
    }
    
    public async void Stop()
    {
        Pause();
        OnCompletionOrCancel?.Invoke(this);
        if (!Game.IsInstalled)
            await Task.Run(() => Directory.Delete(_path, true));
    }
}