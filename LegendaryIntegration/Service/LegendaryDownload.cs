using System.Globalization;
using LauncherGamePlugin;
using LegendaryIntegration.Extensions;

namespace LegendaryIntegration.Service;

public class LegendaryDownload : ProgressStatus
{
    public event Action<LegendaryDownload> OnCompletionOrCancel;
    public event Action<LegendaryDownload> OnPauseOrContinue;
    public LegendaryGame Game { get; set; }
    public bool Repair { get; }
    public bool Active => _terminal.IsActive;
    private Terminal _terminal = new(LegendaryGameSource.Source.App);
    private string _path;
    private string _line1 = "";
    private string _line2 = "";

    public override string Line1 => _line1;
    public override string Line2 => _line2;

    public LegendaryDownload(LegendaryGame game, bool repair = false)
    {
        Game = game;
        Repair = repair;
        if (Game.Download != null)
            throw new Exception("Game already has a download active");

        if (!Repair)
        {
            if (Game.IsInstalled && !Game.UpdateAvailable)
                throw new Exception("Game is installed and up to date?");

            _path = Path.Join(LegendaryGameSource.Source.App.GameDir, "legendary", Game.InternalName);
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
        }
        
        _terminal.OnNewErrLine += DownloadTracker;
        _terminal.OnNewLine += VerifyTracker;
    }
    
    public void DownloadTracker(Terminal t, string last)
    {
        if (last.StartsWith("[cli] INFO: Download size: "))
            _line1 = $"Download: {last.Substring(27).Split('(')[0].Trim()}";

        else if (last.StartsWith("[DLManager] INFO: = Progress: "))
        {
            last = last.Substring(30);
            string[] temp = last.Split('%');
            double a = 0;
            double.TryParse(temp[0].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out a);
            Percentage = a;
            temp = last.Split(',');
            _line2 = $"Remaining: {temp[2].Substring(6)}";
        }
        else return;

        InvokeOnUpdate();
    }

    public void VerifyTracker(Terminal t, string last)
    {
        if (last.StartsWith("Verification progress:"))
        {
            string sub = last.Substring(23);
            string[] split = sub.Split(' ');
            _line1 = $"Verifying: {split[0]} files";
            string percentageNumber = split[1].Substring(1, split[1].Length - 3);
            double a = 0;
            double.TryParse(percentageNumber.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out a);
            Percentage = a;
            _line2 = $"@ {split[2].Substring(1)} MiB/s";
        }
        else return;
        
        InvokeOnUpdate();
    }
    
    public async void Start()
    {
        Game.Parser.PauseAllDownloads();
        OnPauseOrContinue?.Invoke(this);
        if (Repair)
            await _terminal.ExecLegendary($"-y repair {Game.InternalName}");
        else
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